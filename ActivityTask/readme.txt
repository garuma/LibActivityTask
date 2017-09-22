
Source is at https://github.com/garuma/LibActivityTask

This small NuGet gives you a way to create asynchronous methods in Android
that handles two things for you:

- Activity instance re-creation
- Activity lifecycle events

The first one is common with configuration changes like the user rotating
the device screen. In this case by default Android will re-create your
activity which means that your async operation previously captured instance
is now likely defunct. This library solves this problem by introducing
the `ActivityScope` class, once initialized with an Activity instance it
becomes a replacement for it that will keep track of the latest incarnation
of your activity.

The second one manifests itself when your activity is put on hold, this can
happen when the user press the home button or if you are launching another
activity. In this case, the library introduce the `ActivityTask` type that
should be used as the return type of your async method (acting like a normal
Task) would. When you do so, the async method builder associated with it will
handle lifecycle events for you (using the previously mentioned `ActivityScope`)
and schedule your await continuations only when the activity is alive. For this
to work, the async method needs to be passed the instance of the `ActivityScope`
from the caller.

Below is a small example of how you can use the library for those two things:

```
using Neteril.Android;

static bool alreadyExecuted = false;

protected override async void OnCreate(Bundle savedInstanceState)
{
	base.OnCreate(savedInstanceState);
	SetContentView(Resource.Layout.Main);

	if (!alreadyExecuted)
	{
		alreadyExecuted = true;
		using (var scope = ActivityScope.Of(this))
			await DoAsyncStuff(scope);
	}
}

TextView MyLabel(Activity activity) => activity.FindViewById<Button>(Resource.Id.myLabel);

async ActivityTask DoAsyncStuff(ActivityScope scope)
{
	await Task.Delay(1000); // Small network call
	MyLabel(scope).Text = "Step 1";
	await Task.Delay(5000); // Big network call
	MyLabel(scope).Text = "Step 2";
}
```