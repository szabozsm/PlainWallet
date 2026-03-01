using Microsoft.Maui.Controls;
using PlainWallet.Models;

namespace PlainWallet.Views;

[QueryProperty(nameof(Card), "Card")]
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
}

