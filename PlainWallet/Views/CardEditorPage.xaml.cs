using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;

namespace PlainWallet.Views;

public partial class CardEditorPage : ContentPage
{
    private MembershipCard? _editingCard;
    private bool _isEditing;
    private static readonly HttpClient _httpClient = new HttpClient();
    public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

    public CardEditorPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadOptions();
        Title = "New Card";
        LogoSelectionPage.LogoSelected += OnLogoSelected;
        var applyItem = new ToolbarItem("Apply", null, async () => await SaveCard())
        { Order = ToolbarItemOrder.Primary, Priority = 1 };

        ToolbarItems.Add(applyItem);
    }

    public CardEditorPage(MembershipCard? card) : this()
    {
        if (card is null) return;
        _editingCard = card;
        IsEditing = true;
        Name = card.Name ?? string.Empty;
        CardNumber = card.CardNumber ?? string.Empty;
        Notes = card.Notes ?? string.Empty;
        SelectedColor0 = card.BackgroundColor;
        SelectedBarcodeType = BarcodeTypeOptions.FirstOrDefault(o => o.Format == card.BarcodeType) ?? BarcodeTypeOptions[4];
        SelectedLogoUri = card.LogoUri;
        SelectedLogoUrl = card.LogoUrl;
        SelectedLogoData = card.LogoData;
        LogoKind = card.LogoKind;
        LogoPreview.Source = card.Logo;

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(CardNumber));
        OnPropertyChanged(nameof(Notes));
        OnPropertyChanged(nameof(SelectedColor0));
        OnPropertyChanged(nameof(SelectedBarcodeType));
        OnPropertyChanged(nameof(SelectedLogoUri));
        OnPropertyChanged(nameof(SelectedLogoUrl));
        OnPropertyChanged(nameof(SelectedLogoData));
        OnPropertyChanged(nameof(IsEditing));

        Title = "Edit Card";

        // var deleteItem = new ToolbarItem("Delete", null, async () => await DeleteCard())
        // { Order = ToolbarItemOrder.Primary, Priority = 1 };
        // ToolbarItems.Add(deleteItem);

    }

    private async void OnLogoSelected(string? logo, LogoKind logoKind)
    {
        if (string.IsNullOrEmpty(logo)) return;

        this.LogoKind = logoKind;

        switch (LogoKind)
        {
            case LogoKind.Builtin:
                SelectedLogoData = null;
                SelectedLogoUri = logo;
                SelectedLogoUrl = null;
                LogoPreview.Source = ImageSource.FromFile(logo);
                var color = LogosService.Instance.GetLogoColor(logo);
                if (color != Color.Default)
                    SelectedColor0 = color;
                break;
            case LogoKind.Web:
                {
                    try
                    {
                        if (logo.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                        {
                            SelectedLogoData = await MembershipCard.DownloadSvgAsPngAsync(logo, 256);
                            LogoPreview.Source = ImageSource.FromStream(() => new MemoryStream(SelectedLogoData));
                        }
                        else
                        {
                            LogoPreview.Source = ImageSource.FromUri(new Uri(logo));
                            // Download the image from the web URL
                            var imageData = await _httpClient.GetByteArrayAsync(logo);
                            SelectedLogoData = await MembershipCard.ResizeImageAsync(imageData, 256, 256);
                        }
                        SelectedLogoUri = null; // Clear URI since we're now using binary data
                        SelectedLogoUrl = logo; // Keep the URL for reference    
                    }
                    catch
                    {
                        SelectedLogoData = null;
                        SelectedLogoUri = null;
                        SelectedLogoUrl = logo; // Still keep the URL even if download failed
                        LogoPreview.Source = ImageSource.FromUri(new Uri(logo)); // Show preview anyway
                    }
                }
                break;
            case LogoKind.File:
                {
                    if (File.Exists(logo))
                    {
                        using var fileStream = File.OpenRead(logo);
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);
                            SelectedLogoData = await MembershipCard.ResizeImageAsync(memoryStream.ToArray(), 256, 256);
                        }
                        SelectedLogoUri = null; // Clear URI since we're now using binary data
                        SelectedLogoUrl = null; // Clear URI since we're now using binary data
                        LogoPreview.Source = ImageSource.FromFile(logo);
                    }
                    else
                    {
                        SelectedLogoData = null;
                        SelectedLogoUri = null; // Clear URI since we're now using binary data
                        SelectedLogoUrl = null; // Clear URI since we're now using binary data
                        LogoPreview.Source = null;
                    }
                }
                break;
        }

        OnPropertyChanged(nameof(LogoKind));
        OnPropertyChanged(nameof(SelectedLogoUri));
        OnPropertyChanged(nameof(SelectedLogoUrl));
        OnPropertyChanged(nameof(SelectedLogoData));
    }

    public string Name { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string? SelectedLogoUri { get; set; }
    public string? SelectedLogoUrl { get; set; }
    private byte[]? SelectedLogoData;

    private Color? _selectedColor;
    public Color? SelectedColor0 { get => _selectedColor; set { _selectedColor = value; OnPropertyChanged(); } }
    public ObservableCollection<BarcodeTypeOption> BarcodeTypeOptions { get; } = new();
    private BarcodeTypeOption? _selectedBarcodeType;
    private LogoKind LogoKind;

    public BarcodeTypeOption? SelectedBarcodeType { get => _selectedBarcodeType; set { _selectedBarcodeType = value; OnPropertyChanged(); } }

    private void LoadOptions()
    {

        foreach (ZXing.Net.Maui.BarcodeFormat format in Enum.GetValues(typeof(ZXing.Net.Maui.BarcodeFormat)))
            BarcodeTypeOptions.Add(new BarcodeTypeOption(format, FormatDisplayName(format)));
        _selectedBarcodeType = BarcodeTypeOptions[4];

        OnPropertyChanged(nameof(SelectedColor0));
        OnPropertyChanged(nameof(BarcodeTypeOptions));
        OnPropertyChanged(nameof(SelectedBarcodeType));
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        await SaveCard();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        await DeleteCard();
    }

    private async Task DeleteCard()
    {
        if (_editingCard is null) return;
        var confirm = await DisplayAlertAsync("Delete", "Are you sure you want to delete this card?", "Delete", "Cancel");
        if (!confirm) return;
        CardStore.Cards.Remove(_editingCard);
        // After deleting a card, navigate explicitly to the main list of cards
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async Task SaveCard()
    {
        if (_editingCard is not null)
        {
            _editingCard.Name = Name?.Trim() ?? string.Empty;
            _editingCard.CardNumber = CardNumber?.Trim() ?? string.Empty;
            _editingCard.Notes = Notes?.Trim() ?? string.Empty;
            _editingCard.BackgroundColor = SelectedColor0 ?? Colors.Gray;
            _editingCard.BarcodeType = SelectedBarcodeType?.Format ?? ZXing.Net.Maui.BarcodeFormat.Code128;
            _editingCard.LogoUri = SelectedLogoUri;
            _editingCard.LogoUrl = SelectedLogoUrl;
            _editingCard.LogoData = SelectedLogoData;
            _editingCard.LogoKind = LogoKind;

        }
        else
        {
            var card = new MembershipCard
            {
                Name = Name?.Trim() ?? string.Empty,
                CardNumber = CardNumber?.Trim() ?? string.Empty,
                Notes = Notes?.Trim() ?? string.Empty,
                BackgroundColor = SelectedColor0 ?? Colors.Gray,
                BarcodeType = SelectedBarcodeType?.Format ?? ZXing.Net.Maui.BarcodeFormat.Code128,
                LogoUri = SelectedLogoUri,
                LogoUrl = SelectedLogoUrl,
                LogoKind = LogoKind,
                LogoData = SelectedLogoData
            };
            CardStore.Cards.Add(card);
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSelectLogoClicked(object? sender, EventArgs e)
    {
        var page = new LogoSelectionPage(_editingCard?.LogoUri, _editingCard?.LogoUrl, _editingCard?.LogoData, _editingCard?.LogoKind ?? LogoKind.Builtin);
        await Navigation.PushAsync(page);
    }

    private async void OnScanBarcodeClicked(object? sender, EventArgs e)
    {
        var scanner = new ScannerPage((String? code, ZXing.Net.Maui.BarcodeFormat barcodeFormat) =>
        {
            CardNumber = code ?? string.Empty;
            SelectedBarcodeType = BarcodeTypeOptions.FirstOrDefault(o => o.Format == barcodeFormat) ?? BarcodeTypeOptions[4];
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
