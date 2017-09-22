using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Neteril.Android
{
	[AsyncMethodBuilder (typeof (ActivityScopeMethodBuilder))]
	public class ActivityTask
	{
		TaskCompletionSource<VoidTaskResult> completion;

		internal TaskCompletionSource<VoidTaskResult> Completion => completion ?? (completion = new TaskCompletionSource<VoidTaskResult> ());
		internal Task CompletionTask => Completion.Task;

		public TaskAwaiter GetAwaiter ()
		{
			return CompletionTask.GetAwaiter ();
		}

		public ConfiguredTaskAwaitable ConfigureAwait (bool continueOnCapturedContext)
		{
			return CompletionTask.ConfigureAwait (continueOnCapturedContext);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public static ActivityScopeMethodBuilder CreateAsyncMethodBuilder () => ActivityScopeMethodBuilder.Create ();
	}

	struct VoidTaskResult { };
}
