using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using ZXing.Net.Maui;

namespace PlainWallet.Views;

public partial class ScannerPage : ContentPage
{
    private readonly Func<string?, ZXing.Net.Maui.BarcodeFormat, Task<bool>>? _onScanned;
    private bool _handling;

    public ScannerPage(Func<string?, ZXing.Net.Maui.BarcodeFormat, Task<bool>>? onScanned)
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
        var format = e.Results?.FirstOrDefault()?.Format ?? ZXing.Net.Maui.BarcodeFormat.QrCode;
        if (string.IsNullOrEmpty(result)) return;
        _handling = true;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            cameraView.IsDetecting = false;
            var accepted = _onScanned is null || await _onScanned(result, format);
            if (accepted)
            {
                await Navigation.PopAsync();
                return;
            }

            await DisplayAlertAsync(
                "QR code not recognized",
                "Scan a PlainWallet ExtendsClass sharing QR code.",
                "OK");
            _handling = false;
            cameraView.IsDetecting = true;
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
    void OnTorchClicked(object sender, EventArgs e)
    {
        cameraView.IsTorchOn = !cameraView.IsTorchOn;
    }
}
