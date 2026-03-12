using Microsoft.Extensions.DependencyInjection;
using PlainWallet.Views;

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
}
