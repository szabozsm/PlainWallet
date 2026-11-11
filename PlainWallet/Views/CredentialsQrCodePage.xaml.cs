namespace PlainWallet.Views;

public partial class CredentialsQrCodePage : ContentPage
{
    public CredentialsQrCodePage(ImageSource qrCode)
    {
        ArgumentNullException.ThrowIfNull(qrCode);
        InitializeComponent();
        CredentialsQrImage.Source = qrCode;
    }
}
