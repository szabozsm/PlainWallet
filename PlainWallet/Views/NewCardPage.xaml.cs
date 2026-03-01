using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using PlainWallet.Models;
using PlainWallet.Services;
using ZXing;

namespace PlainWallet.Views;

public partial class NewCardPage : ContentPage
{
    public NewCardPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadOptions();
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

        foreach (BarcodeFormat format in Enum.GetValues(typeof(BarcodeFormat)))
            BarcodeTypeOptions.Add(new BarcodeTypeOption(format, FormatDisplayName(format)));
        _selectedBarcodeType = BarcodeTypeOptions[0];
        OnPropertyChanged(nameof(ColorOptions));
        OnPropertyChanged(nameof(SelectedColor));
        OnPropertyChanged(nameof(BarcodeTypeOptions));
        OnPropertyChanged(nameof(SelectedBarcodeType));
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var card = new MembershipCard
        {
            Name = Name?.Trim() ?? string.Empty,
            CardNumber = CardNumber?.Trim() ?? string.Empty,
            Notes = Notes?.Trim() ?? string.Empty,
            BackgroundColor = SelectedColor?.Color ?? Colors.Gray,
            BarcodeType = SelectedBarcodeType?.Format ?? BarcodeFormat.CODE_128,
            Logo = ImageSource.FromFile("dotnet_bot.png")
        };
        CardStore.Cards.Add(card);
        await Shell.Current.GoToAsync("..");
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private static string FormatDisplayName(BarcodeFormat format)
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
    public BarcodeFormat Format { get; }
    public string DisplayName { get; }
    public BarcodeTypeOption(BarcodeFormat format, string displayName) => (Format, DisplayName) = (format, displayName);
}
