using FFMPEGConverter.Helpers;
using FFMPEGConverter.Views;
using Microsoft.Extensions.Logging;
using Uno.Resizetizer;

namespace FFMPEGConverter;
public partial class App : Application
{
    public App() => InitializeComponent();

    public static Window? MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window
        {
            Title = "FFMPEG Converter"
        };
        
#if DEBUG
        MainWindow.UseStudio();
#endif


        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(MainPageView), args.Arguments);
        }

        MainWindow.SetWindowIcon();
        MainWindow.Activate();
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");

    public static void InitializeLogging()
    {
#if DEBUG
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
