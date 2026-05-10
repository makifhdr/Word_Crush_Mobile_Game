using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using MauiApp5.Models;
using MauiApp5.Services;
using MauiApp5.Views;

namespace MauiApp5.ViewModels;

public class MainMenuViewModel: INotifyPropertyChanged
{
    private string _userName;

    public string UserName
    {
        get => _userName;
        set
        {
            _userName = value;
            OnPropertyChanged();
        }
    }
    
    public ICommand ChangeUsernameCommand { get; }

    private DataService _dataService;
    private AppData _data;

    public MainMenuViewModel()
    {
        ChangeUsernameCommand = new Command(async void () =>
        {
            try
            {
                await ChangeUsername();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        });
        
        LoadUser();
        
    }
    
    private async Task ChangeUsername()
    {
        var currentPage = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
        
        if (currentPage == null) 
            return;
        
        var popup = new KullaniciAdiDegistirmePopup();
        
        var result = await currentPage.ShowPopupAsync<string>(popup, new PopupOptions()
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        });
        
        var newName = result.Result;
        if (string.IsNullOrWhiteSpace(newName))
            return;
        
        _dataService = new DataService();
        _data = await _dataService.LoadAsync();

        UserName = newName;
        _data.UserName = newName;

        await _dataService.SaveAsync(_data);

        OnPropertyChanged(nameof(UserName));
    }

    private async void LoadUser()
    {
        try
        {
            _dataService = new DataService();
            _data = await _dataService.LoadAsync();

            UserName = _data.UserName;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}