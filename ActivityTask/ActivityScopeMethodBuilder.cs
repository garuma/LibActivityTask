using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Neteril.Android
{
	class ActivityScopeMethodBuilder
	{
		internal static ActivityScope CurrentScope { get; set; }

		IAsyncStateMachine stateMachine;
		SynchronizationContext syncContext;

		public static ActivityScopeMethodBuilder Create () => new ActivityScopeMethodBuilder (SynchronizationContext.Current);

		ActivityScopeMethodBuilder (SynchronizationContext synchronizationContext)
		{
			this.syncContext = synchronizationContext;
			if (syncContext != null)
				syncContext.OperationStarted ();
		}

		public void Start<TStateMachine> (ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
		{
			stateMachine.MoveNext ();
		}

		public ActivityTask Task => new ActivityTask ();

		public void SetResult ()
		{
			if (syncContext != null)
				syncContext.OperationCompleted ();
			CurrentScope.Dispose ();
			CurrentScope = null;
		}

		public void SetException (Exception ex)
		{
			if (syncContext != null)
				syncContext.OperationCompleted ();
		}

		public void SetStateMachine (IAsyncStateMachine stateMachine)
		{
			this.stateMachine = stateMachine;
		}

		public void AwaitOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
		   where TAwaiter : INotifyCompletion
		   where TStateMachine : IAsyncStateMachine
		{
			var callback = GetCompletionAction<TStateMachine> (ref stateMachine);
			awaiter.OnCompleted (callback);
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
		   where TAwaiter : ICriticalNotifyCompletion
		   where TStateMachine : IAsyncStateMachine
		{
			AwaitOnCompleted (ref awaiter, ref stateMachine);
		}

		Action GetCompletionAction<TStateMachine> (ref TStateMachine machine) where TStateMachine : IAsyncStateMachine
		{
			// If this is our first await, such that we've not yet boxed the state machine, do so now.
			if (stateMachine == null) {
				stateMachine = (IAsyncStateMachine)machine;
				stateMachine.SetStateMachine (stateMachine);
			}
			var runner = new Runner (stateMachine, CurrentScope);
			return new Action (runner.Run);
		}

		sealed class Runner
		{
			IAsyncStateMachine machine;
			ActivityScope scope;

			internal Runner (IAsyncStateMachine machine, ActivityScope scope)
			{
				this.machine = machine;
				this.scope = scope;
			}

			public void Run ()
			{
				if (!scope.IsUnavailable)
					machine.MoveNext ();
				else
					scope.OnCompleted (Run);
			}
		}
	}
}
