using System.Collections.ObjectModel;
using PlainWallet.Models;

namespace PlainWallet;

public partial class MainPage : ContentPage
{
    public ObservableCollection<MembershipCard> Cards { get; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadSampleCards();
    }

    private void LoadSampleCards()
    {
        Cards.Add(new MembershipCard
        {
            Name = "SuperMart Club",
            CardNumber = "1234 5678 9012",
            Logo = "dotnet_bot.png",
            BackgroundColor = Colors.DeepSkyBlue,
            Notes = "Show at checkout to collect points."
        });

        Cards.Add(new MembershipCard
        {
            Name = "City Gym",
            CardNumber = "GYM-987654",
            Logo = "dotnet_bot.png",
            BackgroundColor = Colors.MediumPurple,
            Notes = "Access pass for all locations."
        });

        Cards.Add(new MembershipCard
        {
            Name = "Book Lovers",
            CardNumber = "BL-2024-001",
            Logo = "dotnet_bot.png",
            BackgroundColor = Colors.OrangeRed,
            Notes = "10% off all paperbacks."
        });
    }
}

