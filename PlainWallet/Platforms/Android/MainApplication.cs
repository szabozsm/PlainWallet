using Android.App;
using Android.Runtime;
using PlainWallet.Services;

namespace PlainWallet;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	public override void OnCreate()
	{
		AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;
		base.OnCreate();
	}

	private void OnUnhandledException(object? sender, RaiseThrowableEventArgs eventArgs)
	{
		eventArgs.Handled = true;
		MainActivity.ShowUnhandledException(eventArgs.Exception);
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
