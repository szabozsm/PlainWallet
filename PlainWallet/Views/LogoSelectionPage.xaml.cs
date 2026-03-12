using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using PlainWallet.Models;
using PlainWallet.Services;
namespace PlainWallet.Views;

public partial class LogoSelectionPage : ContentPage
{
    public static event Action<string?, LogoKind>? LogoSelected;
    public LogoSelectionPage()
        : this(null, null, null, LogoKind.Builtin)
    {
    }
    private LogoTabViewModel myTabs = new LogoTabViewModel();
    private List<string> _allLogos = new();
    public LogoSelectionPage(string? initialUri, string? initialUrl, byte[]? InitialLogoData, LogoKind logoKind)
    {
        InitializeComponent();
        _allLogos = LogosService.GetBuiltInLogoFileNames().ToList();
        myTabs.Logos = _allLogos.ToList();

        switch (logoKind)
        {
            case LogoKind.Builtin:
                myTabs.CurrentUri = initialUri;
                try
                {
                    myTabs.UrlPreviewSource = ImageSource.FromFile(initialUri);
                }
                catch
                {
                    myTabs.UrlPreviewSource = null;
                }
                break;
            case LogoKind.Web:
                myTabs.CurrentUrl = initialUrl;
                try
                {
                    myTabs.UrlPreviewSource = ImageSource.FromUri(new Uri(initialUrl));
                }
                catch
                {
                    myTabs.UrlPreviewSource = null;
                }
                break;
            case LogoKind.File:
                try
                {
                    myTabs.FilePreviewSource = ImageSource.FromStream(() => new MemoryStream(InitialLogoData));
                }
                catch
                {
                    myTabs.FilePreviewSource = null;
                }
                break;
        }

        tabView.BindingContext = myTabs;

        // Select the appropriate tab based on LogoKind
        switch (logoKind)
        {
            case LogoKind.Builtin:
                tabView.SelectedTab = BuiltinTab;
                break;
            case LogoKind.Web:
                tabView.SelectedTab = WebTab;
                break;
            case LogoKind.File:
                tabView.SelectedTab = FileTab;
                break;
        }
    }
    private async void OnBrowseClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Pick an image",
                FileTypes = FilePickerFileType.Images
            });
            if (result is not null)
            {
                // Use the full path returned by the file picker when available, otherwise the filename
                myTabs.FilePreviewSource = ImageSource.FromFile(result.FullPath ?? result.FileName);
                LogoSelected?.Invoke(result.FullPath ?? result.FileName, LogoKind.File);
                await Navigation.PopAsync();
            }
        }
        catch
        {
            // ignore
        }
    }
    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        LogoSelected?.Invoke(null, LogoKind.Builtin);
        await Navigation.PopAsync();
    }
    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection is null || e.CurrentSelection.Count == 0) return;
        var selected = e.CurrentSelection[0]?.ToString();
        if (string.IsNullOrEmpty(selected)) return;
        LogoSelected?.Invoke(selected, LogoKind.Builtin);
        await Navigation.PopAsync();
    }
    private void OnFilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.Trim().Replace(" ", "_");
        if (string.IsNullOrEmpty(q))
        {
            myTabs.Logos = _allLogos;
            return;
        }
        var filtered = _allLogos.Where(s => s.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        myTabs.Logos = filtered;
    }
    private async void OnUrlTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            myTabs.UrlPreviewSource = null;
            myTabs.IsUrlLoading = false;
            return;
        }
        if (Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
        {
            try
            {
                if (text.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    myTabs.IsUrlLoading = true;
                    myTabs.UrlPreviewSource = null;

                    // Load the image asynchronously to show loading state
                    await Task.Run(async () =>
                    {
                        var SelectedLogoData = await MembershipCard.DownloadSvgAsPngAsync(text, 256);
                        var imageSource = ImageSource.FromStream(() => new MemoryStream(SelectedLogoData));
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            myTabs.UrlPreviewSource = imageSource;
                            myTabs.IsUrlLoading = false;
                        });
                    });
                }
                else
                {
                    // Show loading animation
                    myTabs.IsUrlLoading = true;
                    myTabs.UrlPreviewSource = null;

                    // Load the image asynchronously to show loading state
                    await Task.Run(() =>
                    {
                        // This forces the image to load
                        var imageSource = ImageSource.FromUri(uri);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            myTabs.UrlPreviewSource = imageSource;
                            myTabs.IsUrlLoading = false;
                        });
                    });
                }
            }
            catch
            {
                myTabs.UrlPreviewSource = null;
                myTabs.IsUrlLoading = false;
            }
        }
        else
        {
            myTabs.UrlPreviewSource = null;
            myTabs.IsUrlLoading = false;
        }
    }
    private async void OnUseUrlClicked(object? sender, EventArgs e)
    {
        var url = myTabs.CurrentUrl?.Trim();
        if (string.IsNullOrEmpty(url))
        {
            await DisplayAlertAsync("Invalid URL", "Please enter a non-empty URL.", "OK");
            return;
        }
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
        {
            LogoSelected?.Invoke(url, LogoKind.Web);
            await Navigation.PopAsync();
            return;
        }
        await DisplayAlertAsync("Invalid URL", "Please enter a valid http or https URL.", "OK");
    }
}
