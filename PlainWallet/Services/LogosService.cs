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
    "cvs.svg",
    "ikea.svg",
    "target.svg",
    "whole_foods.svg"
    };

    public static IEnumerable<string> GetBuiltInLogoFileNames() => _builtIn;

    public static ImageSource GetImageSourceForBuiltIn(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null!;
        return ImageSource.FromFile(fileName);
    }
}
