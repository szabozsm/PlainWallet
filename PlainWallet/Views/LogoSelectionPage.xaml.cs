using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using PlainWallet.Services;

namespace PlainWallet.Views;

public partial class LogoSelectionPage : ContentPage
{
    public static event Action<string?>? LogoSelected;

    private List<string> _allLogos = new();

    public LogoSelectionPage()
        : this(null, null, null)
    {
    }

    public LogoSelectionPage(string? initialUri, string? initialUrl, byte[]? InitialLogoData)
    {
        InitializeComponent();
        _allLogos = LogosService.GetBuiltInLogoFileNames().ToList();
        LogosCollection.ItemsSource = _allLogos;

        if (InitialLogoData != null && InitialLogoData.Length > 0)
        {
            try
            {
                UrlPreview.Source = ImageSource.FromStream(() => new MemoryStream(InitialLogoData));
            }
            catch
            {
                UrlPreview.Source = null;
            }
        }
        else
            if (!string.IsNullOrEmpty(initialUrl))
            {
                // set the URL entry and preview
                UrlEntry.Text = initialUrl;
                try
                {
                    UrlPreview.Source = ImageSource.FromUri(new Uri(initialUrl));
                }
                catch
                {
                    UrlPreview.Source = null;
                }
            }
            else
                if (!string.IsNullOrEmpty(initialUri))
                {
                    // set the URL entry and preview
                    UrlEntry.Text = "";
                    try
                    {
                        UrlPreview.Source = ImageSource.FromFile(initialUri);
                    }
                    catch
                    {
                        UrlPreview.Source = null;
                    }
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
                UrlPreview.Source = ImageSource.FromFile(result.FullPath ?? result.FileName);

                LogoSelected?.Invoke(result.FullPath ?? result.FileName);
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
        LogoSelected?.Invoke(null);
        await Navigation.PopAsync();
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection is null || e.CurrentSelection.Count == 0) return;
        var selected = e.CurrentSelection[0]?.ToString();
        if (string.IsNullOrEmpty(selected)) return;
        LogoSelected?.Invoke(selected);
        await Navigation.PopAsync();
    }

    private void OnFilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.Trim().Replace(" ", "_");
        if (string.IsNullOrEmpty(q))
        {
            LogosCollection.ItemsSource = _allLogos;
            return;
        }
        var filtered = _allLogos.Where(s => s.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        LogosCollection.ItemsSource = filtered;
    }

    private void OnUrlTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            UrlPreview.Source = null;
            return;
        }
        if (Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
        {
            try
            {
                UrlPreview.Source = ImageSource.FromUri(uri);
            }
            catch
            {
                UrlPreview.Source = null;
            }
        }
        else
        {
            UrlPreview.Source = null;
        }
    }

    private async void OnUseUrlClicked(object? sender, EventArgs e)
    {
        var url = UrlEntry?.Text?.Trim();
        if (string.IsNullOrEmpty(url))
        {
            await DisplayAlertAsync("Invalid URL", "Please enter a non-empty URL.", "OK");
            return;
        }
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
        {
            LogoSelected?.Invoke(url);
            await Navigation.PopAsync();
            return;
        }
        await DisplayAlertAsync("Invalid URL", "Please enter a valid http or https URL.", "OK");
    }
}
