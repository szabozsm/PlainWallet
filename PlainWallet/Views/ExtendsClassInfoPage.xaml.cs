namespace PlainWallet.Views;

public partial class ExtendsClassInfoPage : ContentPage
{
    public ExtendsClassInfoPage()
    {
        InitializeComponent();
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}