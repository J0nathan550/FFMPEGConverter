using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FFmpeg.NET;
using FFMPEGConverter.Helpers;
using WGetNET;
using Windows.Storage.Pickers;

namespace FFMPEGConverter.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isAudioAvailable = false;
    [ObservableProperty]
    private bool isVideoAvailable = false;
    [ObservableProperty]
    private bool isImageAvailable = false;
    [ObservableProperty]
    private int dataKbs;
    [ObservableProperty]
    private int dataMaximumKbs;
    [ObservableProperty]
    private bool isAbleToChangeCompress = false;
    [ObservableProperty]
    private bool isContainingLogs = false;
    [ObservableProperty]
    private string? dataKbsString;
    [ObservableProperty]
    private string? inputFilePath;
    [ObservableProperty]
    private string? outputFilePath;
    [ObservableProperty]
    private string? consoleLogOutput;
    [ObservableProperty]
    private string? selectedVideoFormat;
    [ObservableProperty]
    private string? selectedAudioFormat;
    [ObservableProperty]
    private string? selectedImageFormat;
    [ObservableProperty]
    private bool isAbleToProcessFile;

    [ObservableProperty]
    private ObservableCollection<string> videoFormats =
    [
        ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm",
        ".gif", ".mpeg", ".mpg", ".m4v", ".ogv", ".vob", ".asf", ".mts",
        ".m2ts", ".ts", ".f4v", ".drc", ".wtv", ".yuv", ".ivf", ".dpx", ".m2v", ".m1v"
    ];

    [ObservableProperty]
    private ObservableCollection<string> audioFormats =
    [
        ".mp3", ".aac", ".wav", ".flac", ".ogg", ".wma", ".m4a", ".ac3",
        ".opus", ".ra", ".mp2", ".au", ".voc",
        ".spx", ".tta", ".wv", ".adx",
        ".caf", ".aiff", ".aif", ".aifc", ".mka"
    ];

    [ObservableProperty]
    private ObservableCollection<string> imageFormats =
    [
        ".png", ".jpeg", ".jpg", ".bmp", ".tiff", ".tif", ".webp", ".tga",
    ];


    public static XamlRoot? XamlRootPointer;

    public MainPageViewModel()
    {
        SaveSystem.Load();
        InputFilePath = SaveSystem.UserData.InputPath;
        OutputFilePath = SaveSystem.UserData.OutputPath;
        ConsoleLogOutput = SaveSystem.UserData.OutputLog;
        DataKbs = SaveSystem.UserData.KBs;
        DataKbsString = DataKbs.ToString();
    }

    partial void OnDataKbsChanging(int value) => DataKbsString = value.ToString();

    partial void OnDataKbsStringChanged(string? value)
    {
        if (int.TryParse(value, out int result))
        {
            if (DataKbs > DataMaximumKbs)
            {
                DataKbs = DataMaximumKbs;
            }
            DataKbs = result;
            SaveSystem.UserData.KBs = DataKbs;
        }
    }

    partial void OnConsoleLogOutputChanged(string? value) => IsContainingLogs = ConsoleLogOutput?.Length > 0;
    partial void OnSelectedVideoFormatChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            SelectedAudioFormat = null;
            DataMaximumKbs = 100000;
            IsAbleToChangeCompress = true;
        }
    }

    partial void OnSelectedAudioFormatChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            SelectedVideoFormat = null;
            DataMaximumKbs = 512000;
            IsAbleToChangeCompress = true;
        }
    }

    partial void OnInputFilePathChanged(string? value)
    {
        SaveSystem.UserData.InputPath = value;
        if (File.Exists(value))
        {
            string? extension = Path.GetExtension(value)?.ToLower();

            if (!string.IsNullOrEmpty(extension))
            {
                if (VideoFormats.Contains(extension))
                {
                    IsVideoAvailable = true;
                    IsAudioAvailable = true;
                    IsImageAvailable = false;
                    SelectedVideoFormat = extension;
                    SelectedAudioFormat = null;
                    SelectedImageFormat = null;
                    DataMaximumKbs = 100000;
                    IsAbleToChangeCompress = true;
                }
                else if (AudioFormats.Contains(extension))
                {
                    IsVideoAvailable = false;
                    IsAudioAvailable = true;
                    IsImageAvailable = false;
                    SelectedVideoFormat = null;
                    SelectedAudioFormat = extension;
                    SelectedImageFormat = null;
                    DataMaximumKbs = 512000;
                    IsAbleToChangeCompress = true;
                }
                else if (ImageFormats.Contains(extension))
                {
                    IsVideoAvailable = false;
                    IsAudioAvailable = false;
                    IsImageAvailable = true;
                    SelectedVideoFormat = null;
                    SelectedAudioFormat = null;
                    SelectedImageFormat = extension;
                    DataMaximumKbs = 10000;
                    IsAbleToChangeCompress = true;
                }
                else
                {
                    IsVideoAvailable = false;
                    IsAudioAvailable = false;
                    IsImageAvailable = false;
                    DataMaximumKbs = 0;
                    IsAbleToChangeCompress = false;
                }
            }
            else
            {
                IsVideoAvailable = false;
                IsAudioAvailable = false;
                IsImageAvailable = false;
                DataMaximumKbs = 0;
                IsAbleToChangeCompress = false;
            }
        }
        else
        {
            IsVideoAvailable = false;
            IsAudioAvailable = false;
            IsImageAvailable = false;
            DataMaximumKbs = 0;
            IsAbleToChangeCompress = false;
        }
    }

    partial void OnOutputFilePathChanged(string? value)
    {
        SaveSystem.UserData.OutputPath = value;
        if (Path.Exists(value))
        {
            IsAbleToProcessFile = true;
            return;
        }
        IsAbleToProcessFile = false;
    }

    [RelayCommand]
    private async Task InputFile()
    {
        FileOpenPicker fileOpenPicker = new()
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder
        };
        foreach (string extension in VideoFormats)
        {
            fileOpenPicker.FileTypeFilter.Add(extension);
        }
        foreach (string extension in AudioFormats)
        {
            fileOpenPicker.FileTypeFilter.Add(extension);
        }
        foreach (string extension in ImageFormats)
        {
            fileOpenPicker.FileTypeFilter.Add(extension);
        }
        fileOpenPicker.FileTypeFilter.Add("*");

        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

        StorageFile pickedFile = await fileOpenPicker.PickSingleFileAsync();

        if (pickedFile != null)
        {
            InputFilePath = pickedFile.Path;
        }
    }

    [RelayCommand]
    private async Task OutputFile()
    {
        FolderPicker folderPicker = new()
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder
        };
        folderPicker.FileTypeFilter.Add("*");

        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        StorageFolder pickedFolder = await folderPicker.PickSingleFolderAsync();
        if (pickedFolder != null)
        {
            OutputFilePath = pickedFolder.Path;
        }
    }

    [RelayCommand]
    private void ClearLog()
    {
        ConsoleLogOutput = string.Empty;
        SaveSystem.UserData.OutputLog = string.Empty;
    }

    [RelayCommand]
    private async Task ProcessFile()
    {
        if (string.IsNullOrWhiteSpace(InputFilePath) || string.IsNullOrWhiteSpace(OutputFilePath))
        {
            ConsoleLogOutput += "Input or output file path is not set.\n";
            SaveSystem.UserData.OutputLog = ConsoleLogOutput;
            return;
        }

        if (!File.Exists(InputFilePath) || !Directory.Exists(OutputFilePath))
        {
            ConsoleLogOutput += "Input file does not exist or output directory is invalid.\n";
            SaveSystem.UserData.OutputLog = ConsoleLogOutput;
            return;
        }

        ConversionOptions options;
        string inputFileName = Path.GetFileNameWithoutExtension(InputFilePath);
        string outputFile;

        if (!string.IsNullOrEmpty(SelectedVideoFormat))
        {
            outputFile = Path.Combine(OutputFilePath, $"{inputFileName}{SelectedVideoFormat}");
            options = new()
            {
                VideoBitRate = DataKbs > 0 ? DataKbs : null,
                //HWAccel = HWAccel.cuda
            };
        }
        else if (!string.IsNullOrEmpty(SelectedAudioFormat))
        {
            outputFile = Path.Combine(OutputFilePath, $"{inputFileName}{SelectedAudioFormat}");
            options = new()
            {
                AudioBitRate = DataKbs > 0 ? DataKbs : null,
                //HWAccel = HWAccel.cuda
            };
        }
        else if (!string.IsNullOrEmpty(SelectedImageFormat))
        {
            outputFile = Path.Combine(OutputFilePath, $"{inputFileName}{SelectedImageFormat}");
            options = new()
            {
                VideoBitRate = DataKbs > 0 ? DataKbs : null,
                //HWAccel = HWAccel.cuda in some cases hwaccel is not good for conversion, i need to add settings
            };
        }
        else
        {
            ConsoleLogOutput += "No valid format selected for processing.\n";
            SaveSystem.UserData.OutputLog = ConsoleLogOutput;
            return;
        }

        try
        {
            ConsoleLogOutput += $"Processing file {inputFileName}...\n";
            SaveSystem.UserData.OutputLog = ConsoleLogOutput;

            Engine ffmpegEngine = new();

            InputFile inputFile = new(InputFilePath);
            OutputFile outputFileMedia = new(outputFile);

            // Execute the conversion
            await ffmpegEngine.ConvertAsync(inputFile, outputFileMedia, options, default);

            ConsoleLogOutput += $"File {inputFileName} processed successfully.\n";
            SaveSystem.UserData.OutputLog = ConsoleLogOutput;

            // Optional: Ask if the user wants to open the output folder
            bool openFolder = await PromptUserToOpenFolder(OutputFilePath);
            if (openFolder)
            {
                try
                {
                    Process.Start("explorer.exe", OutputFilePath);
                }
                catch
                {
                    ContentDialog dialog = new()
                    {
                        Title = "Unexpected Error!",
                        Content = "There was an issue opening your output folder.\nTry to find output file yourself.",
                        PrimaryButtonText = "OK",
                        XamlRoot = XamlRootPointer
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                }
            }
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("FFmpeg executable could not be found neither in PATH nor in directory."))
            {
                ConsoleLogOutput += "FFMPEG is not detected and needs to be installed in PATH.\n";
                SaveSystem.UserData.OutputLog = ConsoleLogOutput;
                // winget ffmpeg
                ContentDialog dialog = new()
                {
                    Title = "FFMPEG is not installed!",
                    Content = "Looks like you don't have FFMPEG installed on your Windows machine.\nIn order to work in this program you need to install FFMPEG.\nDo you want to install it?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    XamlRoot = XamlRootPointer
                };

                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    WinGet winGet = new();
                    if (winGet.IsInstalled)
                    {
                        ConsoleLogOutput += "Started installing FFMPEG, please wait...\n";
                        SaveSystem.UserData.OutputLog = ConsoleLogOutput;
                        await Task.Run(async () =>
                        {
                            WinGetPackageManager packageManager = new();
                            await packageManager.InstallPackageAsync("ffmpeg");
                        });
                        ConsoleLogOutput += "FFMPEG installing is finished!\n";
                        Engine ffmpeg = new(); // needs to be recreated here because of "not existing" bug.
                        await ProcessFileCommand.ExecuteAsync(null);
                    }
                    else
                    {
                        dialog = new()
                        {
                            Title = "WinGet is not installed!",
                            Content = "Looks like you don't have WinGet installed on your Windows machine.\nYou need to manually install it from GitHub or Microsoft Store in order to install it and work with FFMPEG.\nDo you want to open GitHub page where you have to manually install WinGet?",
                            PrimaryButtonText = "Yes",
                            CloseButtonText = "No",
                            XamlRoot = XamlRootPointer
                        };

                        result = await dialog.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            try
                            {
                                Process.Start("https://github.com/microsoft/winget-cli/releases");
                            }
                            catch
                            {
                                dialog = new()
                                {
                                    Title = "Unexpected Error!",
                                    Content = "There was an issue opening your browser.\nYou have to install manually the WinGet.\nLink is: https://github.com/microsoft/winget-cli/releases",
                                    PrimaryButtonText = "OK",
                                    XamlRoot = XamlRootPointer
                                };
                                result = await dialog.ShowAsync();
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleLogOutput += $"Error: {ex.Message}\n";
            SaveSystem.UserData.OutputLog = ConsoleLogOutput;
        }
    }

    private static async Task<bool> PromptUserToOpenFolder(string? folderPath)
    {
        if (folderPath == null)
        {
            return false;
        }
        ContentDialog dialog = new()
        {
            Title = "Open Output Folder",
            Content = "Would you like to open the folder containing the output file?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = XamlRootPointer
        };

        ContentDialogResult result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

}
