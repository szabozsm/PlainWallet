using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;

namespace PlainWallet.Views;

public partial class CardDetailPage : ContentPage
{
    private MembershipCard? _card;

    public MembershipCard? Card
    {
        get => _card;
        set
        {
            _card = value;
            OnPropertyChanged();
        }
    }

    public CardDetailPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Card = CardNavigation.SelectedCard;
        UpdateBarcode();
    }

    private void UpdateBarcode()
    {
        if (BarcodeImage is not null && Card is not null)
            BarcodeImage.Source = BarcodeGenerator.CreateBarcode(Card.CardNumber, Card.BarcodeType);
    }
}

