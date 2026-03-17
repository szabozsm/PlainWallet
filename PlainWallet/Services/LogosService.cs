using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace PlainWallet.Services;

public static class LogosService
{

    private record LogoInfo(string FileName, string Color);

    // Static list of built-in logo filenames (located in Resources/Logos)
    // Update this list if you add/remove files in Resources/Logos
    private static readonly LogoInfo[] _builtIn =
    [
new LogoInfo("ace_hardware.svg","#FF00FF00"), 
new LogoInfo("barnes_noble.svg",""), 
new LogoInfo("big_lots.svg",""), 
new LogoInfo("costco_wholesale.svg",""), 
new LogoInfo("cvs.svg",""), 
new LogoInfo("decathlon.svg",""), 
new LogoInfo("dicks_sporting_goods.svg",""), 
new LogoInfo("dm.svg",""), 
new LogoInfo("giant_eagle.svg",""), 
new LogoInfo("heinens.svg",""), 
new LogoInfo("ikea.svg",""), 
new LogoInfo("lakeshore_learning.svg",""), 
new LogoInfo("lego.svg",""), 
new LogoInfo("lidl.svg",""), 
new LogoInfo("lowe_s.svg",""), 
new LogoInfo("moma.svg",""), 
new LogoInfo("ollie_s.svg",""), 
new LogoInfo("panera_bread.svg",""), 
new LogoInfo("pet_supplies_plus.svg",""), 
new LogoInfo("petsmart.svg",""), 
new LogoInfo("target.svg",""), 
new LogoInfo("tesco.svg",""), 
new LogoInfo("walgreens.svg",""), 
new LogoInfo("walmart.svg",""), 
new LogoInfo("whole_foods.svg",""), 

    ];

    public static IEnumerable<string> GetBuiltInLogoFileNames()
    {
        return _builtIn.Select(x => x.FileName);
    }

    public static ImageSource GetImageSourceForBuiltIn(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null!;
        return ImageSource.FromFile(fileName);
    }
}
