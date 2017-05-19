using Android.App;
using Android.Widget;
using Android.OS;

using System;
using System.Threading.Tasks;

using Neteril.Android;

namespace ActivityTaskTest
{
	[Activity(Label = "ActivityTaskTest", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		static bool launched = false;

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			if (!launched)
			{
				launched = true;
				using (var scope = ActivityScope.Of(this))
					await DoAsyncStuff(scope);
			}
		}

		TextView MyLabel(Activity activity) => activity.FindViewById<TextView>(Resource.Id.myLabel);

		async ActivityTask DoAsyncStuff(ActivityScope scope)
		{
			await Task.Delay(1000); // Small network call
			MyLabel(scope).Text = "Step 1";
			await Task.Delay(5000); // Big network call
			MyLabel(scope).Text = "Step 2";
		}
	}
}

