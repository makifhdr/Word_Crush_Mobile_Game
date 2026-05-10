using System.Diagnostics;
using MauiApp5.Services;
using MauiApp5.Views;

namespace MauiApp5;

public partial class App
{
    public App(DataService dataService)
    {
        InitializeComponent();

        if (dataService.IsFirstLaunch())
        {
            MainPage = new NavigationPage(new LoginPage());
        }
        else
        {
            MainPage = new AppShell();
        }
    }
}