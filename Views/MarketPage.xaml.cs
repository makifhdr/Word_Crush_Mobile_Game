using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        try
        {
            base.OnAppearing();
            await _vm.LoadAsync();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}