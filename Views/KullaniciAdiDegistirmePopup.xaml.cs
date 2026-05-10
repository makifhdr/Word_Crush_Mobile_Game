using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp5.Views;

public partial class KullaniciAdiDegistirmePopup
{
    public KullaniciAdiDegistirmePopup()
    {
        InitializeComponent();
    }

    private void OnTamamClicked(object sender, EventArgs e)
    {
        CloseAsync(UsernameEntry.Text);
    }
    
    private void OnIptalClicked(object sender, EventArgs e)
    {
        CloseAsync(string.Empty);
    }
}