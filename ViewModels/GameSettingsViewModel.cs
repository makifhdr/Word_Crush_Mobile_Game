using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MauiApp5.ViewModels;

public class GameSettingsViewModel
{
    public ObservableCollection<int> GridOptions { get; set; }
    public ObservableCollection<int> MoveOptions { get; set; }

    public int SelectedGridSize { get; set; }
    public int SelectedMoves { get; set; }

    public ICommand StartGameCommand { get; }

    public GameSettingsViewModel()
    {
        GridOptions = new ObservableCollection<int> { 6, 8, 10 };
        MoveOptions = new ObservableCollection<int> { 15, 20, 25 };

        StartGameCommand = new Command(OnStartGame);
    }

    private async void OnStartGame()
    {
        try
        {
            if (SelectedGridSize == 0 || SelectedMoves == 0)
            {
                await Application.Current.MainPage.DisplayAlertAsync(
                    "Hata", "Lütfen boş alan bırakmayın.", "Tamam");
                return;
            }
        
            await Shell.Current.GoToAsync(
                $"game?size={SelectedGridSize}&moves={SelectedMoves}");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
}