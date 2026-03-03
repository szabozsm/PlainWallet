using SkiaSharp;
using ZXing.SkiaSharp;

namespace PlainWallet.Services;

public static class BarcodeGenerator
{
    /// <summary>
    /// Generates a barcode image from the given value using the specified format.
    /// Returns null if value is null/empty or generation fails.
    /// </summary>
    public static ImageSource? CreateBarcode(string? value, ZXing.Net.Maui.BarcodeFormat format, int width = 800, int height = 800)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // EAN-13 requires exactly 12 or 13 digits
        var content = value;
        if (format == ZXing.Net.Maui.BarcodeFormat.Ean13)
        {
            var digits = new string(value.Where(char.IsDigit).ToArray());
            content = digits.Length >= 12 ? digits[..12] : digits.PadLeft(12, '0');
        }

        try
        {
            var writer = new BarcodeWriter
            {
                Format = (ZXing.BarcodeFormat)format,
                Options = new global::ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 10,
                    PureBarcode = true
                }
            };

            using var bitmap = writer.Write(content);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var bytes = data.ToArray();
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        catch
        {
            return null;
        }
    }
}
