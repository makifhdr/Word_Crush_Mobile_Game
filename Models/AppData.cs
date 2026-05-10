namespace MauiApp5.Models;

public class AppData
{
    public string UserName { get; set; }  = string.Empty;
    public int Gold { get; set; }
    public List<GameResult> GameHistory { get; set; } = [];

    public int NextGameId { get; set; } = 0;

    public Dictionary<JokerType, int> JokerNumbers { get; set; }
}