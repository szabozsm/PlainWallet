using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;
using PlainWallet.Data;
using PlainWallet.Views;
using PlainWallet.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PlainWallet;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _services;

    public AppShell()
    {
        _services = IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("MAUI application services are not available.");
        InitializeComponent();
        Routing.RegisterRoute(nameof(CardDetailPage), typeof(CardDetailPage));
        Routing.RegisterRoute(nameof(CardEditorPage), typeof(CardEditorPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        var settingsPage = _services.GetRequiredService<SettingsPage>();
        await Navigation.PushAsync(settingsPage);
        FlyoutIsPresented = false;
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
        catch (Exception exception)
        {
            await DisplayAlertAsync("Export Error", $"Failed to export cards: {exception.Message}", "OK");
        }
        finally
        {
            FlyoutIsPresented = false;
        }
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select JSON file to import",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.json" } },
                    { DevicePlatform.Android, new[] { "application/json", "text/json", "text/plain" } },
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.MacCatalyst, new[] { "json" } }
                })
            });

            if (result is null)
                return;

            await using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var importService = _services.GetRequiredService<ImportService>();
            var cardCount = await importService.ImportData(json);

            if (cardCount > -1)
                await DisplayAlertAsync("Import Success", $"Successfully imported {cardCount} cards.", "OK");
            else
                await DisplayAlertAsync("Import Failed", "No cards were imported. Please check the file and try again.", "OK");
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Import Error", $"Failed to import cards: {exception.Message}", "OK");
        }
        finally
        {
            FlyoutIsPresented = false;
        }
    }

    private async void OnDeleteDatabaseClicked(object? sender, EventArgs e)
    {
        try
        {
            bool answer = await DisplayAlertAsync("Delete", "Are you sure you want to delete ALL CARDS?", "Yes", "No");
            if (answer)
            {
                if (SettingsStore.UseExtendsClass)
                {
                    bool answer2 = await DisplayAlertAsync("Delete", "You are synchronizing your data to the cloud, that will be deleted too. Are you still sure?", "Yes", "No");
                    if (!answer2)
                        return;
                }

                using var innerScope0 = _services.CreateScope();
                using var db = innerScope0.ServiceProvider.GetRequiredService<CardDbContext>();
                await db.Cards.ExecuteDeleteAsync();
                await db.SaveChangesAsync();
                CardStore.Initialize(_services);
            }
        }
        finally
        {
            this.FlyoutIsPresented = false;
        }

    }
}
