using Microsoft.Maui.Controls;
using ZXing;

namespace PlainWallet.Models;

public class MembershipCard
{
    public string CardNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ImageSource? Logo { get; set; }
    public Color BackgroundColor { get; set; } = Colors.Gray;
    public string Notes { get; set; } = string.Empty;
    public ZXing.Net.Maui.BarcodeFormat BarcodeType { get; set; }
}
