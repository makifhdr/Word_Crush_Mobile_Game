using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;

namespace MauiApp5.Views;

public partial class OnayPopup
{
    public OnayPopup(string baslik, string mesaj)
    {
        InitializeComponent();
        
        BaslikLbl.Text = baslik;
        MesajLbl.Text = mesaj;
    }

    private void OnEvetClicked(object sender, EventArgs e)
    {
        CloseAsync(true);
    }

    private void OnHayirClicked(object sender, EventArgs e)
    {
        CloseAsync(false);
    }
    
}