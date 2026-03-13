using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace PlainWallet.Services;

public static class LogosService
{
    // Static list of built-in logo filenames (located in Resources/Logos)
    // Update this list if you add/remove files in Resources/Logos
    private static readonly string[] _builtIn = new[]
    {
 "ace_hardware.svg",
"barnes_noble.svg",
"big_lots.svg",
"costco_wholesale.svg",
"cvs.svg",
"decathlon.svg",
"dicks_sporting_goods.svg",
"dm.svg",
"giant_eagle.svg",
"ikea.svg",
"moma.svg",
"target.svg",
"walmart.svg",
"whole_foods.svg"
    };

    public static IEnumerable<string> GetBuiltInLogoFileNames() => _builtIn;

    public static ImageSource GetImageSourceForBuiltIn(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null!;
        return ImageSource.FromFile(fileName);
    }
}
