using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;
using PlainWallet.Services.Data;
using ZXing;

namespace PlainWallet.Views;

public partial class CardEditorPage : ContentPage
{
    private MembershipCard? _editingCard;
    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

    public CardEditorPage(CardDbContext db)
    {
        InitializeComponent();
        BindingContext = this;
        LoadOptions();
        Title = "New Card";
        this.db = db;
    }

    public CardEditorPage(MembershipCard? card, CardDbContext db) : this(db)
    {
        if (card is null) return;
        _editingCard = card;
        IsEditing = true;
        Name = card.Name ?? string.Empty;
        CardNumber = card.CardNumber ?? string.Empty;
        Notes = card.Notes ?? string.Empty;
        SelectedColor = Colors.LightBlue;
        SelectedBarcodeType = BarcodeTypeOptions.FirstOrDefault(o => o.Format == card.BarcodeType) ?? BarcodeTypeOptions[0];
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(CardNumber));
        OnPropertyChanged(nameof(Notes));
        OnPropertyChanged(nameof(SelectedColor));
        OnPropertyChanged(nameof(SelectedBarcodeType));
        OnPropertyChanged(nameof(IsEditing));

        Title = "Edit Card";

        // add delete toolbar item with confirmation
        var deleteItem = new ToolbarItem("🗑", null, async () =>
        {
            if (_editingCard is null) return;
            var confirm = await DisplayAlert("Delete", "Are you sure you want to delete this card?", "Delete", "Cancel");
            if (!confirm) return;
            await DeleteCardAsync(_editingCard);
            // After deleting a card, navigate explicitly to the main list of cards
            await Shell.Current.GoToAsync("//MainPage");
        })
        { Order = ToolbarItemOrder.Primary, Priority = 1 };
        ToolbarItems.Add(deleteItem);
    }

   
    public string Name { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    
    private Color? _selectedColor;
    public Color? SelectedColor { get => _selectedColor; set { _selectedColor = value; OnPropertyChanged(); } }
    public ObservableCollection<BarcodeTypeOption> BarcodeTypeOptions { get; } = new();
    private BarcodeTypeOption? _selectedBarcodeType;
    private readonly CardDbContext db;

    public BarcodeTypeOption? SelectedBarcodeType { get => _selectedBarcodeType; set { _selectedBarcodeType = value; OnPropertyChanged(); } }

    private void LoadOptions()
    {
      

        foreach (ZXing.Net.Maui.BarcodeFormat format in Enum.GetValues(typeof(ZXing.Net.Maui.BarcodeFormat)))
            BarcodeTypeOptions.Add(new BarcodeTypeOption(format, FormatDisplayName(format)));
        _selectedBarcodeType = BarcodeTypeOptions[0];
        
        OnPropertyChanged(nameof(SelectedColor));
        OnPropertyChanged(nameof(BarcodeTypeOptions));
        OnPropertyChanged(nameof(SelectedBarcodeType));
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_editingCard is not null)
        {
            _editingCard.Name = Name?.Trim() ?? string.Empty;
            _editingCard.CardNumber = CardNumber?.Trim() ?? string.Empty;
            _editingCard.Notes = Notes?.Trim() ?? string.Empty;
            _editingCard.BackgroundColor = SelectedColor ?? Colors.Gray;
            _editingCard.BarcodeType = SelectedBarcodeType?.Format ?? ZXing.Net.Maui.BarcodeFormat.Code128;
            await SaveCardAsync(_editingCard);
        }
        else
        {
            var card = new MembershipCard
            {
                Name = Name?.Trim() ?? string.Empty,
                CardNumber = CardNumber?.Trim() ?? string.Empty,
                Notes = Notes?.Trim() ?? string.Empty,
                BackgroundColor = SelectedColor ?? Colors.Gray,
                BarcodeType = SelectedBarcodeType?.Format ?? ZXing.Net.Maui.BarcodeFormat.Code128,
                Logo = ImageSource.FromFile("dotnet_bot.png")
            };
            await AddCardAsync(card);
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OnScanBarcodeClicked(object? sender, EventArgs e)
    {
        var scanner = new ScannerPage((String? code, ZXing.Net.Maui.BarcodeFormat barcodeFormat) =>
        {
            CardNumber = code ?? string.Empty;
            SelectedBarcodeType = BarcodeTypeOptions.FirstOrDefault(o => o.Format == barcodeFormat) ?? BarcodeTypeOptions[0];
            OnPropertyChanged(nameof(CardNumber));
            OnPropertyChanged(nameof(SelectedBarcodeType));
        });
        await Navigation.PushAsync(scanner);
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private static string FormatDisplayName(ZXing.Net.Maui.BarcodeFormat format)
    {
        var s = format.ToString();
        if (string.IsNullOrEmpty(s)) return s;
        var result = new System.Text.StringBuilder(s.Length + 4);
        result.Append(char.ToUpperInvariant(s[0]));
        for (int i = 1; i < s.Length; i++)
        {
            if (s[i] == '_') result.Append(' ');
            else if (char.IsUpper(s[i])) result.Append(' ').Append(s[i]);
            else result.Append(char.ToUpperInvariant(s[i]));
        }
        return result.ToString();
    }

 private async Task DeleteCardAsync(MembershipCard card)
    {
        if (db == null) throw new InvalidOperationException("CardStore is not initialized.");
        db.Cards.Remove(card);
        await db.SaveChangesAsync();
    }

    private async Task SaveCardAsync(MembershipCard card)
    {
        if (db == null) throw new InvalidOperationException("CardStore is not initialized.");
        if (card.Id == 0)
            db.Cards.Add(card);
        else
            db.Cards.Update(card);
        await db.SaveChangesAsync();
    }

    
    private  async Task AddCardAsync(MembershipCard card)
    {
        if (db == null) throw new InvalidOperationException("CardStore is not initialized.");
        db.Cards.Add(card);
        await db.SaveChangesAsync();
    }


}

 

public class BarcodeTypeOption
{
    public ZXing.Net.Maui.BarcodeFormat Format { get; }
    public string DisplayName { get; }
    public BarcodeTypeOption(ZXing.Net.Maui.BarcodeFormat format, string displayName) => (Format, DisplayName) = (format, displayName);
}
