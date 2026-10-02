using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace PlainWallet.Services;

public static class LogosService
{

    private sealed record LogoInfo(string Name, byte[] ImageData, string BackgroundColor);

    private static readonly LogoInfo[] _builtIn = LoadBuiltInLogos();

    private static LogoInfo[] LoadBuiltInLogos()
    {
        using var stream = typeof(LogosService).Assembly.GetManifestResourceStream("PlainWallet.Resources.Logos.logos.json")
            ?? throw new InvalidOperationException("The built-in logos resource was not found.");
        var logos = JsonSerializer.Deserialize<LogoJson[]>(stream)
            ?? throw new InvalidOperationException("The built-in logos resource is empty.");

        return logos
            .Select(logo => new LogoInfo(logo.Name, Convert.FromBase64String(logo.LogoData), logo.BackgroundColor))
            .ToArray();
    }

    private sealed class LogoJson
    {
        public string Name { get; set; } = string.Empty;
        public string LogoData { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
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

    public static ImageSource? GetImageSourceForBuiltIn(string fileName)
    {
        var logoInfo = FindLogo(fileName);
        if (logoInfo is null) return null;
        return ImageSource.FromStream(() => new MemoryStream(logoInfo.ImageData, writable: false));
    }

    private static LogoInfo? FindLogo(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        var name = Path.GetFileNameWithoutExtension(fileName);
        return _builtIn.FirstOrDefault(logo => string.Equals(logo.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
