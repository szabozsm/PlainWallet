using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls;
using Microsoft.EntityFrameworkCore;
using PlainWallet.Data;
using PlainWallet.Services;
using Microsoft.Maui.Storage;
using System.IO;
using UraniumUI;

namespace PlainWallet;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.UseSkiaSharp()
			.UseUraniumUI()
			.UseUraniumUIMaterial()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// configure SQLite DB path in app data
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cards.db");
		builder.Services.AddDbContext<CardDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Initialize CardStore from database
		CardStore.Initialize(app.Services);

		return app;
	}
}
