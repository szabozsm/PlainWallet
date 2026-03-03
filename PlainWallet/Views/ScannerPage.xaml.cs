using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using ZXing.Net.Maui;

namespace PlainWallet.Views;

public partial class ScannerPage : ContentPage
{
    private readonly Action<string?>? _onScanned;
    private bool _handling;

    public ScannerPage(Action<string?>? onScanned)
    {
        InitializeComponent();
        cameraView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false
        };

        _onScanned = onScanned;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        cameraView.IsDetecting = true;
        _handling = false;
    }

    protected override void OnDisappearing()
    {
        cameraView.IsDetecting = false;
        base.OnDisappearing();
    }

    private void CameraView_OnDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_handling) return;
        var result = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(result)) return;
        _handling = true;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _onScanned?.Invoke(result);
            await Navigation.PopAsync();
        });
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        cameraView.IsDetecting = false;
        await Navigation.PopAsync();
    }

    private void OnSwitchClicked(object sender, EventArgs e)
    {
        cameraView.CameraLocation = cameraView.CameraLocation == CameraLocation.Rear ? CameraLocation.Front : CameraLocation.Rear;
    }
}
