using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;

namespace MauiApp5.Views;

public partial class OyunSonucuPopup : Popup
{
    public OyunSonucuPopup(int skor, string sure, int bulunanKelimeSayisi, string enUzunkelime, int kazanilanAltin)
    {
        InitializeComponent();

        BaslikLbl.Text = "Oyun Bitti!";
        SkorLbl.Text = skor.ToString();
        SureLbl.Text = sure;
        KelimeSayisiLbl.Text = bulunanKelimeSayisi.ToString();
        EnUzunKelimeLbl.Text = enUzunkelime;
        KazanilanAltinLbl.Text = kazanilanAltin.ToString();
    }
    
    private void OnTamamClicked(object sender, EventArgs e)
    {
        CloseAsync(); 
    }
}