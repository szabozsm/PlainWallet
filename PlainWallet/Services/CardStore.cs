using System.Collections.ObjectModel;
using PlainWallet.Models;

namespace PlainWallet.Services;

/// <summary>
/// Shared store for membership cards so MainPage and NewCardPage use the same list.
/// </summary>
public static class CardStore
{
    public static ObservableCollection<MembershipCard> Cards { get; } = new();
}
