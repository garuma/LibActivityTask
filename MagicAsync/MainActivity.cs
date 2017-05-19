using Android.App;
using Android.Widget;
using Android.OS;

using System;
using System.Threading.Tasks;

using Neteril.Android;

namespace MagicAsync
{
	[Activity(Label = "MagicAsync", MainLauncher = true, Icon = "@mipmap/icon")]
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

		Button MyButton(Activity activity) => activity.FindViewById<Button>(Resource.Id.myButton);

		async ActivityTask DoAsyncStuff(ActivityScope scope)
		{
			await Task.Delay(1000); // Small network call
			MyButton(scope).Text = "Step 1";
			await Task.Delay(5000); // Big network call
			MyButton(scope).Text = "Step 2";
		}
	}
}

