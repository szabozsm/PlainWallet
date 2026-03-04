using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;
using PlainWallet.Services.Data;

namespace PlainWallet.Views;

public partial class CardDetailPage : ContentPage
{
    private MembershipCard? _card;
    private readonly CardDbContext db;

    public MembershipCard? Card
    {
        get => _card;
        set
        {
            _card = value;
            OnPropertyChanged();
        }
    }

    public CardDetailPage(CardDbContext db)
    {
        InitializeComponent();
        BindingContext = this;
        this.db = db;
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

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (Card is null) return;
        await Navigation.PushAsync(new CardEditorPage(Card, db));
    }
}

