namespace PlainWallet;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var startupException = Services.ExceptionReporter.TakePending();
		if (startupException is { } pending)
			return new Window(new StartupErrorPage(pending.Title, pending.Exception));

		try
		{
			return new Window(new AppShell());
		}
		catch (Exception exception)
		{
			return new Window(new StartupErrorPage("Startup error", exception));
		}
	}

	private sealed class StartupErrorPage : ContentPage
	{
		private readonly string _title;
		private readonly Exception _exception;
		private bool _alertShown;

		public StartupErrorPage(string title, Exception exception)
		{
			_title = title;
			_exception = exception;
			Title = title;
			Content = new ScrollView
			{
				Content = new Label
				{
					Text = exception.ToString(),
					Padding = 16
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (_alertShown)
				return;

			_alertShown = true;
			try
			{
				await DisplayAlertAsync(_title, _exception.ToString(), "OK");
			}
			catch
			{
			}
		}
	}
}