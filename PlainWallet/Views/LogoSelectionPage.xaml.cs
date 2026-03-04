using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using PlainWallet.Services;

namespace PlainWallet.Views;

public partial class LogoSelectionPage : ContentPage
{
    public static event Action<string?>? LogoSelected;

    public LogoSelectionPage()
    {
        InitializeComponent();
        var logos = LogosService.GetBuiltInLogoFileNames().Select(f => f as object).ToList();
         LogosCollection.ItemsSource = logos;
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
                // Use the full path/URI returned by the file picker
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
}
