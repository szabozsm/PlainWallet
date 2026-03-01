using PlainWallet.Views;

namespace PlainWallet;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(CardDetailPage), typeof(CardDetailPage));
	}
}
