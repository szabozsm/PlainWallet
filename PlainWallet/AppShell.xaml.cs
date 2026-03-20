using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;
using PlainWallet.Data;
using PlainWallet.Models;
using PlainWallet.Views;
using System.Text.Json;
using System.Text;
using PlainWallet.Services;
using Xamarin.Google.ErrorProne.Annotations;

namespace PlainWallet;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _services;

    public AppShell()
    {
        _services = IPlatformApplication.Current.Services;
        InitializeComponent();
        Routing.RegisterRoute(nameof(CardDetailPage), typeof(CardDetailPage));
        Routing.RegisterRoute(nameof(CardEditorPage), typeof(CardEditorPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        var settingsPage = _services.GetRequiredService<SettingsPage>();
        await Navigation.PushAsync(settingsPage);
        this.FlyoutIsPresented = false;
    }

    private async void OnBuyMeCoffeeTapped(object? sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri("https://buymeacoffee.com/szabozsm"));
        }
        catch
        {
            await DisplayAlert("Browser Error", "Could not open the coffee page. Please visit: https://buymeacoffee.com/szabozsm", "OK");
        }
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {

        try
        {

            var importService = _services.GetRequiredService<ImportService>();
            var exportData = importService.GetDataToExport();

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var fileName = $"plainwallet_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            await File.WriteAllTextAsync(filePath, json);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export PlainWallet Cards",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export Error", $"Failed to export cards: {ex.Message}", "OK");
        }
        finally
        {
            this.FlyoutIsPresented = false;
        }
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select JSON file to import",
                FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.json" } },
                        { DevicePlatform.Android, new[] { "application/json", "text/json", "text/plain" } },
                        { DevicePlatform.WinUI, new[] { ".json" } },
                        { DevicePlatform.MacCatalyst, new[] { "json" } }
                    })
            });

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var importService = _services.GetRequiredService<ImportService>();

                var cardCount = await importService.ImportData(json);
                if (cardCount > -1)
                {
                    await DisplayAlertAsync("Import Success", $"Successfully imported {cardCount} cards.", "OK");
                }
                else
                {
                    await DisplayAlertAsync("Import Failed", $"No cards were imported. Please check the file and try again.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Import Error", $"Failed to import cards: {ex.Message}", "OK");
        }
        finally
        {
            this.FlyoutIsPresented = false;
        }
    }

}
