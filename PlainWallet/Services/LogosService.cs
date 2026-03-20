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
new LogoInfo("ace_hardware.svg",""), 
new LogoInfo("barnes_noble.svg",""), 
new LogoInfo("big_lots.svg",""), 
new LogoInfo("cleveland_botanical_garden.svg",""), 
new LogoInfo("cleveland_metroparks_zoo.svg",""), 
new LogoInfo("cma_cleveland_museum_of_art.svg",""), 
new LogoInfo("columbus_zoo.svg",""), 
new LogoInfo("costco_wholesale.svg",""), 
new LogoInfo("cuyahoga_county_public_library.svg",""), 
new LogoInfo("cvs.svg","#cc0000ff"), 
new LogoInfo("decathlon.svg","#1482c2ff"), 
new LogoInfo("dicks_sporting_goods.svg","#006554ff"), 
new LogoInfo("dm.svg",""), 
new LogoInfo("giant_eagle.svg",""), 
new LogoInfo("heinens.svg",""), 
new LogoInfo("holden_arboretum.svg",""), 
new LogoInfo("ikea.svg","#2360a5ff"), 
new LogoInfo("lake_metroparks.svg",""), 
new LogoInfo("lakeshore_learning.svg","#ed1c24ff"), 
new LogoInfo("lego.svg","#ff0000ff"), 
new LogoInfo("lidl.svg","#1e71b8ff"), 
new LogoInfo("lowe_s.svg",""), 
new LogoInfo("moma.svg",""), 
new LogoInfo("museum_of_natural_history_cleveland.svg",""), 
new LogoInfo("ollie_s.svg",""), 
new LogoInfo("panera_bread.svg","#606b21ff"), 
new LogoInfo("pet_supplies_plus.svg",""), 
new LogoInfo("petsmart.svg",""), 
new LogoInfo("shaker_library.svg",""), 
new LogoInfo("target.svg",""), 
new LogoInfo("tesco.svg",""), 
new LogoInfo("vitamin_shoppe.svg",""), 
new LogoInfo("walgreens.svg",""), 
new LogoInfo("walmart.svg",""), 
new LogoInfo("whole_foods.svg",""), 

    ];

    public static IEnumerable<string> GetBuiltInLogoFileNames()
    {
        return _builtIn.Select(x => x.FileName);
    }

    public static Color GetLogoColor(string filename)
    {
        var logoInfo = _builtIn.FirstOrDefault(x => x.FileName == filename);
        if (logoInfo == null) return Colors.Transparent;
        if (string.IsNullOrEmpty( logoInfo.Color)) return Colors.Transparent;
        return Color.FromRgba(logoInfo.Color);
    }

    public static ImageSource GetImageSourceForBuiltIn(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null!;
        return ImageSource.FromFile(fileName);
    }
}
