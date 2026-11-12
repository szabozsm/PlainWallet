namespace PlainWallet.Views;

public partial class CredentialsQrCodePage : ContentPage
{
    public CredentialsQrCodePage(ImageSource qrCode, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(qrCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        InitializeComponent();
        CredentialsQrImage.Source = qrCode;
        TransferInstructionsLabel.Text = $"Scan this QR code to transfer your {serviceName} settings.";
        SemanticProperties.SetDescription(CredentialsQrImage, $"QR code containing {serviceName} credentials.");
    }
}
