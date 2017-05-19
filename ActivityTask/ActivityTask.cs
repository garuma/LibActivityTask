using System.Runtime.CompilerServices;

namespace Neteril.Android
{
	[AsyncMethodBuilder(typeof(ActivityScopeMethodBuilder))]
	public struct ActivityTask
	{
		public ActivityTaskAwaiter GetAwaiter() => new ActivityTaskAwaiter();
	}
}
