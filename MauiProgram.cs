using CommunityToolkit.Maui;
using MauiApp5.Models;
using MauiApp5.Services;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Cell = MauiApp5.Models.Cell;

namespace MauiApp5;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("VarelaRound-Regular.ttf", "DefaultFont");
            });
        builder.Services.Add(new ServiceDescriptor(typeof(DataService), typeof(DataService), ServiceLifetime.Scoped));
        builder.Services.Add(new ServiceDescriptor(typeof(WordService), typeof(WordService), ServiceLifetime.Scoped));
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}