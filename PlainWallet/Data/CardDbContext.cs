using Microsoft.EntityFrameworkCore;
using PlainWallet.Models;

namespace PlainWallet.Data;

public class CardDbContext : DbContext
{
    public CardDbContext(DbContextOptions<CardDbContext> options) : base(options) {
         this.Database.EnsureCreated();
     }

    public DbSet<MembershipCard> Cards { get; set; } = null!;
}
