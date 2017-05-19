using System;

namespace Neteril.Android
{
	public struct ActivityTaskAwaiter
	{
		public bool IsCompleted => true;

		public void OnCompleted(Action continuation)
		{
		}

		public void GetResult()
		{
		}
	}
}
