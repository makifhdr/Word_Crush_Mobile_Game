using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp5.Models;

namespace MauiApp5.Views;

public partial class JokerDetailPage : ContentPage
{
    public JokerDetailPage(Joker joker)
    {
        InitializeComponent();

        NameLbl.Text = joker.Name;
        DescLbl.Text = joker.Description;
        GifImage.Source = joker.GifPath;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}