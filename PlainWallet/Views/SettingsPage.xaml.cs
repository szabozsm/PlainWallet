using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;
using Xamarin.Google.Crypto.Tink.Shaded.Protobuf;

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
    }

    public bool UseExtendsClass
    {
        get => SettingsStore.UseExtendsClass;
        set => SettingsStore.UseExtendsClass = value;
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
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load settings: {ex.Message}", "OK");
        }
    }

    private async void OnApplyClicked(object? sender, EventArgs e)
    {
        try
        {
            await SettingsStore.SaveAsync();
            //    await DisplayAlert("Success", "Settings saved successfully!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
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

}
