using System.Diagnostics;
using MauiApp5.ViewModels;

namespace MauiApp5.Views;

public partial class MarketPage
{
    private readonly MarketViewModel _vm;

    public MarketPage()
    {
        InitializeComponent();
        _vm = new MarketViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _vm.LoadAsync();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}