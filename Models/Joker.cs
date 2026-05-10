using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiApp5.Models;

public class Joker : INotifyPropertyChanged 
{
    public JokerType JokerTipi { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string Description { get; set; } = string.Empty;
    public string GifPath { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    
    private int _numberOf;
    public int NumberOf
    {
        get => _numberOf;
        set
        {
            if (_numberOf != value)
            {
                _numberOf = value;
                OnPropertyChanged();
            }
        }
    }
    
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum JokerType
{
    Balik,
    HarfKaristirma,
    LolipopKirici,
    PartiGuclendirici,
    SerbestDegistirme,
    Tekerlek,
    Null
    
}