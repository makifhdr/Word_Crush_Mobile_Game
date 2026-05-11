using System.Diagnostics;
using MauiApp5.Models;
using MauiApp5.ViewModels;
using Cell = Microsoft.Maui.Controls.Cell;

namespace MauiApp5.Views;

public partial class GamePage
{
    private async Task TriggerPowerUps()
    {
        try
        {
            foreach (var cell in _selectedCells)
            {
                if (cell.Power != PowerType.None)
                {
                    await ApplyPower(cell);
                    cell.Power = PowerType.None;
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    private async Task ApplyPower(Models.Cell cell)
    {
        try
        {
            switch (cell.Power)
            {
                case PowerType.ClearRow:
                    await ClearRow(cell.Row);
                    break;

                case PowerType.ClearColumn:
                    await ClearColumn(cell.Col);
                    break;

                case PowerType.ClearNeighbors:
                    await ClearNeighbors(cell.Row, cell.Col);
                    break;

                case PowerType.ClearBigArea:
                    await ClearBigArea(cell.Row, cell.Col);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    private async Task ClearRow(int row)
    {
        try
        {
            var harfler = "";
            for (int col = 0; col < Size; col++)
            {
                bool isTriggerCell = _selectedCells.Count > 0 && _selectedCells.Last() == _board[row, col];
                
                if (!isTriggerCell && _board[row, col].Letter != '\0')
                {
                    harfler += _board[row, col].Letter;
                    _board[row, col].Letter = '\0';
                    
                    AddToExplosions(row,col);
                }
            }
            
            int skor = CalculateWordScore(harfler);
            _score += skor;
            
            await PowerUpPuanArttirmaEfekti(skor);
            
            Debug.WriteLine("Satır temizlendi");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    private async Task ClearColumn(int col)
    {
        try
        {
            var harfler = "";
            for (int row = 0; row < Size; row++)
            {
                bool isTriggerCell = _selectedCells.Count > 0 && _selectedCells.Last() == _board[row, col];
                
                if (!isTriggerCell && _board[row, col].Letter != '\0')
                {
                    harfler += _board[row, col].Letter;
                    _board[row, col].Letter = '\0';
                    
                    AddToExplosions(row,col);
                }
            }
            
            int skor = CalculateWordScore(harfler);
            _score += skor;
            
            await PowerUpPuanArttirmaEfekti(skor);
            Debug.WriteLine("Sütun temizlendi");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    private async Task ClearNeighbors(int row, int col)
    {
        try
        {
            var directions = new (int dx, int dy)[]
            {
                (1,0),(-1,0),(0,1),(0,-1),
                (1,1),(-1,-1),(1,-1),(-1,1)
            };

            var harfler = "";
            foreach (var dir in directions)
            {
                int r = row + dir.dy;
                int c = col + dir.dx;
                
                if (r >= 0 && r < Size && c >= 0 && c < Size && _selectedCells.Last() != _board[r, c])
                {
                    harfler += _board[r, c].Letter;
                    _board[r, c].Letter = '\0';
                    
                    AddToExplosions(r,c);
                }
            }
            int skor = CalculateWordScore(harfler);
            _score += skor;
            
            await PowerUpPuanArttirmaEfekti(skor);
            
            Debug.WriteLine("Komşular temizlendi");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    private async Task ClearBigArea(int row, int col)
    {
        try
        {
            var harfler = "";
            for (int r = row - 2; r <= row + 2; r++)
            {
                for (int c = col - 2; c <= col + 2; c++)
                {
                    if (r >= 0 && r < Size && c >= 0 && c < Size && _selectedCells.Last() != _board[r, c])
                    {
                        harfler += _board[r, c].Letter;
                        _board[r, c].Letter = '\0';

                        AddToExplosions(r, c);
                    }
                }
            }
            int skor = CalculateWordScore(harfler);
            _score += skor;
            
            await PowerUpPuanArttirmaEfekti(skor);
            Debug.WriteLine("Büyük komşular temizlendi");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    private async Task UseBalikJoker()
    {
        var random = new Random();

        int destroyCount = random.Next(3,10);

        var harfler = "";

        for (int i = 0; i < destroyCount; i++)
        {
            int r = random.Next(Size);
            int c = random.Next(Size);

            var harf = _board[r, c].Letter;

            if (harf != '\0')
            {
                harfler += _board[r, c].Letter;
            
                _board[r, c].Letter = '\0';
                _board[r, c].Power = PowerType.None;

                AddToExplosions(r, c);
            }
            else
            {
                i--;
            }
        }
        

        var eklenenSkor = CalculateWordScore(harfler);
        
        _score += eklenenSkor;
        await PowerUpPuanArttirmaEfekti(eklenenSkor);
        GameCanvas.InvalidateSurface();
    }
    
    private async Task UseTekerlekJoker(int row, int col)
    {
        try
        {
            await ClearRow(row);
            await ClearColumn(col);
            GameCanvas.InvalidateSurface();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
    
    private async Task UseLollipop(int row, int col)
    {
        var eklenenSkor = CalculateWordScore(_board[row, col].Letter.ToString());
        _score += eklenenSkor;
        await PowerUpPuanArttirmaEfekti(eklenenSkor);
        _board[row, col].Letter = '\0';
        _board[row, col].Power = PowerType.None;
        AddToExplosions(row, col);
        GameCanvas.InvalidateSurface();
    }
    
    private Models.Cell _swapFirstCell = new();
    
    private async Task<bool> UseSwapJoker(int row, int col)
    {
        var cell = _board[row, col];
        
        if (_swapFirstCell.Letter == '\0')
        {
            _swapFirstCell = cell;
            GameCanvas.InvalidateSurface(); 
            return false;
        }
        
        bool basarili = false;
        
        if (IsTouching(_swapFirstCell, cell))
        {
            await AnimateSwapAsync(_swapFirstCell, cell);
            
            (_swapFirstCell.Letter, cell.Letter) = (cell.Letter, _swapFirstCell.Letter);
            (_swapFirstCell.Power, cell.Power) = (cell.Power, _swapFirstCell.Power);
            basarili = true; 
        }
        
        _swapFirstCell = new Models.Cell();
        
        GameCanvas.InvalidateSurface(); 
        
        return basarili;
    }
    
    private async Task UseShuffleJoker()
    {
        await AnimateShuffleAsync();
    }
    
    private async Task UsePartyJoker()
    {
        var harfler = "";
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                harfler += _board[i, j].Letter;
                _board[i, j].Letter = '\0';
                _board[i, j].Power = PowerType.None;
                AddToExplosions(i, j);
            }
        }
        
        var eklenenSkor = CalculateWordScore(harfler);
        _score += eklenenSkor;

        await PowerUpPuanArttirmaEfekti(eklenenSkor);
        GameCanvas.InvalidateSurface();
    }
    
    private async void UseJoker(JokerType type, int row, int col)
    {
        try
        {
            _isProcessing = true;
            bool isProcessComplete = true;
            try
            {
                switch (type)
                {
                    case JokerType.Balik:
                        await UseBalikJoker();
                        break;
                    case JokerType.Tekerlek:
                        await UseTekerlekJoker(row, col);
                        break;
                    case JokerType.LolipopKirici:
                        await UseLollipop(row, col);
                        break;
                    case JokerType.SerbestDegistirme:
                        if (!await UseSwapJoker(row, col))
                        {
                            isProcessComplete = _swapFirstCell.Letter == '\0';

                            return;
                        }
                        break;
                    case JokerType.HarfKaristirma:
                        await UseShuffleJoker();
                        break;
                    case JokerType.PartiGuclendirici:
                        await UsePartyJoker();
                        break;
                }
            
                bool hasExplosions;
            
                lock (_explosions)
                {
                    hasExplosions = _explosions.Count > 0;
                }
            
                if (hasExplosions)
                {
                    await AnimateExplosions();
                }

                if (BindingContext is GameViewModel vm) await vm.ConsumeJoker(type);

                GameCanvas.InvalidateSurface();
            
                PrepareGravityAndFill();
                await AnimateAll();
            
                _kalanKelimeSayisi = KalanKelimeler().Count;
                PuanLbl.Text = $"Skor: {_score}";
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                _isProcessing = false;
            
                if (isProcessComplete)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _selectedJoker = JokerType.Null;
                        GorselJokerSeciminiGuncelle(JokerType.Null);
                    });
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
    
    private void GorselJokerSeciminiGuncelle(JokerType type)
    {
        if (BindingContext is not GameViewModel vm) return;
        
        for (int i = 0; i < vm.Jokers.Count; i++)
        {
            var joker = vm.Jokers[i];
            bool olmasiGerekenDurum = (joker.JokerTipi == type);
            
            if (joker.IsSelected != olmasiGerekenDurum)
            {
                joker.IsSelected = olmasiGerekenDurum;
                vm.Jokers[i] = joker;
            }
        }
    }
}