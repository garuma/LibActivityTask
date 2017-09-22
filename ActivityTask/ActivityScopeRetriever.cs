using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Neteril.Android
{
    static class ActivityScopeRetriever<TStateMachine> where TStateMachine : IAsyncStateMachine
    {
        delegate ActivityScope GetActivityScopeDelegate (TStateMachine stateMachine);
        static GetActivityScopeDelegate getter;

		internal static ActivityScope GetScopeFromStateMachine (ref TStateMachine stateMachine)
		{
			if (getter != null)
				return getter (stateMachine);
			var field =	typeof (TStateMachine).GetFields ()?.FirstOrDefault (f => f.FieldType == typeof (ActivityScope));
			if (field == null)
				return null;
			var dynamicGetter = new DynamicMethod ("__MagicSpecialGetScope" + typeof (TStateMachine).Name,
			                                       typeof (ActivityScope),
												   new[] { typeof (TStateMachine) },
			                                       restrictedSkipVisibility: true);
			var generator = dynamicGetter.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldfld, field);
			generator.Emit (OpCodes.Ret);
			getter = (GetActivityScopeDelegate)dynamicGetter.CreateDelegate (typeof (GetActivityScopeDelegate));
			return getter (stateMachine);
		}
    }
}