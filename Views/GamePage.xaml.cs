using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using MauiApp5.Helpers;
using MauiApp5.ViewModels;
using MauiApp5.Models;
using MauiApp5.Services;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using Cell = MauiApp5.Models.Cell;
using Debug = System.Diagnostics.Debug;

namespace MauiApp5.Views;

[QueryProperty(nameof(Size), "size")]
[QueryProperty(nameof(Moves), "moves")]
public partial class GamePage
{
    public int Size { get; set; }
    public int Moves { get; set; }
    
    private Cell[,] _board = null!;
    
    private float _cellWidth;
    private float _cellHeight;
    private List<Cell> _selectedCells = [];
    private List<string> _bulunanKelimeler = [];
    private bool _isSelecting;
    private bool _isErrorState;
    private DateTime _startingTime = DateTime.Now;
    private int _score;
    private HashSet<string> _wordSet = null!;
    private Dictionary<char, int> _charScores;
    private int _kalanKelimeSayisi;
    private JokerType _selectedJoker = JokerType.Null;
    private bool _isProcessing;

    private DataService _dataService;
    private WordService _wordService;
    private AppData _data = null!;
    
    private SKTypeface _customFont = null!;
    
    private bool _isGameInitialized = false;
    
    public GamePage(DataService dataService, WordService wordService)
    {
        _dataService = dataService;
        _wordService = wordService;

        _charScores = new Dictionary<char, int>
        {
            {'A', 1}, {'B', 3}, {'C', 4}, {'Ç', 4}, {'D', 3}, {'E', 1}, {'F', 7}, {'G', 5}, {'Ğ', 8}, {'H', 5},
            {'I', 2}, {'İ', 1}, {'J', 10}, {'K', 1}, {'L', 1}, {'M', 2}, {'N', 1}, {'O', 2}, {'Ö', 7}, {'P', 5},
            {'R', 1}, {'S', 2}, {'Ş', 4}, {'T', 1}, {'U', 2}, {'Ü', 3}, {'V', 7}, {'Y', 3}, {'Z', 4}
        };
        
        InitializeComponent();
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (_isGameInitialized) return;
        try
        {
            await using var fontStream = await FileSystem.OpenAppPackageFileAsync("VarelaRound-Regular.ttf");
            _customFont = SKTypeface.FromStream(fontStream);
            
            _board = new Cell[Size, Size];
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _board[i, j] = new Cell { Row = i, Col = j, Letter = '\0' };
                }
            }
            _wordSet = await _wordService.LoadWordsAsync();
            FillEmptyCells();

            _kalanKelimeSayisi = KalanKelimeler().Count;
            while (_kalanKelimeSayisi == 0)
            {
                GenerateSmartBoard();
                _kalanKelimeSayisi = KalanKelimeler().Count;
            }
            GameCanvas.InvalidateSurface();
            Debug.WriteLine($"Kalan kelime sayısı: {_kalanKelimeSayisi}");

            _data = await _dataService.LoadAsync();

            var vm = new GameViewModel(Size, Moves);
            BindingContext = vm;
            await vm.LoadAsync();
            
            vm.OnJokerSelected += async (jokerType) =>
            {
                
                if (jokerType is JokerType.HarfKaristirma or JokerType.PartiGuclendirici or JokerType.Balik)
                {
                    _selectedJoker = JokerType.Null; 
                    
                    GorselJokerSeciminiGuncelle(jokerType); 
                    
                    UseJoker(jokerType, 0, 0); 
                }
                else
                {
                    if (_selectedJoker == jokerType)
                    {
                        _selectedJoker = JokerType.Null;
                        GorselJokerSeciminiGuncelle(JokerType.Null);
                    }
                    else
                    {
                        _selectedJoker = jokerType;
                        GorselJokerSeciminiGuncelle(jokerType);
                    }
                }
            };
            _isGameInitialized = true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    private async void OnCanvasTouch(object sender, SKTouchEventArgs e)
    {
        try
        {
            if (_isProcessing) return;
            float touchX = e.Location.X;
            float touchY = e.Location.Y;

            int col = (int)(touchX / _cellWidth);
            int row = (int)(touchY / _cellHeight);

            bool isInsideGrid = row >= 0 && row < Size && col >= 0 && col < Size;
            
            if (_selectedJoker != JokerType.Null && e.ActionType == SKTouchAction.Pressed)
            {
                if (!isInsideGrid) return;
                var activeJoker = _selectedJoker;
                
                if (activeJoker == JokerType.SerbestDegistirme && _swapFirstCell.Letter == '\0')
                {
                    UseJoker(activeJoker, row, col);
                }
                else
                {
                    _selectedJoker = JokerType.Null;
                    UseJoker(activeJoker, row, col);
                }

                return;
            }
        
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (isInsideGrid)
                    {
                        _isErrorState = false;
                        _selectedCells.Clear();
                        _selectedCells.Add(_board[row, col]);
                        _isSelecting = true;
                        GameCanvas.InvalidateSurface(); 
                        YazilanKelimeLbl.Text = GetWord();
                    }
                    break;

                case SKTouchAction.Moved:
                    if (_isSelecting && isInsideGrid)
                    {
                        var currentCell = _board[row, col];
                        var lastCell = _selectedCells.Last();

                        if (currentCell != lastCell)
                        {
                            if (_selectedCells.Count > 1 && _selectedCells[_selectedCells.Count - 2] == currentCell)
                            {
                                _selectedCells.RemoveAt(_selectedCells.Count - 1);
                                GameCanvas.InvalidateSurface();
                                YazilanKelimeLbl.Text = GetWord();
                            }
                            else if (!_selectedCells.Contains(currentCell) && IsNeighbor(lastCell, currentCell))
                            {
                                float cellCenterX = (col * _cellWidth) + (_cellWidth / 2f);
                                float cellCenterY = (row * _cellHeight) + (_cellHeight / 2f);
                            
                                double distanceToCenter = Math.Sqrt(Math.Pow(touchX - cellCenterX, 2) + Math.Pow(touchY - cellCenterY, 2));
                            
                                double selectionThreshold = Math.Min(_cellWidth, _cellHeight) * 0.35;

                                if (distanceToCenter <= selectionThreshold)
                                {
                                    _selectedCells.Add(currentCell);
                                    GameCanvas.InvalidateSurface();
                                    YazilanKelimeLbl.Text = GetWord();
                                }
                            }
                        }
                    }
                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    if (_isSelecting)
                    {
                        _isSelecting = false;
                        _isProcessing = true;
                        string word = GetWord();

                        if (word.Length < 3)
                        {
                            await AnimateKelimeUyarisi();
                        }
                        else if(IsWordValid(word))
                        {
                            _bulunanKelimeler.Add(word);
                            Debug.WriteLine($"Bulunan Kelime: {word}");
                            
                            await TriggerPowerUps();
                        
                            await HandleWordFound(word);
                            (BindingContext as GameViewModel)?.UseMove();
                            Debug.WriteLine($"Kalan kelime sayısı: {_kalanKelimeSayisi}");
                        }
                        else
                        {
                            AnimateHamleLabel();
                            (BindingContext as GameViewModel)?.UseMove();
                            await ShowErrorEffectAsync();
                        }

                        YazilanKelimeLbl.Text = "";
                        _isProcessing = false;
                    }
                    break;
            }

            if ((BindingContext as GameViewModel)?.MovesLeft <= 0)
            {
                OyunuBitir();
            }

            e.Handled = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    
    private async Task<GameResult> OyunSonucuAl()
    {
        _data = await _dataService.LoadAsync();
        var gameResult = new GameResult
        {
            Id = _data.NextGameId,
            Date = _startingTime,
            Score = _score,
            DurationSeconds = (int)DateTime.Now.Subtract(_startingTime).TotalSeconds,
            GridSize = Size,
            LongestWord = _bulunanKelimeler.Count == 0
                ? string.Empty 
                : _bulunanKelimeler.OrderByDescending(x => x.Length).FirstOrDefault() ?? string.Empty,
            WordCount = _bulunanKelimeler.Count
        };
        _data.GameHistory.Add(gameResult);
        _data.NextGameId++;
        await _dataService.SaveAsync(_data);
        
        return gameResult;
    }

    private async void OyunuBitir()
    {
        try
        {
            var popup = new OyunSonucuPopup("Oyun Bitti", "Hamle kalmadı");
            await this.ShowPopupAsync(popup);
            var gameResult = await OyunSonucuAl();
            
            await OyunSonucuGoruntule(gameResult);
            await Shell.Current.GoToAsync(PageStrings.mainMenu);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }

    private async Task OyunSonucuGoruntule(GameResult gameResult)
    {
        try
        {
            var enUzunKelime = gameResult.LongestWord.Equals(string.Empty) ? "\"Kelime Bulunamadı\"" : gameResult.LongestWord;
            
            var oyunSonucuString = $"Skor: {gameResult.Score}\n" +
                                   $"Süre: {gameResult.SureString}\n" +
                                   $"Bulunan kelime: {gameResult.WordCount}\n" +
                                   $"Bulunan en uzun kelime: {enUzunKelime}";

            var baslikString = "Oyun Bitti!";
            
            var popup = new OyunSonucuPopup(baslikString, oyunSonucuString);
            await this.ShowPopupAsync(popup, new PopupOptions()
            {
                CanBeDismissedByTappingOutsideOfPopup = false
            });
        }catch(Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    public async Task<bool> ConfirmExitAsync()
    {
        try
        {
            var popup = new OnayPopup("Oyundan Çık", "Oyundan çıkmak istediğinize emin misiniz? Oynadığınız oyun skor tablosuna kaydedilecektir.");
            
            var result = await this.ShowPopupAsync<bool>(popup, new PopupOptions()
            {
                CanBeDismissedByTappingOutsideOfPopup = false
            });
            
            if (result.Result)
            {
                var gameResult = await OyunSonucuAl();
                await OyunSonucuGoruntule(gameResult);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return false;
    }
}

class Explosion
{
    public float X;
    public float Y;
    public float Radius;
    public float MaxRadius;
    public float Alpha;
    public int Row;
    public int Col;
}