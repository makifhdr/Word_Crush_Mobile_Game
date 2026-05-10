using MauiApp5.Helpers;
using MauiApp5.Models;
using MauiApp5.Services;
using MauiApp5.ViewModels;

namespace MauiApp5.Views;

public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainMenuViewModel();
    }

    private async void OnGameBtnClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(PageStrings.gameSettings);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    private async void OnScoreBtnClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(PageStrings.score);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    private async void OnMarketBtnClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(PageStrings.market);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}