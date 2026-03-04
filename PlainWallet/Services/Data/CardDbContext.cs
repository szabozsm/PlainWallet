using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlainWallet.Models;

namespace PlainWallet.Services.Data;

public class CardDbContext : DbContext
{
    public CardDbContext(DbContextOptions<CardDbContext> options) : base(options)
    {
          //SQLitePCL.Batteries_V2.Init();
           this.Database.EnsureCreated();
    }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MembershipCard>()
            .Property(t => t.BackgroundColor)
            .HasConversion<ColorToInt32Converter>(); // Apply the custom converter
    }

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
 
 optionsBuilder
        .UseSeeding((context, _) =>
        {
//   var random = new Random();
//         var names = new[] { "SuperMart Club", "City Gym", "Book Lovers", "Cinema Stars", "Coffee Points", "Tech Store Plus" };
//         var notes = new[] { "Show at checkout to collect points.", "Access pass for all locations.", "10% off all paperbacks.", "Free popcorn every 5 visits.", "Every 7th drink is free.", "Extended warranty on all gadgets." };
//         var colors = new[] { Colors.DeepSkyBlue, Colors.MediumPurple, Colors.OrangeRed, Colors.SeaGreen, Colors.Goldenrod, Colors.CadetBlue };
//         var barcodeFormats = new[] { ZXing.Net.Maui.BarcodeFormat.Code128, ZXing.Net.Maui.BarcodeFormat.Code39, ZXing.Net.Maui.BarcodeFormat.Code93, ZXing.Net.Maui.BarcodeFormat.Ean13 };

//         for (int i = 0; i < 8; i++)
//         {
//             var index = random.Next(names.Length);
//             var card = new MembershipCard
//             {
//                 Name = names[index],
//                 CardNumber = $"{random.Next(1000, 9999)} {random.Next(1000, 9999)} {random.Next(1000, 9999)}",
//                 Logo = ImageSource.FromFile("dotnet_bot.png"),
//                 BackgroundColor = colors[random.Next(colors.Length)],
//                 Notes = notes[index],
//                 BarcodeTypeValue = (int)barcodeFormats[random.Next(barcodeFormats.Length)]
//             };

//             context.Set<MembershipCard>().Add(card);
           
//             Cards.Add(card);
//         }
//         context.SaveChanges();


        });


    }

    public DbSet<MembershipCard> Cards { get; set; } = null!;
}
public class ColorToInt32Converter : ValueConverter<Color, string>
{
    public ColorToInt32Converter()
        : base(
            c => c.ToArgbHex(true), // Convert Color to int (database storage)
            v => Color.FromArgb(v) // Convert int back to Color (application use)
        ) { }
}
