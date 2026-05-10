using System.Diagnostics;
using ViewExtensions = Microsoft.Maui.Controls.ViewExtensions;

namespace MauiApp5.Views;

using Cell = MauiApp5.Models.Cell;
public partial class GamePage
{
    private async Task AnimateSwapAsync(Cell cell1, Cell cell2)
    {
        const int steps = 15;
        const int duration = 250;
        int delay = duration / steps;
        
        float diffX = (cell2.Col - cell1.Col) * _cellWidth;
        float diffY = (cell2.Row - cell1.Row) * _cellHeight;

        for (int i = 0; i <= steps; i++)
        {
            float progress = i / (float)steps;
            
            float eased = progress < 0.5f ? 2f * progress * progress : 1f - (float)Math.Pow(-2f * progress + 2f, 2f) / 2f;
            
            cell1.OffsetX = diffX * eased;
            cell1.OffsetY = diffY * eased;
            
            cell2.OffsetX = -diffX * eased;
            cell2.OffsetY = -diffY * eased;

            GameCanvas.InvalidateSurface();
            await Task.Delay(delay);
        }
        
        cell1.OffsetX = 0; cell1.OffsetY = 0;
        cell2.OffsetX = 0; cell2.OffsetY = 0;
    }
    
    private async Task AnimateShuffleAsync()
    {
        const int steps = 20;
        const int duration = 400;
        int delay = duration / steps;

        var positions = new List<(int row, int col)>();
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                positions.Add((i, j));
            }
        }
        
        var shuffledPositions = positions.Shuffle().ToList();
        
        var targetMapping = new Dictionary<Cell, (int targetRow, int targetCol)>();
        int index = 0;
        
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                targetMapping[_board[i, j]] = shuffledPositions[index++];
            }
        }
        
        for (int i = 0; i <= steps; i++)
        {
            float progress = i / (float)steps;
            
            float eased = progress < 0.5f ? 2f * progress * progress : 1f - (float)Math.Pow(-2f * progress + 2f, 2f) / 2f;
            
            foreach (var cell in _board)
            {
                var target = targetMapping[cell];
                
                float diffX = (target.targetCol - cell.Col) * _cellWidth;
                float diffY = (target.targetRow - cell.Row) * _cellHeight;
                
                cell.OffsetX = diffX * eased;
                cell.OffsetY = diffY * eased;
            }

            GameCanvas.InvalidateSurface();
            await Task.Delay(delay);
        }
        
        var newBoard = new Cell[Size, Size];

        foreach (var cell in _board)
        {
            var target = targetMapping[cell];
            
            cell.Row = target.targetRow;
            cell.Col = target.targetCol;
            
            cell.OffsetX = 0;
            cell.OffsetY = 0;
            
            newBoard[target.targetRow, target.targetCol] = cell;
        }
        
        _board = newBoard; 
        GameCanvas.InvalidateSurface();
    }
    
    
    private async void AnimateHamleLabel()
    {
        try
        {
            await KalanHamleLbl.ScaleToAsync(1.5, 100);
            await KalanHamleLbl.ScaleToAsync(1.0, 100);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    
    
    private async Task ShowErrorEffectAsync()
    {
        try
        {
            _isErrorState = true;
            GameCanvas.InvalidateSurface();

            await Task.Delay(500);

            _isErrorState = false;
            _selectedCells.Clear();
            GameCanvas.InvalidateSurface();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    
    private async Task AnimateAll()
    {
        if (_animatingCells.Count == 0) return;

        const int duration = 300;
        const int steps = 20;
        float stepTime = duration / (float)steps;

        for (int i = 0; i < steps; i++)
        {
            float t = (i + 1) / (float)steps;
            float eased = 1f - (1f - t) * (1f - t);

            foreach (var (cell, startOffsetY) in _animatingCells)
            {
                cell.OffsetY = startOffsetY * (1f - eased);
            }

            GameCanvas.InvalidateSurface();
            await Task.Delay((int)stepTime);
        }

        foreach (var (cell, _) in _animatingCells)
            cell.OffsetY = 0;
    }
    
    private async Task AnimateExplosions()
    {
        const int steps = 20;
        const int duration = 300;
        int delay = duration / steps;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;

            lock (_explosions)
            {
                foreach (var exp in _explosions)
                {
                    exp.Radius = exp.MaxRadius * t;
                    exp.Alpha = 255 * (1 - t);
                    
                    _board[exp.Row, exp.Col].Alpha = 255 * (1 - t);
                    _board[exp.Row, exp.Col].Scale = 1f - (0.5f * t);
                }
            }

            GameCanvas.InvalidateSurface();
            await Task.Delay(delay);
        }

        lock (_explosions)
        {
            _explosions.Clear();
        }
    }
    
    

    private async Task PowerUpPuanArttirmaEfekti(int eklenenSkor)
    {
        EklenenPuanLbl.CancelAnimations();
        EklenenPuanLbl.FontAttributes = FontAttributes.Italic;
        EklenenPuanLbl.Text = $"+{eklenenSkor}";
        await EklenenPuanLbl.ScaleToAsync(1.5, 500);
        await EklenenPuanLbl.ScaleToAsync(1.0, 500);
        EklenenPuanLbl.FontAttributes = FontAttributes.None;
        EklenenPuanLbl.Text = string.Empty;
        PuanLbl.Text = $"Skor: {_score}";
    }

    private async Task PuanArttirmaEfekti(int eklenenSkor)
    {
        uint effectLength = 250;
        EklenenPuanLbl.Text = $"+{eklenenSkor}";
        await EklenenPuanLbl.ScaleToAsync(1.25, effectLength);
        await EklenenPuanLbl.ScaleToAsync(1.0, effectLength);
    }
    
    

    private async Task AnimateKelimeUyarisi()
    {
        try
        {
            _isErrorState = true;
            GameCanvas.InvalidateSurface();
            KelimeUyariLbl.Text = "En az 3 harfli bir kelime yazmanız gerek!";
            await Task.Delay(1500);
            KelimeUyariLbl.Text = string.Empty;
            _isErrorState = false;
            _selectedCells.Clear();
            GameCanvas.InvalidateSurface();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}