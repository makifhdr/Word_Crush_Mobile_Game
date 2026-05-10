using MauiApp5.Helpers;
using MauiApp5.Views;

namespace MauiApp5;

public partial class AppShell
{
    public AppShell()
    {
        Routing.RegisterRoute(PageStrings.game, typeof(GamePage));
        Routing.RegisterRoute(PageStrings.gameSettings, typeof(GameSettingsPage));
        Routing.RegisterRoute(PageStrings.mainMenu, typeof(MainPage));
        Routing.RegisterRoute(PageStrings.score, typeof(ScorePage));
        Routing.RegisterRoute(PageStrings.market, typeof(MarketPage));
        InitializeComponent();
        Navigating += OnNavigating;
    }
    
    private async void OnNavigating(object sender, ShellNavigatingEventArgs e)
    {
        try
        {
            if (e.Source == ShellNavigationSource.Pop)
            {
                var currentPage = Current.CurrentPage;

                if (currentPage is GamePage gamePage)
                {
                    e.Cancel();

                    bool answer = await gamePage.ConfirmExitAsync();

                    if (answer)
                    {
                        await Current.GoToAsync(PageStrings.mainMenu);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}