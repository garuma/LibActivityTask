﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Runtime;

namespace Neteril.Android
{
	public class ActivityScope : IDisposable
	{
		const string BundleIdentityKey = "__magic__xamarin_identity";

		long activityIdentity;
		ActivityScopeListener listener;
		ConcurrentQueue<Action> continuations = new ConcurrentQueue<Action> ();

		public Activity Instance { get; private set; }
		internal bool IsUnavailable { get; private set; }

		ActivityScope (Activity activity)
		{
			Instance = activity;
			activityIdentity = ActivityIdProvider.GetNextId ();
			listener = new ActivityScopeListener ();
			listener.ActivityStateChanged += HandleActivityStateChanged;
			activity.Application.RegisterActivityLifecycleCallbacks (listener);
		}

		public static ActivityScope Of (Activity activity) => new ActivityScope (activity);

		public void Dispose ()
		{
			if (listener != null) {
				listener.ActivityStateChanged -= HandleActivityStateChanged;
				Instance.Application.UnregisterActivityLifecycleCallbacks (listener);
				listener = null;
			}
		}

		public static implicit operator Activity (ActivityScope scope)
		{
			return scope.Instance;
		}

		internal void OnCompleted (Action continuation)
		{
			continuations.Enqueue (continuation);
		}

		void HandleActivityStateChanged (Activity activity, ActivityState newState, Bundle savedData)
		{
			switch (newState) {
			case ActivityState.Created:
				if (activity != null && !savedData.ContainsKey (BundleIdentityKey))
					return;
				var id = savedData.GetLong (BundleIdentityKey);
				if (id == activityIdentity)
					Instance = activity;
				break;
			case ActivityState.SaveInstance:
				if (IsSameActivity (activity))
					savedData.PutLong (BundleIdentityKey, activityIdentity);
				break;
			case ActivityState.Resumed:
			case ActivityState.Started:
				if (IsSameActivity (activity))
					SetAvailability (isAvailable: true);
				break;
			case ActivityState.Stopped:
			case ActivityState.Destroyed:
			case ActivityState.Paused:
				if (IsSameActivity (activity))
					SetAvailability (isAvailable: false);
				break;
			}
		}

		bool IsSameActivity (Activity other) => ReferenceEquals (Instance, other)
		                                        || JNIEnv.IsSameObject (Instance.Handle, other.Handle);

		void SetAvailability (bool isAvailable)
		{
			if (IsUnavailable ^ isAvailable)
				return;
			IsUnavailable = !isAvailable;
			if (isAvailable)
				while (continuations.TryDequeue (out var continuation))
					continuation ();
		}

		enum ActivityState {
			Created,
			Started,
			Resumed,
			Paused,
			SaveInstance,
			Stopped,
			Destroyed
		}

		class ActivityScopeListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
		{
			public event Action<Activity, ActivityState, Bundle> ActivityStateChanged;

			public void OnActivityCreated (Activity activity, Bundle savedInstanceState) => ActivityStateChanged?.Invoke (activity, ActivityState.Created, savedInstanceState);
			public void OnActivitySaveInstanceState (Activity activity, Bundle outState) => ActivityStateChanged?.Invoke (activity, ActivityState.SaveInstance, outState);
			public void OnActivityResumed (Activity activity) => ActivityStateChanged?.Invoke (activity, ActivityState.Resumed, null);
			public void OnActivityStarted (Activity activity) => ActivityStateChanged?.Invoke (activity, ActivityState.Started, null);
			public void OnActivityPaused (Activity activity) => ActivityStateChanged?.Invoke (activity, ActivityState.Paused, null);
			public void OnActivityStopped (Activity activity) => ActivityStateChanged?.Invoke (activity, ActivityState.Stopped, null);
			public void OnActivityDestroyed (Activity activity) => ActivityStateChanged?.Invoke (activity, ActivityState.Destroyed, null);
		}
	}
}
