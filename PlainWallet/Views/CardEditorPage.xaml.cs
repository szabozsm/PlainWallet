using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;
using ZXing;
using PlainWallet.Views;
using Microsoft.Maui.Storage;
using System.IO;

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
        LogoSelectionPage.LogoSelected += OnLogoSelected;
    }

    public CardEditorPage(MembershipCard? card) : this()
    {
        if (card is null) return;
        _editingCard = card;
        IsEditing = true;
        Name = card.Name ?? string.Empty;
        CardNumber = card.CardNumber ?? string.Empty;
        Notes = card.Notes ?? string.Empty;
        SelectedColor = card.BackgroundColor;
        SelectedBarcodeType = BarcodeTypeOptions.FirstOrDefault(o => o.Format == card.BarcodeType) ?? BarcodeTypeOptions[0];
        SelectedLogoUri = card.LogoUri;
        if (!string.IsNullOrEmpty(SelectedLogoUri))
        {
            try { LogoPreview.Source = ImageSource.FromFile(SelectedLogoUri); } catch { LogoPreview.Source = ImageSource.FromFile(SelectedLogoUri); }
        }
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(CardNumber));
        OnPropertyChanged(nameof(Notes));
        OnPropertyChanged(nameof(SelectedColor));
        OnPropertyChanged(nameof(SelectedBarcodeType));
        OnPropertyChanged(nameof(IsEditing));

        Title = "Edit Card";

        // add delete toolbar item with confirmation
        var deleteItem = new ToolbarItem("Delete", null, async () =>
        {
            if (_editingCard is null) return;
            var confirm = await DisplayAlertAsync("Delete", "Are you sure you want to delete this card?", "Delete", "Cancel");
            if (!confirm) return;
            CardStore.Cards.Remove(_editingCard);
            // After deleting a card, navigate explicitly to the main list of cards
            await Shell.Current.GoToAsync("//MainPage");
        }) { Order = ToolbarItemOrder.Primary, Priority = 1 };
        ToolbarItems.Add(deleteItem);
    }

    private async void OnLogoSelected(string? logo)
    {
        if (string.IsNullOrEmpty(logo)) return;
        SelectedLogoUri = logo;
        try
        {
            // Check if it's a file path (not a URL)
            if (!logo.Contains("://"))
            {
                // It's a file path, read it and store as binary data
                if (File.Exists(logo))
                {
                    using var fileStream = File.OpenRead(logo);
                    if (_editingCard is not null)
                    {
                        await SetLogoFromStreamAsync(fileStream, logo);
                    }
                    else
                    {
                        // For new cards, store in temporary field
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);
                            _selectedLogoData = memoryStream.ToArray();
                        }
                    }
                    LogoPreview.Source = ImageSource.FromFile(logo);
                }
                else
                {
                    // File doesn't exist, treat as built-in resource
                    LogoPreview.Source = ImageSource.FromFile(logo);
                }
            }
            else
            {
                // It's a URL, store URI only (for now)
                LogoPreview.Source = ImageSource.FromUri(new Uri(logo));
            }
        }
        catch
        {
            // fallback: try URI then file
            try { LogoPreview.Source = ImageSource.FromUri(new Uri(logo)); } catch { LogoPreview.Source = ImageSource.FromFile(logo); }
        }
        OnPropertyChanged(nameof(SelectedLogoUri));
    }

    private async Task SetLogoFromStreamAsync(Stream stream, string? uri = null)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                if (_editingCard is not null)
                {
                    _editingCard.LogoData = memoryStream.ToArray();
                    _editingCard.LogoUri = uri;
                }
            }
        }
        catch
        {
            if (_editingCard is not null)
            {
                _editingCard.LogoData = null;
                _editingCard.LogoUri = uri;
            }
        }
    }

    public string Name { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string? SelectedLogoUri { get; set; }
    private byte[]? _selectedLogoData;

    
    private Color? _selectedColor;
    public Color? SelectedColor { get => _selectedColor; set { _selectedColor = value; OnPropertyChanged(); } }
    public ObservableCollection<BarcodeTypeOption> BarcodeTypeOptions { get; } = new();
    private BarcodeTypeOption? _selectedBarcodeType;
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
            if (!string.IsNullOrEmpty(SelectedLogoUri)) _editingCard.LogoUri = SelectedLogoUri;
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
                LogoUri = !string.IsNullOrEmpty(SelectedLogoUri) ? SelectedLogoUri : "dotnet_bot.png",
                LogoData = _selectedLogoData
            };
            CardStore.Cards.Add(card);
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSelectLogoClicked(object? sender, EventArgs e)
    {
        string? initial = null;
        if (_editingCard is not null && !string.IsNullOrEmpty(_editingCard.LogoUri) && _editingCard.LogoUri.Contains("://"))
            initial = _editingCard.LogoUri;
        var page = new LogoSelectionPage(initial);
        await Navigation.PushAsync(page);
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

 

public class BarcodeTypeOption
{
    public ZXing.Net.Maui.BarcodeFormat Format { get; }
    public string DisplayName { get; }
    public BarcodeTypeOption(ZXing.Net.Maui.BarcodeFormat format, string displayName) => (Format, DisplayName) = (format, displayName);
}
