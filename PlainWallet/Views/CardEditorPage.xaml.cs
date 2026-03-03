using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;
using ZXing;

namespace PlainWallet.Views;

public partial class CardEditorPage : ContentPage
{
    private MembershipCard? _editingCard;
    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

    public CardEditorPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadOptions();
        Title = "New Card";
    }

    public CardEditorPage(MembershipCard? card) : this()
    {
        if (card is null) return;
        _editingCard = card;
        IsEditing = true;
        Name = card.Name ?? string.Empty;
        CardNumber = card.CardNumber ?? string.Empty;
        Notes = card.Notes ?? string.Empty;
        SelectedColor = ColorOptions.FirstOrDefault(o => o.Color == card.BackgroundColor) ?? ColorOptions[0];
        SelectedBarcodeType = BarcodeTypeOptions.FirstOrDefault(o => o.Format == card.BarcodeType) ?? BarcodeTypeOptions[0];
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(CardNumber));
        OnPropertyChanged(nameof(Notes));
        OnPropertyChanged(nameof(SelectedColor));
        OnPropertyChanged(nameof(SelectedBarcodeType));
        OnPropertyChanged(nameof(IsEditing));

        Title = "Edit Card";

        // add delete toolbar item
        var deleteItem = new ToolbarItem("🗑", null, async () =>
        {
            if (_editingCard is not null) CardStore.Cards.Remove(_editingCard);
            await Shell.Current.GoToAsync("..");
        }) { Order = ToolbarItemOrder.Primary, Priority = 1 };
        ToolbarItems.Add(deleteItem);
    }

    public string Name { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public ObservableCollection<ColorOption> ColorOptions { get; } = new();
    private ColorOption? _selectedColor;
    public ColorOption? SelectedColor { get => _selectedColor; set { _selectedColor = value; OnPropertyChanged(); } }
    public ObservableCollection<BarcodeTypeOption> BarcodeTypeOptions { get; } = new();
    private BarcodeTypeOption? _selectedBarcodeType;
    public BarcodeTypeOption? SelectedBarcodeType { get => _selectedBarcodeType; set { _selectedBarcodeType = value; OnPropertyChanged(); } }

    private void LoadOptions()
    {
        ColorOptions.Add(new ColorOption("Deep Sky Blue", Colors.DeepSkyBlue));
        ColorOptions.Add(new ColorOption("Medium Purple", Colors.MediumPurple));
        ColorOptions.Add(new ColorOption("Orange Red", Colors.OrangeRed));
        ColorOptions.Add(new ColorOption("Sea Green", Colors.SeaGreen));
        ColorOptions.Add(new ColorOption("Goldenrod", Colors.Goldenrod));
        ColorOptions.Add(new ColorOption("Cadet Blue", Colors.CadetBlue));
        _selectedColor = ColorOptions[0];

        foreach (ZXing.Net.Maui.BarcodeFormat format in Enum.GetValues(typeof(ZXing.Net.Maui.BarcodeFormat)))
            BarcodeTypeOptions.Add(new BarcodeTypeOption(format, FormatDisplayName(format)));
        _selectedBarcodeType = BarcodeTypeOptions[0];
        OnPropertyChanged(nameof(ColorOptions));
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
            _editingCard.BackgroundColor = SelectedColor?.Color ?? Colors.Gray;
            _editingCard.BarcodeType = SelectedBarcodeType?.Format ?? ZXing.Net.Maui.BarcodeFormat.Code128;
        }
        else
        {
            var card = new MembershipCard
            {
                Name = Name?.Trim() ?? string.Empty,
                CardNumber = CardNumber?.Trim() ?? string.Empty,
                Notes = Notes?.Trim() ?? string.Empty,
                BackgroundColor = SelectedColor?.Color ?? Colors.Gray,
                BarcodeType = SelectedBarcodeType?.Format ?? ZXing.Net.Maui.BarcodeFormat.Code128,
                Logo = ImageSource.FromFile("dotnet_bot.png")
            };
            CardStore.Cards.Add(card);
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
}

public class ColorOption
{
    public string Name { get; }
    public Color Color { get; }
    public ColorOption(string name, Color color) => (Name, Color) = (name, color);
}

public class BarcodeTypeOption
{
    public ZXing.Net.Maui.BarcodeFormat Format { get; }
    public string DisplayName { get; }
    public BarcodeTypeOption(ZXing.Net.Maui.BarcodeFormat format, string displayName) => (Format, DisplayName) = (format, displayName);
}
