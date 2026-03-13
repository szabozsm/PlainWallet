using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Android.Util;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using Svg.Skia;
using ZXing;

namespace PlainWallet.Models;

public enum LogoKind
{
    Builtin,
    Web,
    File
}

public class MembershipCard : INotifyPropertyChanged
{

    private Color _backgroundColor = Colors.LightGray;
    private Guid _id = Guid.NewGuid();
    private int _barcodeTypeValue;
    private string _cardNumber = string.Empty;
    private byte[]? _logoData;
    private LogoKind _logoKind;
    private string? _logoUri = "";
    private string? _logoUrl = "";
    private string _name = string.Empty;
    private string _notes = string.Empty;
    private DateTime _lastChanged = DateTime.Now;
    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonConverter(typeof(MauiColorJsonConverter))]
    public Color BackgroundColor { get => _backgroundColor; set { if (_backgroundColor == value) return; _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); } }

    public LogoKind LogoKind { get => _logoKind; set { if (_logoKind == value) return; _logoKind = value; OnPropertyChanged(nameof(LogoKind)); } }

    [JsonIgnore]
    [NotMapped]
    public ZXing.Net.Maui.BarcodeFormat BarcodeType
    {
        get => (ZXing.Net.Maui.BarcodeFormat)BarcodeTypeValue;
        set => BarcodeTypeValue = (int)value;
    }

    public int BarcodeTypeValue { get => _barcodeTypeValue; set { if (_barcodeTypeValue == value) return; _barcodeTypeValue = value; OnPropertyChanged(nameof(BarcodeTypeValue)); } }

    public string CardNumber { get => _cardNumber; set { if (_cardNumber == value) return; _cardNumber = value; OnPropertyChanged(nameof(CardNumber)); } }

    [JsonIgnore]
    [NotMapped]
    public Color ComplementaryColor
    {
        get
        {
            double luminance = (0.299 * BackgroundColor.Red + 0.587 * BackgroundColor.Green + 0.114 * BackgroundColor.Blue);
            return luminance > 0.5 ? Colors.Black : Colors.White;
        }
    }

    // Primary key for EF

    [Key]
    public Guid Id { get => _id; set { if (_id == value) return; _id = value; OnPropertyChanged(nameof(Id)); } }

    [JsonIgnore]
    [NotMapped]
    public ImageSource? Logo
    {
        get
        {
            switch (LogoKind)
            {
                case LogoKind.Builtin:
                    if (!string.IsNullOrEmpty(LogoUri))
                        return ImageSource.FromFile(LogoUri);
                    break;
                case LogoKind.Web:
                    {
                        if (LogoData != null && LogoData.Length > 0)
                        {
                            try
                            {
                                return ImageSource.FromStream(() => new MemoryStream(LogoData));
                            }
                            catch
                            {
                            }
                        }
                        try
                        {
                            if (LogoUrl.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                            {
                                byte[] SelectedLogoData = MembershipCard.DownloadSvgAsPngAsync(LogoUrl, 256).GetAwaiter().GetResult();
                                return ImageSource.FromStream(() => new MemoryStream(SelectedLogoData));
                            }
                            else

                                if (!string.IsNullOrEmpty(LogoUrl))
                                    return ImageSource.FromUri(new Uri(LogoUrl));
                        }
                        catch
                        {
                        }

                        return null;
                    }

                    break;
                case LogoKind.File:
                    {
                        if (LogoData != null && LogoData.Length > 0)
                        {
                            try
                            {
                                return ImageSource.FromStream(() => new MemoryStream(LogoData));
                            }
                            catch
                            {
                                return null;
                            }
                        }
                        return null;
                    }
                    break;
            }
            return null;
        }

    }
    // Persist binary logo data directly in the database

    public byte[]? LogoData { get => _logoData; set { if (_logoData == value) return; _logoData = value; OnPropertyChanged(nameof(LogoData)); } }

    // Persist a URI or path for the logo image if available
    public string? LogoUri { get => _logoUri; set { if (_logoUri == value) return; _logoUri = value; OnPropertyChanged(nameof(LogoUri)); } }

    public string? LogoUrl { get => _logoUrl; set { if (_logoUrl == value) return; _logoUrl = value; OnPropertyChanged(nameof(LogoUrl)); } }
    public string Name { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); } }

    public string Notes { get => _notes; set { if (_notes == value) return; _notes = value; OnPropertyChanged(nameof(Notes)); } }

    [NotMapped]
    public DateTime JsonLastChanged { get; set; }

    [JsonIgnore]
    public DateTime LastChanged { get => _lastChanged; private set => _lastChanged = value; }

    /// <summary>
    /// Resizes an image to the specified maximum dimensions while maintaining aspect ratio
    /// </summary>
    /// <param name="imageBytes">The original image bytes</param>
    /// <param name="maxWidth">Maximum width</param>
    /// <param name="maxHeight">Maximum height</param>
    /// <returns>Resized image bytes</returns>
    public static async Task<byte[]> ResizeImageAsync(byte[] imageBytes, int maxWidth, int maxHeight)
    {
        try
        {
            if (imageBytes.Length == 0)
                return imageBytes;

            // Use Microsoft.Maui.ApplicationModel.DataTransfer for image processing
            // Load image from bytes
            using var originalStream = new MemoryStream(imageBytes);

            // For MAUI, we'll use a simple approach with ImageSource
            // This is a basic implementation - for production use, consider using a library like ImageSharp
            return await ResizeImageMauiAsync(originalStream, maxWidth, maxHeight);
        }
        catch
        {
            // If resizing fails, return original
            return imageBytes;
        }
    }
    protected void OnPropertyChanged(string name)
    {
        _lastChanged = DateTime.Now;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public static async Task<byte[]> DownloadSvgAsPngAsync(string url, int maxSize = 256)
    {

        // 1. Download the SVG content
        using var httpClient = new HttpClient();
        var svgContent = await httpClient.GetStringAsync(url);

        // 2. Load SVG into Svg.Skia
        using var svg = new SKSvg();
        using var svgStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));
        svg.Load(svgStream);

        if (svg.Picture is null)
            throw new InvalidOperationException("Failed to parse SVG content.");

        // 3. Determine original SVG bounds
        var svgBounds = svg.Picture.CullRect;
        float svgWidth = svgBounds.Width;
        float svgHeight = svgBounds.Height;

        if (svgWidth <= 0 || svgHeight <= 0)
            throw new InvalidOperationException("SVG has invalid dimensions.");

        // 4. Calculate scale to fit within maxSize x maxSize, preserving aspect ratio
        float scale = Math.Min(maxSize / svgWidth, maxSize / svgHeight);

        int targetWidth = (int)Math.Round(svgWidth * scale);
        int targetHeight = (int)Math.Round(svgHeight * scale);

        // 5. Render SVG to a SkiaSharp bitmap
        using var bitmap = new SKBitmap(targetWidth, targetHeight);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.Transparent);
        canvas.Scale(scale);
        canvas.DrawPicture(svg.Picture);
        canvas.Flush();

        // 6. Encode bitmap to PNG and return as byte[]
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    private static async Task<byte[]> ResizeImageMauiAsync(Stream imageStream, int maxWidth, int maxHeight)
    {
        try
        {
            // Read the original stream
            using var originalStream = new MemoryStream();
            await imageStream.CopyToAsync(originalStream);
            var originalBytes = originalStream.ToArray();

            // Load image using SkiaSharp
            using var bitmap = SKBitmap.Decode(originalBytes);
            if (bitmap == null)
                return originalBytes; // Return original if decoding fails

            // Calculate new dimensions maintaining aspect ratio
            var originalWidth = bitmap.Width;
            var originalHeight = bitmap.Height;

            if (originalWidth <= maxWidth && originalHeight <= maxHeight)
            {
                // Image is already small enough, return original
                return originalBytes;
            }

            // Calculate scaling factor
            var scaleX = (float)maxWidth / originalWidth;
            var scaleY = (float)maxHeight / originalHeight;
            var scale = Math.Min(scaleX, scaleY);

            var newWidth = (int)(originalWidth * scale);
            var newHeight = (int)(originalHeight * scale);

            // Create resized bitmap
            using var resizedBitmap = new SKBitmap(newWidth, newHeight);
            using var canvas = new SKCanvas(resizedBitmap);

            // Draw scaled image
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(bitmap, new SKRect(0, 0, newWidth, newHeight));

            // Convert to byte array
            using var outputStream = new MemoryStream();
            resizedBitmap.Encode(outputStream, SKEncodedImageFormat.Png, 90);
            return outputStream.ToArray();
        }
        catch
        {
            return new byte[0];
        }
    }

}
public class MauiColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string hexValue = reader.GetString();
        if (string.IsNullOrEmpty(hexValue))
        {
            return Colors.White;
        }
        return Color.FromArgb(hexValue);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(value.ToArgbHex(includeAlpha: true));
    }
}