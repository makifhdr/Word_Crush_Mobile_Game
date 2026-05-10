namespace MauiApp5.Models;

public class GameResult
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int GridSize { get; set; }

    public string GridSizeString
    {
        get => $"{GridSize}x{GridSize}";
        set;
    } = string.Empty;

    public int Score { get; set; }
    public int WordCount { get; set; }

    public string LongestWord { get; set; } = string.Empty;

    public string LongestWordString
    {
        get
        {
            if (LongestWord.Equals(string.Empty))
            {
                return "\"Kelime bulunamadı\"";
            }
            return LongestWord;
        }
        set;
    } = string.Empty;

    public int DurationSeconds { get; set; }

    public string SureString
    {
        get => SureFormatla(DurationSeconds);
        set;
    } = string.Empty;

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
}