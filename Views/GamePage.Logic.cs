using System.Diagnostics;
using MauiApp5.Models;
using Cell = MauiApp5.Models.Cell;

namespace MauiApp5.Views;

public partial class GamePage
{
    private bool PlaceWordPath(string word, int index, int row, int col, bool[,] visited)
    {
        if (index == word.Length)
            return true;

        
        if (row < 0 || row >= Size || col < 0 || col >= Size)
            return false;

        if (visited[row, col])
            return false;

        char existing = _board[row, col].Letter;

        
        if (existing != '\0' && existing != word[index])
            return false;

        
        char backup = _board[row, col].Letter;
        _board[row, col].Letter = word[index];
        visited[row, col] = true;

        var directions = new (int dx, int dy)[]
        {
            (1,0),(-1,0),(0,1),(0,-1),
            (1,1),(-1,-1),(1,-1),(-1,1)
        };

        foreach (var dir in directions)
        {
            if (PlaceWordPath(word, index + 1, row + dir.dy, col + dir.dx, visited))
            {
                Debug.WriteLine($"Kelime yerleştirme başarılı: {word}");
                return true;
            }
        }

        _board[row, col].Letter = backup;
        visited[row, col] = false;

        return false;
    }
    
    private void TryPlaceWordPath(string word)
    {
        var random = new Random();

        for (int attempt = 0; attempt < 100; attempt++)
        {
            int row = random.Next(Size);
            int col = random.Next(Size);

            var visited = new bool[Size, Size];

            if (PlaceWordPath(word, 0, row, col, visited)) return;
        }
    }
    
    private void GenerateSmartBoard()
    {
        var words = _wordSet
            .Where(w =>w.Length <= Size * Size)
            .Shuffle()
            .Take(10)
            .ToList();

        foreach (var word in words)
        {
            TryPlaceWordPath(word);
        }

        FillEmptyCells();
    }
    
    private void FillEmptyCells()
    {
        var random = new Random();
        string letters = "AAAAAEEEEEİİİİİ" +
                         "NNNRRRLLLIIIDDDKKK" +
                         "MYTUSBOÜŞZGHÇĞCVPÖFJ";
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (_board[i, j].Letter == '\0')
                {
                    _board[i, j].Letter = letters[random.Next(letters.Length)];
                    if (_board[i, j].Power != PowerType.None)
                        _board[i, j].Power = PowerType.None;
                }
            }
        }
    }
    
    private async Task HandleWordFound(string word)
    {
        
        var subWords = GetSubWords(word).OrderByDescending(w => w.Length).ToList();

        foreach (var subWord in subWords)
        {
            var eklenecekSkor = CalculateWordScore(subWord);
            _score += eklenecekSkor;
            await PuanArttirmaEfekti(eklenecekSkor);
            PuanLbl.Text = $"Skor: {_score}";
        }
        
        EklenenPuanLbl.Text = string.Empty;

        HucreSilmeyiYonet(word);
        
        await AnimateExplosions();
        
        _selectedCells.Clear();
        GameCanvas.InvalidateSurface();
        
        PrepareGravityAndFill();
        await AnimateAll();

        _kalanKelimeSayisi = KalanKelimeler().Count;
        
        if (_kalanKelimeSayisi == 0)
        {
            GenerateSmartBoard();
        }
        
        GameCanvas.InvalidateSurface();
    }

    private void HucreSilmeyiYonet(string word)
    {
        var newPower = word.Length switch
        {
            4 => PowerType.ClearRow,
            5 => PowerType.ClearNeighbors,
            6 => PowerType.ClearColumn,
            >= 7 => PowerType.ClearBigArea,
            _ => PowerType.None
        };

        var lastCell = _selectedCells.Last();

        foreach (var cell in _selectedCells)
        {
            if (cell == lastCell && newPower != PowerType.None)
            {
                cell.Power = newPower;
                Debug.WriteLine($"Powerup oluşturuldu: {newPower} at Row:{cell.Row}, Col:{cell.Col}");
            }
            else
            {
                AddToExplosions(cell.Row,cell.Col);

                cell.Letter = '\0';
                cell.Power = PowerType.None;
            }
        }
    }

    private void AddToExplosions(int row, int col)
    {
        lock (_explosions)
        {
            float x = (col * _cellWidth) + _cellWidth / 2f;
            float y = (row * _cellHeight) + _cellHeight / 2f;

            _explosions.Add(new Explosion
            {
                X = x,
                Y = y,
                Radius = 5,
                MaxRadius = _cellWidth * 0.6f,
                Alpha = 255,
                Row = row,
                Col = col
            });
        }
    }
    
    private Dictionary<string, List<(int row, int col)>> _wordPaths = new();

    private HashSet<string> KalanKelimeler()
    {
        var result = new List<string>();
        var usedCells = new HashSet<(int, int)>();

        _wordPaths = FindAllWordPaths();

        var words = _wordPaths.Keys
            .OrderByDescending(w => w.Length)
            .ToList();

        foreach (var word in words)
        {
            var path = _wordPaths[word];
            
            if (path.Any(p => usedCells.Contains(p)))
                continue;

            result.Add(word);

            foreach (var p in path)
                usedCells.Add(p);
        }
        
        foreach (var kelime in result)
        {
            Debug.WriteLine(kelime);
        }

        return result.ToHashSet();
    }
    
    private Dictionary<string, List<(int row, int col)>> FindAllWordPaths()
    {
        _wordPaths.Clear();

        bool[,] visited = new bool[Size, Size];

        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                Dfs(row, col, "", visited, new List<(int, int)>());
            }
        }

        return _wordPaths;
    }
    
    private void Dfs(
        int row,
        int col,
        string current,
        bool[,] visited,
        List<(int row, int col)> path
    )
    {
        if (row < 0 || row >= Size || col < 0 || col >= Size)
            return;

        if (visited[row, col])
            return;

        current += _board[row, col].Letter;

        if (!_wordService.IsPrefix(current))
            return;

        visited[row, col] = true;
        path.Add((row, col));

        if (current.Length >= 3 && _wordSet.Contains(current))
        {
            _wordPaths[current] = new List<(int, int)>(path);
        }

        var directions = new (int dx, int dy)[]
        {
            (1,0),(-1,0),(0,1),(0,-1),
            (1,1),(-1,-1),(1,-1),(-1,1)
        };

        foreach (var dir in directions)
        {
            Dfs(row + dir.dy, col + dir.dx, current, visited, path);
        }

        visited[row, col] = false;
        path.RemoveAt(path.Count - 1);
    }
    
    private List<string> GetSubWords(string word)
    {
        var words = new List<string>();

        if (word.Length == 3)
            return [word];

        for (int i = 2; i < word.Length; i++)
        {
            var subWord = word.Substring(0, i + 1);
            if (IsWordValid(subWord))
            {
                words.Add(subWord);
            }
        } 
        
        Debug.WriteLine($"Alt kelimeler alımı başarılı");

        return words;
    }

    private int CalculateWordScore(string word)
    {
        var score = 0;
        foreach (var c in word)
        {
            score += _charScores[c];
        }
        
        Debug.WriteLine($"Puan hesaplama başarılı: {word} {score}");
        return score;
    }
    
    private List<(Cell cell, float startOffsetY)> _animatingCells = new();

    private void PrepareGravityAndFill()
    {
        _animatingCells.Clear();

        var random = new Random();
        string letters = "AAAAAEEEEEİİİİİ" +
                         "NNNRRRLLLIIIDDDKKK" +
                         "MYTUSBOÜŞZGHÇĞCVPÖFJ";

        for (int col = 0; col < Size; col++)
        {
            int emptyCount = 0;

            for (int row = Size - 1; row >= 0; row--)
            {
                var cell = _board[row, col];

                if (cell.Letter == '\0')
                {
                    emptyCount++;
                }
                else if (emptyCount > 0)
                {
                    int targetRow = row + emptyCount;
                    var targetCell = _board[targetRow, col];
                    
                    targetCell.Letter = cell.Letter;
                    targetCell.Power = cell.Power;
                    targetCell.Scale = 1f;
                    targetCell.Alpha = 255f;

                    cell.Letter = '\0';
                    cell.Power = PowerType.None;
                    
                    float startOffset = -emptyCount * _cellHeight;
                    targetCell.OffsetY = startOffset;
                    _animatingCells.Add((targetCell, startOffset));
                }
            }
            
            int newLetterIndex = 0;
            for (int row = 0; row < Size; row++)
            {
                var cell = _board[row, col];
                if (cell.Letter == '\0')
                {
                    cell.Letter = letters[random.Next(letters.Length)];
                    cell.Power = PowerType.None;
                    cell.Scale = 1f;
                    cell.Alpha = 255f;
                    
                    float startOffset = -(newLetterIndex + emptyCount) * _cellHeight;
                    cell.OffsetY = startOffset;
                    _animatingCells.Add((cell, startOffset));
                    newLetterIndex++;
                }
            }
        }
    }

    private bool IsWordValid(string word)
    {
        string searchWord = _wordService.ToTurkishUpper(word);
        return _wordSet.Contains(searchWord);
    }
    
    
    
    private bool IsNeighbor(Cell a, Cell b)
    {
        return Math.Abs(a.Row - b.Row) <= 1 && Math.Abs(a.Col - b.Col) <= 1;
    }
    
    private bool IsTouching(Cell a, Cell b)
    {
        return (Math.Abs(a.Row - b.Row) <= 1 && a.Col == b.Col) ||
               (Math.Abs(a.Col - b.Col) <= 1 && a.Row == b.Row);
    }
    
    private string GetWord()
    {
        return string.Concat(_selectedCells.Select(c => c.Letter));
    }
    
    private List<Explosion> _explosions = new();
}