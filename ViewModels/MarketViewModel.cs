using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using MauiApp5.Models;
using MauiApp5.Services;
using MauiApp5.Views;

namespace MauiApp5.ViewModels;

public class MarketViewModel: INotifyPropertyChanged
{
    private readonly DataService _dataService = new();
    private AppData _data = new();

    public int Gold => _data.Gold;

    public List<Joker> Jokers { get; set; } = [];

    public ICommand ShowJokerDetailCommand { get; }
    public ICommand BuyJokerCommand { get; }

    public MarketViewModel()
    {
        ShowJokerDetailCommand = new Command<Joker>(async void (joker) =>
        {
            try
            {
                if (joker == null) return;

                await Shell.Current.Navigation.PushModalAsync(
                    new JokerDetailPage(joker)
                );
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        });
        
        BuyJokerCommand = new Command<Joker>(async void (joker) =>
        {
            try
            {
                var currentPage = Shell.Current?.CurrentPage;
        
                if (currentPage == null) 
                    return;
            
                if (joker == null) return;

                int price = GetPrice(joker.JokerTipi);

                if (_data.Gold < price)
                {
                    var uyariPopup = new UyariPopup("Yetersiz Altın", "Altınınız yetmiyor!");
                    await currentPage.ShowPopupAsync(uyariPopup);
                    return;
                }

            
                var popup = new OnayPopup(
                    "Satın Alma Onayı", 
                    $"{joker.Name} jokerini {price} altın karşılığında satın almak istediğinize emin misiniz?"
                );

            
                var result = await currentPage.ShowPopupAsync<bool>(popup, new PopupOptions()
                {
                    CanBeDismissedByTappingOutsideOfPopup = false
                });
            
                if (result.Result)
                {
                    _data.Gold -= price;
                    _data.JokerNumbers[joker.JokerTipi]++;
                    await _dataService.SaveAsync(_data);
                }
            
                CreateJokers();
                OnPropertyChanged(nameof(Gold));
                OnPropertyChanged(nameof(Jokers));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        });
    }
    
    private int GetPrice(JokerType type)
    {
        return Jokers.Find(x => x.JokerTipi == type)!.Cost;
    }

    public async Task LoadAsync()
    {
        _data = await _dataService.LoadAsync();

        CreateJokers();

        OnPropertyChanged(nameof(Gold));
        OnPropertyChanged(nameof(Jokers));
    }

    private void CreateJokers()
    {
        Jokers =
        [
            new Joker
            {
                JokerTipi = JokerType.Balik,
                Name = "Balık",
                Cost = 150,
                Description =
                    "Tahtada rastgele olarak harfleri yok etmektedir. Rastgele yok olan harflerin üzerindeki harfler aşağıya düşmektedir.",
                NumberOf = GetJokerCount(JokerType.Balik),
                GifPath = "balikgif.gif",
                ImagePath = "balik.png",
            },

            new Joker
            {
                JokerTipi = JokerType.Tekerlek,
                Name = "Tekerlek",
                Cost = 300,
                Description = "Tahtada seçilen harfin bulunduğu satır ve sütundaki tüm harfler yok olmaktadır.",
                NumberOf = GetJokerCount(JokerType.Tekerlek),
                GifPath = "tekerlekgif.gif",
                ImagePath = "tekerlek.png",
            },

            new Joker
            {
                JokerTipi = JokerType.LolipopKirici,
                Name = "Lolipop Kırıcı",
                Cost = 75,
                Description =
                    "Tahtada seçilen bir harfi yok etmek için kullanılmaktadır. Bu harf yok olduğunda yukarısındaki kelimeler aşağı düşmektedir.",
                NumberOf = GetJokerCount(JokerType.LolipopKirici),
                GifPath = "lolipopkiricigif.gif",
                ImagePath = "lolipopkirici.png",
            },

            new Joker
            {
                JokerTipi = JokerType.SerbestDegistirme,
                Name = "Serbest Değiştirme",
                Cost = 50,
                Description = "Tahtada birbirine temas eden iki harfin yer değiştirilmesini sağlamaktadır.",
                NumberOf = GetJokerCount(JokerType.SerbestDegistirme),
                GifPath = "serbestdegistirmegif.gif",
                ImagePath = "serbestdegistirme.png",
            },

            new Joker
            {
                JokerTipi = JokerType.HarfKaristirma,
                Name = "Harf Karıştırma",
                Cost = 125,
                Description =
                    "Bu özellik seçildiğinde tahtada bulunan harflerin rastgele bir şekilde karıştırılmasını sağlamaktadır.",
                NumberOf = GetJokerCount(JokerType.HarfKaristirma),
                GifPath = "harfkaristirmagif.gif",
                ImagePath = "harfkaristirma.png",
            },

            new Joker
            {
                JokerTipi = JokerType.PartiGuclendirici,
                Name = "Parti Güçlendiricisi",
                Cost = 500,
                Description =
                    "Bu özellik seçildiğinde tahtada bulunan tüm harfler yok edilir ve tekrardan rastgele bir şekilde harfler yukarıdan aşağıya düşmektedir.",
                NumberOf = GetJokerCount(JokerType.PartiGuclendirici),
                GifPath = "partiguclendiricigif.gif",
                ImagePath = "partiguclendirici.png",
            }
        ];
    }

    private int GetJokerCount(JokerType type)
    {
        return _data.JokerNumbers.GetValueOrDefault(type);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}