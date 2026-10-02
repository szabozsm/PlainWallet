using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls;
using Microsoft.EntityFrameworkCore;
using PlainWallet.Data;
using PlainWallet.Services;
using Microsoft.Maui.Storage;
using System.IO;
using UraniumUI;
using CommunityToolkit.Maui;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace PlainWallet;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
		{
			eventArgs.SetObserved();
			ExceptionReporter.Report(eventArgs.Exception, "Background task error");
		};

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseBarcodeReader()
			.UseSkiaSharp()
			.UseUraniumUI()
			.UseUraniumUIMaterial()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		#if MAUI_DEVFLOW
		builder.AddMauiDevFlowAgent();
		#endif

		// configure SQLite DB path in app data
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cards.db");
		builder.Services.AddTransient<IExtendsClassClient, ExtendsClassClient>();
		builder.Services.AddTransient<ImportService>();
		builder.Services.AddDbContext<CardDbContext>(options => options.UseSqlite($"Data Source={dbPath}")
#if DEBUG
		.EnableSensitiveDataLogging()
#endif
		);

		builder.Services.AddTransient<Views.SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		try
		{
			SettingsStore.Initialize(app.Services);
			CardStore.Initialize(app.Services);
		}
		catch (Exception exception)
		{
			ExceptionReporter.Report(exception, "Startup database error");
		}

		return app;
	}
}
