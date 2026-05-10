namespace MauiApp5.Services;

public class WordService
{
    private HashSet<string> _prefixSet;
    
    private HashSet<string> _words;
    
    public async Task<HashSet<string>> LoadWordsAsync()
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync("allWords.txt");
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        
        _words = text
            .Split('\n')
            .Select(w => ToTurkishUpper(w))
            .Where(w => !string.IsNullOrEmpty(w))
            .ToHashSet();
        
        _prefixSet = new HashSet<string>();

        foreach (var word in _words)
        {
            for (int i = 1; i <= word.Length; i++)
            {
                _prefixSet.Add(word.Substring(0, i));
            }
        }

        return _words;
    }
    
    public string ToTurkishUpper(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        return text.Trim()
            .Replace("i", "İ")
            .Replace("ı", "I")
            .ToUpper();
    }
    
    public bool IsPrefix(string prefix)
    {
        return _prefixSet.Contains(prefix);
    }
}