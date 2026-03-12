using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;
using PlainWallet.Data;
using PlainWallet.Models;
using PlainWallet.Views;
using System.Text.Json;
using System.Text;
using PlainWallet.Services;

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

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        try
        {

            using var innerScope = _services.CreateScope();
            using var db = innerScope.ServiceProvider.GetRequiredService<CardDbContext>();
            var cards = db.Cards.ToList();

            var exportData = new
            {
                Version = "1.0",
                ExportDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Cards = cards
            };

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

                var importData = JsonSerializer.Deserialize<ImportData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (importData?.Cards != null)
                {
                    using var innerScope0 = _services.CreateScope();
                    using var db = innerScope0.ServiceProvider.GetRequiredService<CardDbContext>();

                    foreach (var card in importData.Cards)
                    {
                        var tracked = db.Cards.Local.FirstOrDefault(x => x.Id == card.Id) ?? db.Cards.Find(card.Id);
                        if (tracked == null)
                        {
                            db.Cards.Attach(card);
                            db.Entry(card).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        }
                        else
                        {
                            db.Entry(tracked).CurrentValues.SetValues(card);
                        }
                    }
                    
                    await db.SaveChangesAsync();
                    await DisplayAlert("Import Success", $"Successfully imported {importData.Cards.Count} cards.", "OK");
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

    private class ImportData
    {
        public List<MembershipCard>? Cards { get; set; }
    }
}
