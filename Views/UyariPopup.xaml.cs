using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;

namespace MauiApp5.Views;

public partial class UyariPopup : Popup
{
    public UyariPopup(string baslik, string mesaj)
    {
        InitializeComponent();

        BaslikLbl.Text = baslik;
        MesajLbl.Text = mesaj; 
    }

    private void OnTamamClicked(object sender, EventArgs e)
    {
        CloseAsync(); 
    }
}