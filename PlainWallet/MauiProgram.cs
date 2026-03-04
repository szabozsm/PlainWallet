using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls;
using Microsoft.EntityFrameworkCore;
using PlainWallet.Services.Data;
using PlainWallet.Services;
using Microsoft.Maui.Storage;
using System.IO;

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
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // configure SQLite for DbContext
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cards.db");
		builder.Services.AddDbContext<CardDbContext>(options =>
		{
			options.UseSqlite($"Data Source={dbPath}");
			
		});

		var app = builder.Build();

		return app;
	}
}
