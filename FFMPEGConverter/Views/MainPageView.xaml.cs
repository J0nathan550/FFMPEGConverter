using FFMPEGConverter.Helpers;
using FFMPEGConverter.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;

namespace FFMPEGConverter.Views;

public partial class MainPageView : Page
{
    private readonly MainPageViewModel? ViewModel = new();

    public MainPageView()
    {
        DataContext = ViewModel;
        InitializeComponent();
        Loaded += MainPageView_Loaded;
    }

    private void MainPageView_Loaded(object sender, RoutedEventArgs e)
    {
        WindowId appWindowId = XamlRoot.ContentIslandEnvironment.AppWindowId;
        AppWindowTitleBar titleBar = AppWindow.GetFromWindowId(appWindowId).TitleBar;
        titleBar.BackgroundColor = Colors.Black;
        titleBar.ForegroundColor = Colors.White;
        titleBar.InactiveBackgroundColor = Colors.Black;
        titleBar.InactiveForegroundColor = Colors.White;
        titleBar.ButtonHoverBackgroundColor = Colors.White;
        titleBar.ButtonHoverForegroundColor = Colors.Black;
        titleBar.ButtonInactiveForegroundColor = Colors.Gray;
        titleBar.ButtonInactiveBackgroundColor = Colors.Black;
        titleBar.ButtonBackgroundColor = Colors.Black;
        titleBar.ButtonForegroundColor = Colors.White;
        MainPageViewModel.XamlRootPointer = XamlRoot;
        SaveSystem.Load();
        Loaded -= MainPageView_Loaded;
    }
}
