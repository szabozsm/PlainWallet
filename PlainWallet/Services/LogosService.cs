using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using SkiaSharp;
using Svg.Skia;
using System.Text;
using System.IO.Compression;

namespace PlainWallet.Services;

public static class LogosService
{
    private const int MaxSvgRasterizedSize = 320;
    private sealed record LogoInfo(string Name, byte[] ImageData, string BackgroundColor, bool IsSvg);

    private static readonly LogoInfo[] _builtIn = LoadBuiltInLogos();

    private static LogoInfo[] LoadBuiltInLogos()
    {
        using var stream = typeof(LogosService).Assembly.GetManifestResourceStream("PlainWallet.Resources.Logos.logos.bin")
            ?? throw new InvalidOperationException("The built-in logos resource was not found.");

        using var decompressor = new BrotliStream(stream, CompressionMode.Decompress);

        var logos = JsonSerializer.Deserialize<LogoJson[]>(decompressor)
            ?? throw new InvalidOperationException("The built-in logos resource is empty.");

        return logos
            .Select(logo => new LogoInfo(logo.Name, logo.IsSvg ? Encoding.UTF8.GetBytes(logo.LogoSvg) : Convert.FromBase64String(logo.LogoData), logo.BackgroundColor, logo.IsSvg))
            .ToArray();
    }

    private sealed class LogoJson
    {
        public string Name { get; set; } = string.Empty;
        public string LogoData { get; set; } = string.Empty;
        public string LogoSvg { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public bool IsSvg { get; set; }
    }

    public static IEnumerable<string> GetBuiltInLogoFileNames()
    {
        return _builtIn.Select(x => $"{x.Name}.svg").OrderBy(x => x);
    }

    public static Color GetLogoColor(string filename)
    {
        var logoInfo = FindLogo(filename);
        if (logoInfo is null || string.IsNullOrEmpty(logoInfo.BackgroundColor)) return Colors.Transparent;
        return Color.FromArgb(logoInfo.BackgroundColor);
    }

    public static byte[]? GetLogoDataForBuiltIn(string fileName) => FindLogo(fileName)?.ImageData;

    public static bool IsBuiltInLogoSvg(string fileName) => FindLogo(fileName)?.IsSvg ?? false;

    public static ImageSource? GetImageSourceForBuiltIn(string fileName)
    {
        var logoInfo = FindLogo(fileName);

        if (logoInfo is null) return null;
        if (!logoInfo.IsSvg) return GetImageSourceForLogoData(logoInfo.ImageData);
        return GetImageSourceForLogoData(RasterizeSvg(logoInfo.ImageData));
    }

    public static ImageSource? GetImageSourceForLogoData(byte[]? imageData)
    {
        if (imageData is null || imageData.Length == 0) return null;
        return ImageSource.FromStream(() => new MemoryStream(imageData, writable: false));
    }

    private static byte[]? RasterizeSvg(byte[] svgData, int maxSize = MaxSvgRasterizedSize)
    {
        try
        {
            using var stream = new MemoryStream(svgData, writable: false);
            using var svg = new SKSvg();
            svg.Load(stream);

            var picture = svg.Picture;
            if (picture is null) return null;

            var bounds = picture.CullRect;
            if (bounds.Width <= 0 || bounds.Height <= 0) return null;

            var scale = Math.Min(maxSize / bounds.Width, maxSize / bounds.Height);
            var width = Math.Max(1, (int)Math.Round(bounds.Width * scale));
            var height = Math.Max(1, (int)Math.Round(bounds.Height * scale));

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.Scale(scale);
            canvas.DrawPicture(picture);
            canvas.Flush();

            using var image = SKImage.FromBitmap(bitmap);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
            return encoded.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static LogoInfo? FindLogo(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        var name = Path.GetFileNameWithoutExtension(fileName);
        return _builtIn.FirstOrDefault(logo => string.Equals(logo.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
