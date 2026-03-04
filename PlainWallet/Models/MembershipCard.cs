using Microsoft.Maui.Controls;
using ZXing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlainWallet.Models;

public class MembershipCard
{
    [Key]
    public int Id { get; set; }

    public string CardNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

[NotMapped]
    public ImageSource? Logo { get; set; }

 
    public Color BackgroundColor { get; set; } = Colors.Gray;

    public string Notes { get; set; } = string.Empty;

    // persisted barcode format
    public int BarcodeTypeValue { get; set; }

    [NotMapped]
    public ZXing.Net.Maui.BarcodeFormat BarcodeType
    {
        get => (ZXing.Net.Maui.BarcodeFormat)BarcodeTypeValue;
        set => BarcodeTypeValue = (int)value;
    }
  
}
