using SkiaSharp;
using ZXing.SkiaSharp;

namespace PlainWallet.Services;

public static class BarcodeGenerator
{
    /// <summary>
    /// Generates a Code 128 barcode image from the given value.
    /// Returns null if value is null/empty or generation fails.
    /// </summary>
    public static ImageSource? CreateBarcode(string? value, int width = 800, int height = 80)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            var writer = new BarcodeWriter
            {
                Format = global::ZXing.BarcodeFormat.CODE_128,
                Options = new global::ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 10,
                    PureBarcode = true
                }
            };

            using var bitmap = writer.Write(value);
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
