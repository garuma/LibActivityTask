using System;
using System.Collections.Concurrent;
using System.Threading;
using Android.App;
using Android.OS;

namespace Neteril.Android
{
	public class ActivityScope : Java.Lang.Object, Application.IActivityLifecycleCallbacks
	{
		const string BundleIdentityKey = "__magic__xamarin_identity";

		long activityIdentity;
		ConcurrentQueue<Action> continuations = new ConcurrentQueue<Action> ();

		public Activity Instance { get; private set; }
		internal bool IsUnavailable { get; private set; }

		ActivityScope (Activity activity)
		{
			Instance = activity;
			activityIdentity = ActivityScopeIdProvider.GetNextId ();
			activity.Application.RegisterActivityLifecycleCallbacks (this);
		}

		public static ActivityScope Of (Activity activity)
		{
			var scope = new ActivityScope (activity);
			ActivityScopeMethodBuilder.CurrentScope = scope;
			return scope;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				Instance.Application.UnregisterActivityLifecycleCallbacks (this);
		}

		public static implicit operator Activity (ActivityScope scope)
		{
			return scope.Instance;
		}

		internal void OnCompleted (Action continuation)
		{
			continuations.Enqueue (continuation);
		}

		public void OnActivityCreated (Activity activity, Bundle savedInstanceState)
		{
			if (activity != null && !savedInstanceState.ContainsKey (BundleIdentityKey))
				return;
			var id = savedInstanceState.GetLong (BundleIdentityKey);
			if (id == activityIdentity)
				Instance = activity;
		}

		public void OnActivitySaveInstanceState (Activity activity, Bundle outState)
		{
			if (ReferenceEquals (activity, Instance))
				outState.PutLong (BundleIdentityKey, activityIdentity);
		}

		public void OnActivityResumed (Activity activity)
		{
			if (ReferenceEquals (activity, Instance))
				SetAvailability (isAvailable: true);
		}

		public void OnActivityStarted (Activity activity)
		{
			if (ReferenceEquals (activity, Instance))
				SetAvailability (isAvailable: true);
		}

		public void OnActivityPaused (Activity activity)
		{
			if (ReferenceEquals (activity, Instance))
				SetAvailability (isAvailable: false);
		}

		public void OnActivityStopped (Activity activity)
		{
			if (ReferenceEquals (activity, Instance))
				SetAvailability (isAvailable: false);
		}

		public void OnActivityDestroyed (Activity activity)
		{
			if (ReferenceEquals (activity, Instance))
				SetAvailability (isAvailable: false);
		}

		void SetAvailability (bool isAvailable)
		{
			if (IsUnavailable ^ isAvailable)
				return;
			IsUnavailable = !isAvailable;
			if (isAvailable)
				while (continuations.TryDequeue (out var continuation))
					continuation ();
		}
	}

	static class ActivityScopeIdProvider
	{
		static long IdentityTagPool;

		public static long GetNextId () => Interlocked.Increment (ref IdentityTagPool);
	}
}
