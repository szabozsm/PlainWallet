using Microsoft.EntityFrameworkCore;
using PlainWallet.Models;

namespace PlainWallet.Data;

public class CardDbContext : DbContext
{
    public CardDbContext(DbContextOptions<CardDbContext> options) : base(options) {
        SQLitePCL.Batteries_V2.Init();
         this.Database.EnsureCreated();
     }

 

      protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<MembershipCard>()
            .Property(e=>e.BackgroundColor)
             .HasConversion(
                c => c.ToArgbHex(true), // Convert Color to int (database storage)
                v => Color.FromArgb(v) // Convert int back to Color (application use)
            );
        }

    public DbSet<MembershipCard> Cards { get; set; } = null!;
}
