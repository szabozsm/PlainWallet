using Microsoft.Maui.ApplicationModel;

namespace PlainWallet.Views;

public partial class StorageInfoPage : ContentPage
{
    public StorageInfoPage(string htmlSnippet)
    {
        ArgumentNullException.ThrowIfNull(htmlSnippet);
        InitializeComponent();
        InfoWebView.Source = new HtmlWebViewSource { Html = htmlSnippet };
    }

    private async void OnInfoWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!Uri.TryCreate(e.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return;
        }

        e.Cancel = true;
        try
        {
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Browser Error", $"Could not open the link: {exception.Message}", "OK");
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
