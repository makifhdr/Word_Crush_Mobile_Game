using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp5.Models;
using MauiApp5.Services;

namespace MauiApp5.ViewModels;

public class GameViewModel: INotifyPropertyChanged
{
    public int GridSize { get; set; }
    
    private int _movesLeft;

    public int MovesLeft
    {
        get => _movesLeft;
        set
        {
            _movesLeft = value;
            OnPropertyChanged();
        }
    }
    
    private readonly DataService _dataService = new();
    private AppData _data;
    
    public ObservableCollection<Joker> Jokers { get; set; }

    public ICommand SelectJokerCommand { get; }

    public event Action<JokerType> OnJokerSelected;

    public Dictionary<JokerType, int> JokerNumbers => _data?.JokerNumbers;
    
    public GameViewModel(int size, int moves)
    {
        GridSize = size;
        MovesLeft = moves;
        
        Jokers = new ObservableCollection<Joker>();

        SelectJokerCommand = new Command<Joker>(joker =>
        {
            if (joker.NumberOf > 0)
                OnJokerSelected?.Invoke(joker.JokerTipi);
        });
    }
    
    public async Task LoadAsync()
    {
        _data = await _dataService.LoadAsync();

        Jokers.Clear();

        foreach (var kvp in _data.JokerNumbers)
        {
            Jokers.Add(new Joker
            {
                JokerTipi = kvp.Key,
                NumberOf = kvp.Value,
                ImagePath = GetIcon(kvp.Key)
            });
        }
    }
    
    public void UseMove()
    {
        MovesLeft--;
    }
    
    public async Task ConsumeJoker(JokerType type)
    {
        _data.JokerNumbers[type]--;
        await _dataService.SaveAsync(_data);

        var joker = Jokers.First(j => j.JokerTipi == type);
        
        joker.NumberOf--;
    }
    
    private string GetIcon(JokerType type)
    {
        return type switch
        {
            JokerType.Balik => "balik.png",
            JokerType.Tekerlek => "tekerlek.png",
            JokerType.LolipopKirici => "lolipopkirici.png",
            JokerType.SerbestDegistirme => "serbestdegistirme.png",
            JokerType.HarfKaristirma => "harfkaristirma.png",
            JokerType.PartiGuclendirici => "partiguclendirici.png",
            _ => "?"
        };
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}