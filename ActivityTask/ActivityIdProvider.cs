using System;
using System.Threading;

namespace Neteril.Android
{
	static class ActivityIdProvider
	{
		static long IdentityTagPool;

		public static long GetNextId () => Interlocked.Increment (ref IdentityTagPool);
	}
}
