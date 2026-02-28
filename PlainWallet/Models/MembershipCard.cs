namespace PlainWallet.Models;

public class MembershipCard
{
    public string CardNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public Color BackgroundColor { get; set; } = Colors.Gray;
    public string Notes { get; set; } = string.Empty;
}
