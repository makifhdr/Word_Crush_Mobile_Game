using System.Collections.ObjectModel;
using System.ComponentModel;
using MauiApp5.Models;
using MauiApp5.Services;

namespace MauiApp5.ViewModels;

public class ScoreViewModel: INotifyPropertyChanged
{
    private readonly DataService _dataService;

    public ObservableCollection<GameResult> GameHistory { get; set; } = new();
    
    
    private int _oyunSayisi;
    public int OyunSayisi
    {
        get => _oyunSayisi;
        set { _oyunSayisi = value; OnPropertyChanged(); }
    }

    private int _bestScore;
    public int BestScore
    {
        get => _bestScore;
        set { _bestScore = value; OnPropertyChanged(); }
    }

    private double _averageScore;
    public double AverageScore
    {
        get => _averageScore;
        set { _averageScore = value; OnPropertyChanged(); }
    }
    
    private int _toplamKelime;
    public int ToplamKelime
    {
        get => _toplamKelime;
        set { _toplamKelime = value; OnPropertyChanged(); }
    }
    
    private string _enUzunKelime;
    public string EnUzunKelime
    {
        get => _enUzunKelime;
        set { _enUzunKelime = value; OnPropertyChanged(); }
    }
    
    private string _toplamSure;
    public string ToplamSure
    {
        get => _toplamSure;
        set { _toplamSure = value; OnPropertyChanged(); }
    }

    public ScoreViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    public async Task LoadDataAsync()
    {
        var data = await _dataService.LoadAsync();

        GameHistory.Clear();

        foreach (var game in data.GameHistory.OrderByDescending(g => g.Id))
        {
            game.Id++;
            GameHistory.Add(game);
        }

        CalculateStats();
    }

    private void CalculateStats()
    {
        if (!GameHistory.Any())
        {
            BestScore = 0;
            AverageScore = 0;
            OyunSayisi = 0;
            ToplamKelime = 0;
            EnUzunKelime = "\"Henüz hiç oyun oynanmadı\"";
            ToplamSure ="\"Henüz hiç oyun oynanmadı\"";
            return;
        }

        var enUzunKelime = GameHistory
            .OrderByDescending(x => x.LongestWord.Length)
            .FirstOrDefault().LongestWord;
        
        BestScore = GameHistory.Max(g => g.Score);
        AverageScore = Math.Round(GameHistory.Average(g => g.Score), 2);
        OyunSayisi = GameHistory.Count;
        ToplamKelime = GameHistory.Sum(x => x.WordCount);
        EnUzunKelime = (enUzunKelime.Equals(string.Empty) ? "\"Hiç Kelime Bulunamadı\"" : enUzunKelime);
        ToplamSure = SureFormatla(GameHistory.Sum(x => x.DurationSeconds));
    }
    
    

    private string SureFormatla(int saniye)
    {
        var formatString = "";
        if (saniye < 60)
        {
            return $"{saniye} saniye";
        }

        if (saniye < 3600)
        {
            int dakika = saniye / 60;
            formatString = $"{dakika} dakika ";
            if (saniye % 60 != 0)
            {
                formatString += $"{saniye%60} saniye";
            }
            return formatString;
        }
        
        int saat = saniye / 3600;
        int dakika1 = (saniye / 60) % 60;

        formatString = $"{saat} saat ";

        if (dakika1 != 0)
        {
            formatString += $"{dakika1} dakika ";
        }

        if (saniye % 3600 != 0)
        {
            formatString += $"{saniye%60} saniye";
        }
        
        return formatString;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}