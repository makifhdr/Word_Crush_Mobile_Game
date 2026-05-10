using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp5.Helpers;
using MauiApp5.ViewModels;

namespace MauiApp5.Views;

public partial class GameSettingsPage
{
    public GameSettingsPage()
    {
        InitializeComponent();
        BindingContext = new GameSettingsViewModel();
    }
}