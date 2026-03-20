using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using ZXing;
using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;
using PlainWallet.Views;

namespace PlainWallet;

public partial class MainPage : ContentPage
{
    public ObservableCollection<MembershipCard> Cards => CardStore.Cards;
    public ObservableCollection<MembershipCard> FilteredCards { get; } = new();

    private string _filter = string.Empty;
    private readonly Random _random = new();
    private readonly IServiceProvider _services;

    public MainPage()
    {
        _services = IPlatformApplication.Current.Services;
        InitializeComponent();
        BindingContext = this;
        // keep filtered view in sync with the store
        CardStore.Cards.CollectionChanged += CardStore_CardsChanged;
        ApplyFilter();
    }

    public bool ShowAnimation
    {
        get;
        set;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        ApplyFilter();
        CardCollection.Loaded += OnCardCollectionLoaded;
 
    }

    private async void OnCardCollectionLoaded(object? sender, EventArgs e)
    {
        await UpdateCardsFromInternet();
    }

    private async Task UpdateCardsFromInternet()
    {
        if (SettingsStore.UseExtendsClass)
        {
            try
            {
                ShowAnimation = true;
                OnPropertyChanged(nameof(ShowAnimation));
                var importService = _services.GetRequiredService<ImportService>();
                await importService.DownloadData();
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlertAsync("Error", $"Failed to download data from extendsclass.com: {ex.Message}", "OK");
            }
            finally
            {
                ShowAnimation = false;
                OnPropertyChanged(nameof(ShowAnimation));
            }
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
        // Gesture recognizer is inside the item container; its BindingContext is the card
        if (sender is Element element &&
            element.Parent is BindableObject parent &&
            parent.BindingContext is MembershipCard card)
        {
            OpenCardDetail(card);
        }
    }

    private void OnFilterTextChanged(object sender, TextChangedEventArgs e)
    {
        _filter = e.NewTextValue ?? string.Empty;
        ApplyFilter();
    }

    private void CardStore_CardsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = (_filter ?? string.Empty).Trim();
        FilteredCards.Clear();
        IEnumerable<MembershipCard> items = CardStore.Cards;
        if (!string.IsNullOrEmpty(q))
        {
            var lower = q.ToLowerInvariant();
            items = items.Where(c =>
                (!string.IsNullOrEmpty(c.Name) && c.Name.ToLowerInvariant().Contains(lower)) ||
                (!string.IsNullOrEmpty(c.CardNumber) && c.CardNumber.ToLowerInvariant().Contains(lower)) ||
                (!string.IsNullOrEmpty(c.Notes) && c.Notes.ToLowerInvariant().Contains(lower))
            );
        }

        foreach (var it in items.OrderBy(c => c.Name))
            FilteredCards.Add(it);
    }

    private async void OpenCardDetail(MembershipCard card)
    {
        CardNavigation.SelectedCard = card;
        await Shell.Current.GoToAsync(nameof(CardDetailPage));
    }

    private async void OnAddCardClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CardEditorPage));
    }
}

