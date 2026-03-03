using System.Collections.ObjectModel;
using ZXing;
using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;
using PlainWallet.Views;

namespace PlainWallet;

public partial class MainPage : ContentPage
{
    public ObservableCollection<MembershipCard> Cards => CardStore.Cards;
    private readonly Random _random = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadSampleCards();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        OnPropertyChanged(nameof(Cards));
    }

    private void LoadSampleCards()
    {
        if (CardStore.Cards.Count > 0)
            return;
        var names = new[]
        {
            "SuperMart Club",
            "City Gym",
            "Book Lovers",
            "Cinema Stars",
            "Coffee Points",
            "Tech Store Plus"
        };

        var notes = new[]
        {
            "Show at checkout to collect points.",
            "Access pass for all locations.",
            "10% off all paperbacks.",
            "Free popcorn every 5 visits.",
            "Every 7th drink is free.",
            "Extended warranty on all gadgets."
        };

        var colors = new[]
        {
            Colors.DeepSkyBlue,
            Colors.MediumPurple,
            Colors.OrangeRed,
            Colors.SeaGreen,
            Colors.Goldenrod,
            Colors.CadetBlue
        };

        var barcodeFormats = new[]
        {
            ZXing.Net.Maui.BarcodeFormat.Code128,
            ZXing.Net.Maui.BarcodeFormat.Code39,
            ZXing.Net.Maui.BarcodeFormat.Code93,
            ZXing.Net.Maui.BarcodeFormat.Ean13
        };

        for (int i = 0; i < 8; i++)
        {
            var index = _random.Next(names.Length);
            CardStore.Cards.Add(new MembershipCard
            {
                Name = names[index],
                CardNumber = $"{_random.Next(1000, 9999)} {_random.Next(1000, 9999)} {_random.Next(1000, 9999)}",
                Logo = ImageSource.FromFile("dotnet_bot.png"),
                BackgroundColor = colors[_random.Next(colors.Length)],
                Notes = notes[index],
                BarcodeType = barcodeFormats[_random.Next(barcodeFormats.Length)]
            });
        }
    }

    private void OnCardSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is MembershipCard selectedCard)
            OpenCardDetail(selectedCard);

        if (sender is CollectionView collectionView)
            collectionView.SelectedItem = null;
    }

    private void OnCardTapped(object sender, TappedEventArgs e)
    {
        // Gesture recognizer is inside the Frame; Frame's BindingContext is the card
        if (sender is Element element &&
            element.Parent is BindableObject parent &&
            parent.BindingContext is MembershipCard card)
        {
            OpenCardDetail(card);
        }
    }

    private async void OpenCardDetail(MembershipCard card)
    {
        CardNavigation.SelectedCard = card;
        await Shell.Current.GoToAsync(nameof(CardDetailPage));
    }

    private async void OnAddCardClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NewCardPage));
    }
}

