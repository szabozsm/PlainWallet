using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;

namespace PlainWallet.Views;

public partial class SettingsPage : ContentPage
{

    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = this;
        Title = "Settings";
        LoadSettings();
    }

    public string DummyProperty
    {
        get => SettingsStore.DummyProperty;
        set => SettingsStore.DummyProperty = value;
    }

    public string Apikey
    {
        get => SettingsStore.Apikey;
        set => SettingsStore.Apikey = value;
    }

    public string SecurityKey
    {
        get => SettingsStore.SecurityKey;
        set => SettingsStore.SecurityKey = value;
    }

    public string BucketId
    {
        get => SettingsStore.BucketId;
        set => SettingsStore.BucketId = value;
    }

    public bool UseExtendsClass
    {
        get => SettingsStore.UseExtendsClass;
        set
        {
            SettingsStore.UseExtendsClass = value;
            OnPropertyChanged(nameof(UseJsonBin));
        }
    }

    public bool UseJsonBin
    {
        get => SettingsStore.UseJsonBin;
        set
        {
            SettingsStore.UseJsonBin = value;
            OnPropertyChanged(nameof(UseExtendsClass));
        }
    }

    public bool WifiOnlyForExtendsClass
    {
        get => SettingsStore.WifiOnlyForExtendsClass;
        set => SettingsStore.WifiOnlyForExtendsClass = value;
    }

    public bool TwoColumnMode
    {
        get => SettingsStore.TwoColumnMode;
        set => SettingsStore.TwoColumnMode = value;
    }

    public string JsonBinAccessKey
    {
        get => SettingsStore.JsonBinAccessKey;
        set => SettingsStore.JsonBinAccessKey = value;
    }

    public string JsonBinBinId
    {
        get => SettingsStore.JsonBinBinId;
        set => SettingsStore.JsonBinBinId = value;
    }

    private void LoadSettings()
    {
        try
        {
            // SettingsStore should be initialized in App startup
            // Just trigger property change to refresh UI
            OnPropertyChanged(nameof(DummyProperty));
            OnPropertyChanged(nameof(Apikey));
            OnPropertyChanged(nameof(SecurityKey));
            OnPropertyChanged(nameof(UseExtendsClass));
            OnPropertyChanged(nameof(WifiOnlyForExtendsClass));
            OnPropertyChanged(nameof(BucketId));
            OnPropertyChanged(nameof(JsonBinAccessKey));
            OnPropertyChanged(nameof(JsonBinBinId));
            OnPropertyChanged(nameof(UseJsonBin));
            OnPropertyChanged(nameof(TwoColumnMode));
        }
        catch (Exception ex)
        {
            _ = DisplayAlertAsync("Error", $"Failed to load settings: {ex.Message}", "OK");
        }
    }

    private async void OnApplyClicked(object? sender, EventArgs e)
    {
        try
        {
            await SettingsStore.SaveAsync();
            await Shell.Current.GoToAsync("..");
            if (UseExtendsClass || UseJsonBin)
            {
                var services = IPlatformApplication.Current?.Services
                    ?? throw new InvalidOperationException("MAUI application services are not available.");
                var importService = services.GetRequiredService<ImportService>();
                await importService.DownloadData();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await SettingsStore.CancelAsync();
        await Shell.Current.GoToAsync("..");
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        // Toggle the IsPassword property
        SecurityKeyEntry.IsPassword = !SecurityKeyEntry.IsPassword;

        // Change the icon source based on the new state
        if (sender is ImageButton imageButton)
        {
            imageButton.Source = SecurityKeyEntry.IsPassword ? "eye_show.svg" : "eye_hide.svg";
        }
    }

    private async void OnShareCredentialsClicked(object? sender, EventArgs e)
    {
        if (!ExtendsClassCredentialsQrCode.HasAllCredentials(Apikey, SecurityKey, BucketId))
        {
            await DisplayAlertAsync(
                "Missing credentials",
                "Enter the API key, security key, and Bin ID before sharing.",
                "OK");
            return;
        }

        var payload = ExtendsClassCredentialsQrCode.Encode(Apikey, SecurityKey, BucketId);
        var qrCode = BarcodeGenerator.CreateBarcode(
            payload,
            ZXing.Net.Maui.BarcodeFormat.QrCode,
            width: 1024,
            height: 1024);
        if (qrCode is null)
        {
            await DisplayAlertAsync("QR code error", "Could not create the credentials QR code.", "OK");
            return;
        }

        await Navigation.PushAsync(new CredentialsQrCodePage(qrCode, "ExtendsClass.com"));
    }

    private async void OnScanCredentialsClicked(object? sender, EventArgs e)
    {
        var scanner = new ScannerPage(async (value, _) =>
        {
            if (!ExtendsClassCredentialsQrCode.TryDecode(value, out var credentials) || credentials is null)
                return false;

            Apikey = credentials.ApiKey;
            SecurityKey = credentials.SecurityKey;
            BucketId = credentials.BinId;
            OnPropertyChanged(nameof(Apikey));
            OnPropertyChanged(nameof(SecurityKey));
            OnPropertyChanged(nameof(BucketId));
            return true;
        }, "Scan a PlainWallet ExtendsClass sharing QR code.");

        await Navigation.PushAsync(scanner);
    }

    private async void OnShareJsonBinCredentialsClicked(object? sender, EventArgs e)
    {
        if (!JsonBinCredentialsQrCode.HasAllCredentials(JsonBinAccessKey, JsonBinBinId))
        {
            await DisplayAlertAsync(
                "Missing credentials",
                "Enter the JSONBin X-Access-Key and Bin ID before sharing.",
                "OK");
            return;
        }

        var payload = JsonBinCredentialsQrCode.Encode(JsonBinAccessKey, JsonBinBinId);
        var qrCode = BarcodeGenerator.CreateBarcode(
            payload,
            ZXing.Net.Maui.BarcodeFormat.QrCode,
            width: 1024,
            height: 1024);
        if (qrCode is null)
        {
            await DisplayAlertAsync("QR code error", "Could not create the credentials QR code.", "OK");
            return;
        }

        await Navigation.PushAsync(new CredentialsQrCodePage(qrCode, "jsonBin.io"));
    }

    private async void OnScanJsonBinCredentialsClicked(object? sender, EventArgs e)
    {
        var scanner = new ScannerPage(async (value, _) =>
        {
            if (!JsonBinCredentialsQrCode.TryDecode(value, out var credentials) || credentials is null)
                return false;

            JsonBinAccessKey = credentials.AccessKey;
            JsonBinBinId = credentials.BinId;
            OnPropertyChanged(nameof(JsonBinAccessKey));
            OnPropertyChanged(nameof(JsonBinBinId));
            return true;
        }, "Scan a PlainWallet JSONBin sharing QR code.");

        await Navigation.PushAsync(scanner);
    }

    private void OnToggleJsonBinAccessKeyVisibility(object sender, EventArgs e)
    {
        JsonBinAccessKeyEntry.IsPassword = !JsonBinAccessKeyEntry.IsPassword;

        if (sender is ImageButton imageButton)
        {
            imageButton.Source = JsonBinAccessKeyEntry.IsPassword ? "eye_show.svg" : "eye_hide.svg";
        }
    }

    private void OnImage1Tapped(object sender, EventArgs e)
    {
        OneColumnRadioButton.IsChecked = true;
    }
    private void OnImage2Tapped(object sender, EventArgs e)
    {
        TwoColumnRadioButton.IsChecked = true;
    }

    private async void OnExtendsClassInfoTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new StorageInfoPage(StorageInfoContent.ExtendsClass));
    }

    private async void OnJsonBinInfoTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new StorageInfoPage(StorageInfoContent.JsonBin));
    }

}
