using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ZXing;

namespace PlainWallet.Models;

public class MembershipCard : INotifyPropertyChanged
{
    private string _cardNumber = string.Empty;
    private string _name = string.Empty;
    private string _notes = string.Empty;

    private Color _backgroundColor = Colors.Gray;

    // Primary key for EF
    [Key]
    public int Id { get; set; }

    public string CardNumber { get => _cardNumber; set { if (_cardNumber == value) return; _cardNumber = value; OnPropertyChanged(nameof(CardNumber)); } }
    public string Name { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); } }

    // Persisted representation of the background color as hex ARGB


    public Color BackgroundColor { get; set; } = Colors.Gray;


    // Persist a URI or path for the logo image if available
    public string? LogoUri { get; set; } = "";

    [NotMapped]
    public ImageSource? Logo
    {
        get
        {
            if (string.IsNullOrEmpty(LogoUri))
                return null;
            try
            {
                // If the stored value looks like a file name, return a file image source
                if (!LogoUri.Contains("://") && !LogoUri.StartsWith("/"))
                    return ImageSource.FromFile(LogoUri);
                return ImageSource.FromUri(new Uri(LogoUri));
            }
            catch
            {
                return null;
            }
        }
        set
        {
            try
            {
                if (value is FileImageSource file) LogoUri = file.File;
                else if (value is UriImageSource uri) LogoUri = uri.Uri?.ToString();
                else LogoUri = null;
            }
            catch
            {
                LogoUri = null;
            }
            OnPropertyChanged(nameof(Logo));
            OnPropertyChanged(nameof(LogoUri));
        }
    }

    public string Notes { get => _notes; set { if (_notes == value) return; _notes = value; OnPropertyChanged(nameof(Notes)); } }

    public int BarcodeTypeValue { get; set; }

    [NotMapped]
    public ZXing.Net.Maui.BarcodeFormat BarcodeType
    {
        get => (ZXing.Net.Maui.BarcodeFormat)BarcodeTypeValue;
        set => BarcodeTypeValue = (int)value;
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private static string ToHex(Color c)
    {
        byte a = (byte)(c.Alpha * 255);
        byte r = (byte)(c.Red * 255);
        byte g = (byte)(c.Green * 255);
        byte b = (byte)(c.Blue * 255);
        return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
    }

    private static Color ParseColor(string? hex)
    {
        try
        {
            if (string.IsNullOrEmpty(hex)) return Colors.Gray;
            var h = hex.TrimStart('#');
            if (h.Length == 8)
            {
                byte a = Convert.ToByte(h.Substring(0, 2), 16);
                byte r = Convert.ToByte(h.Substring(2, 2), 16);
                byte g = Convert.ToByte(h.Substring(4, 2), 16);
                byte b = Convert.ToByte(h.Substring(6, 2), 16);
                return Color.FromRgba(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
            }
            if (h.Length == 6)
            {
                byte r = Convert.ToByte(h.Substring(0, 2), 16);
                byte g = Convert.ToByte(h.Substring(2, 2), 16);
                byte b = Convert.ToByte(h.Substring(4, 2), 16);
                return Color.FromRgb(r / 255.0, g / 255.0, b / 255.0);
            }
        }
        catch
        {
            // fall through to return default
        }
        return Colors.Gray;
    }
}
