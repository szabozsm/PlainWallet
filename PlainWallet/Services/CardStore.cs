using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using PlainWallet.Data;
using PlainWallet.Models;

namespace PlainWallet.Services;

/// <summary>
/// Shared store for membership cards so MainPage and NewCardPage use the same list.
/// This static store is backed by a SQLite database via EF Core. Call Initialize at startup.
/// </summary>
public static class CardStore
{
    public static ObservableCollection<MembershipCard> Cards { get; } = new();

    public static void Initialize(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<CardDbContext>();
        ctx.Database.EnsureCreated();

        Cards.Clear();
        foreach (var c in ctx.Cards.ToList())
        {
            Cards.Add(c);
            SubscribeCard(c, services);
        }

        Cards.CollectionChanged += (s, e) =>
        {
            using var innerScope = services.CreateScope();
            var innerCtx = innerScope.ServiceProvider.GetRequiredService<CardDbContext>();
            if (e.NewItems != null)
            {
                foreach (MembershipCard item in e.NewItems)
                {
                    innerCtx.Cards.Add(item);
                }
            }
            if (e.OldItems != null)
            {
                foreach (MembershipCard item in e.OldItems)
                {
                    var tracked = innerCtx.Cards.Local.FirstOrDefault(x => x.Id == item.Id) ?? innerCtx.Cards.Find(item.Id);
                    if (tracked != null) innerCtx.Cards.Remove(tracked);
                }
            }
            innerCtx.SaveChanges();

            if (e.NewItems != null)
            {
                foreach (MembershipCard item in e.NewItems)
                    SubscribeCard(item, services);
            }
        };
    }

    private static void SubscribeCard(MembershipCard card, IServiceProvider services)
    {
        card.PropertyChanged += (_, args) =>
        {
            using var scope = services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<CardDbContext>();
            var tracked = ctx.Cards.Local.FirstOrDefault(x => x.Id == card.Id) ?? ctx.Cards.Find(card.Id);
            if (tracked == null)
            {
                ctx.Cards.Attach(card);
                ctx.Entry(card).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            else
            {
                // Mark the tracked entity as Modified to ensure EF Core detects changes
                ctx.Entry(tracked).CurrentValues.SetValues(card);
                // ctx.Entry(tracked).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            ctx.SaveChanges();
        };
    }
}
