using PlainWallet.Models;

namespace PlainWallet.Services;

/// <summary>
/// Holds the currently selected card when navigating to the detail screen.
/// Shell query parameters only support primitives, so we pass the card via this holder.
/// </summary>
public static class CardNavigation
{
    public static MembershipCard? SelectedCard { get; set; }
}
