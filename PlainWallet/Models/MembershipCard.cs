using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using ZXing;

namespace PlainWallet.Models;

public class MembershipCard : INotifyPropertyChanged
{

    private Color _backgroundColor = Colors.LightGray;
    private int _barcodeTypeValue;
    private string _cardNumber = string.Empty;
    private byte[]? _logoData;
    private string? _logoUri = "";
    private string? _logoUrl = "";
    private string _name = string.Empty;
    private string _notes = string.Empty;
    public event PropertyChangedEventHandler? PropertyChanged;

    public Color BackgroundColor { get => _backgroundColor; set { if (_backgroundColor == value) return; _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); } }

    [NotMapped]
    public ZXing.Net.Maui.BarcodeFormat BarcodeType
    {
        get => (ZXing.Net.Maui.BarcodeFormat)BarcodeTypeValue;
        set => BarcodeTypeValue = (int)value;
    }

    public int BarcodeTypeValue { get => _barcodeTypeValue; set { if (_barcodeTypeValue == value) return; _barcodeTypeValue = value; OnPropertyChanged(nameof(BarcodeTypeValue)); } }

    public string CardNumber { get => _cardNumber; set { if (_cardNumber == value) return; _cardNumber = value; OnPropertyChanged(nameof(CardNumber)); } }

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
    public int Id { get; set; }

    [NotMapped]
    public ImageSource? Logo
    {
        get
        {
            // First try to load from binary data if available
            if (LogoData != null && LogoData.Length > 0)
            {
                try
                {
                    return ImageSource.FromStream(() => new MemoryStream(LogoData));
                }
                catch
                {
                    // Fall back to URI if binary data fails
                }
            }

            try
            {
                // If the stored value looks like a file name, return a file image source
                if (!string.IsNullOrEmpty(LogoUri))
                    return ImageSource.FromFile(LogoUri);
                else
                    if (!string.IsNullOrEmpty(LogoUrl))
                        return ImageSource.FromUri(new Uri(LogoUrl));
                    else
                        return null;

            }
            catch
            {
                return null;
            }
        }

    }
    // Persist binary logo data directly in the database

    public byte[]? LogoData { get => _logoData; set { if (_logoData == value) return; _logoData = value; OnPropertyChanged(nameof(LogoData)); } }

    // Persist a URI or path for the logo image if available
    public string? LogoUri { get => _logoUri; set { if (_logoUri == value) return; _logoUri = value; OnPropertyChanged(nameof(LogoUri)); } }

    public string? LogoUrl { get => _logoUrl; set { if (_logoUrl == value) return; _logoUrl = value; OnPropertyChanged(nameof(LogoUrl)); } }
    public string Name { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); } }

    public string Notes { get => _notes; set { if (_notes == value) return; _notes = value; OnPropertyChanged(nameof(Notes)); } }

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
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
