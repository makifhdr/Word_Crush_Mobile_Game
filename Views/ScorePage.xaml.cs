using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp5.Services;
using MauiApp5.ViewModels;

namespace MauiApp5.Views;

public partial class ScorePage : ContentPage
{
    private ScoreViewModel _viewModel;

    public ScorePage()
    {
        InitializeComponent();

        _viewModel = new ScoreViewModel(new DataService());
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }
}