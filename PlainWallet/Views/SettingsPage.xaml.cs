using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using PlainWallet.Data;
using PlainWallet.Models;

namespace PlainWallet.Views;

public partial class SettingsPage : ContentPage
{
    private Settings? _settings;
    private bool _isEditing;

    private readonly IServiceProvider services;
    public SettingsPage()
    {
        this.services = IPlatformApplication.Current.Services;
        InitializeComponent();
        BindingContext = this;
        Title = "Settings";
        LoadSettings();
    }

    public string DummyProperty
    {
        get => _settings?.DummyProperty ?? string.Empty;
        set
        {
            if (_settings != null)
            {
                _settings.DummyProperty = value;
                OnPropertyChanged(nameof(DummyProperty));
            }
        }
    }

    private async void LoadSettings()
    {
        try
        {

            using var innerScope = services.CreateScope();
            using var db = innerScope.ServiceProvider.GetRequiredService<CardDbContext>();

            // Try to get existing settings
            _settings = await db.Settings.FirstOrDefaultAsync();

            if (_settings == null)
            {
                // Create default settings if none exist
                _settings = new Settings();
                db.Settings.Add(_settings);
                await db.SaveChangesAsync();
            }

            _isEditing = true;
            OnPropertyChanged(nameof(DummyProperty));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load settings: {ex.Message}", "OK");
        }
    }

    private async void OnApplyClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_settings == null) return;

            using var innerScope = services.CreateScope();
            using var db = innerScope.ServiceProvider.GetRequiredService<CardDbContext>();

            if (_isEditing)
            {
                db.Settings.Update(_settings);
            }
            else
            {
                db.Settings.Add(_settings);
            }

            await db.SaveChangesAsync();

            await DisplayAlert("Success", "Settings saved successfully!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
