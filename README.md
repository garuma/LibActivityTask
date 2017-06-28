# ActivityTask

<a href="https://www.nuget.org/packages/org.neteril.libactivitytask"><img src="https://img.shields.io/nuget/v/org.neteril.LibActivityTask.svg" alt="NuGet" /></a>

This small library gives you a way to create asynchronous methods in Android that handles two things for you:

- Activity instance re-creation
- Activity lifecycle events

The first one is common with configuration changes like the user rotating the device screen. In this case by default Android will re-create your activity which means that your async operation previously captured instance is now likely defunct. The library solves this problem by introducing the `ActivityScope` class, once initialized with an Activity instance it becomes a replacement for it that will keep track of the latest incarnation of your activity.

The second one manifests itself when your activity is put on hold, this can happen when the user presses the home button or if you are launching another activity. In this case, the library introduces the `ActivityTask` type that should be used as the return type of your async method (acting like a normal Task would). When you do so, the async method builder associated with it will handle lifecycle events for you using the `ActivityScope` that is passed as a parameter to the method and schedule your await continuations only when the activity is alive. 

Below is small example (extracted from the test app) of how you can use the library to reap both benefits:

``` csharp
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
