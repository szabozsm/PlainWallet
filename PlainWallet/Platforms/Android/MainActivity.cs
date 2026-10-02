using Android.App;
using Android.Content.PM;
using Android.OS;
using PlainWallet.Services;

namespace PlainWallet;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	private static MainActivity? _current;
	private static int _errorDialogShown;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		_current = this;
		base.OnCreate(savedInstanceState);
	}

	protected override void OnDestroy()
	{
		if (ReferenceEquals(_current, this))
			_current = null;

		base.OnDestroy();
	}

	internal static void ShowUnhandledException(Exception exception)
	{
		var activity = _current;
		if (activity is null)
		{
			ExceptionReporter.Report(exception, "Unhandled application error");
			return;
		}

		var currentActivity = activity!;
		if (Interlocked.Exchange(ref _errorDialogShown, 1) != 0)
			return;

		currentActivity.RunOnUiThread(() =>
		{
			try
			{
				var builder = new AlertDialog.Builder(currentActivity);
				builder.SetTitle("Application error");
				builder.SetMessage(exception.ToString());
				builder.SetPositiveButton("Close", (_, _) => currentActivity.Finish());
				builder.SetCancelable(false);
				builder.Show();
			}
			catch
			{
				ExceptionReporter.Report(exception, "Unhandled application error");
			}
		});
	}
}
