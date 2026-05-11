using MauiApp5.Models;
using MauiApp5.Services;

namespace MauiApp5.Views;

public partial class LoginPage : ContentPage
{
    private DataService _dataService;
    public LoginPage()
    {
        InitializeComponent();
        _dataService = new DataService();
    }
    
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text.Trim();
        
        if (string.IsNullOrEmpty(username))
        {
            ErrorLabel.Text = "Kullanıcı adı boş olamaz!";
            ErrorLabel.IsVisible = true;
            return;
        }

        var data = new AppData
        {
            UserName = username,
            Gold = 0,
            GameHistory = [],
            JokerNumbers = new Dictionary<JokerType, int>
            {
                { JokerType.Balik, 0},
                { JokerType.HarfKaristirma, 0},
                { JokerType.LolipopKirici, 0},
                { JokerType.PartiGuclendirici, 0},
                { JokerType.SerbestDegistirme, 0},
                { JokerType.Tekerlek, 0},
            },
            NextGameId = 0
        };

        await _dataService.SaveAsync(data);

        Application.Current.MainPage = new AppShell();
    }
}