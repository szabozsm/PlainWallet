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
    private readonly bool _focusFilterOnAppearing;
    private bool _filterEntryLoaded;
    private bool _pageAppeared;
    private bool _filterFocusRequested;

    public LogoSelectionPage()
        : this(null, null, null, LogoKind.Builtin)
    {
    }
    private LogoTabViewModel myTabs = new LogoTabViewModel();
    private List<BuiltInLogoOption> _allLogos = new();
    public LogoSelectionPage(
        string? initialUri,
        string? initialUrl,
        byte[]? InitialLogoData,
        LogoKind logoKind,
        string? initialFilter = null,
        bool focusFilterOnAppearing = false)
    {
        InitializeComponent();
        _focusFilterOnAppearing = focusFilterOnAppearing;
        FilterEntry.Loaded += OnFilterEntryLoaded;
        _allLogos = LogosService.GetBuiltInLogoFileNames()
            .Select(fileName => new BuiltInLogoOption(
                fileName,
                LogosService.GetImageSourceForBuiltIn(fileName)!,
                LogosService.IsBuiltInLogoSvg(fileName)))
            .ToList();
        myTabs.Logos = _allLogos.ToList();

        switch (logoKind)
        {
            case LogoKind.Builtin:
                myTabs.CurrentUri = initialUri ?? string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(initialUri))
                    {
                        myTabs.UrlPreviewSource = LogosService.GetImageSourceForBuiltIn(initialUri);
                    }
                }
                catch
                {
                    myTabs.UrlPreviewSource = null;
                }
                break;
            case LogoKind.Web:
                myTabs.CurrentUrl = initialUrl ?? string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(initialUrl) && Uri.TryCreate(initialUrl, UriKind.Absolute, out var uri))
                    {
                        myTabs.UrlPreviewSource = ImageSource.FromUri(uri);
                    }
                }
                catch
                {
                    myTabs.UrlPreviewSource = null;
                }
                break;
            case LogoKind.File:
                try
                {
                    if (InitialLogoData is not null)
                    {
                        myTabs.FilePreviewSource = ImageSource.FromStream(() => new MemoryStream(InitialLogoData));
                    }
                }
                catch
                {
                    myTabs.FilePreviewSource = null;
                }
                break;
        }

        tabView.BindingContext = myTabs;
        if (initialFilter is not null)
            FilterEntry.Text = initialFilter;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _pageAppeared = true;
        TryFocusFilter();
    }

    private void OnFilterEntryLoaded(object? sender, EventArgs e)
    {
        _filterEntryLoaded = true;
        TryFocusFilter();
    }

    private void TryFocusFilter()
    {
        if (!_focusFilterOnAppearing || !_pageAppeared || !_filterEntryLoaded || _filterFocusRequested)
            return;

        _filterFocusRequested = true;
        Dispatcher.Dispatch(() =>
        {
            FilterEntry.Focus();
            FilterEntry.CursorPosition = FilterEntry.Text?.Length ?? 0;
            FilterEntry.SelectionLength = 0;
        });
    }

    private async void OnDeleteLogoClicked(object? sender, EventArgs e)
    {
        LogoSelected?.Invoke(null, LogoKind.None);
        await Navigation.PopAsync();
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
                var selectedPath = result.FullPath ?? result.FileName;
                myTabs.FilePreviewSource = ImageSource.FromFile(selectedPath);
                LogoSelected?.Invoke(selectedPath, LogoKind.File);
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
        if (e.CurrentSelection[0] is not BuiltInLogoOption selected) return;
        LogoSelected?.Invoke(selected.FileName, LogoKind.Builtin);
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
        var filtered = _allLogos.Where(logo => logo.FileName.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
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
                        var SelectedLogoData = await MembershipCard.DownloadSvgAsPngAsync(text);
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
