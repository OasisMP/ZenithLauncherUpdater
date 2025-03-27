using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Runtime;
using System.Diagnostics;
using IWshRuntimeLibrary;
using ZenithLauncherUpdater.Services;

namespace ZenithLauncherUpdater.Pages.InstallLauncher
{
    /// <summary>
    /// Interaction logic for BeginDownload.xaml
    /// </summary>
    public partial class BeginDownload : Page
    {
        private readonly HttpClient _httpClient;
        private string _destinationPath;
        private string _appPath;
        private ZenithLauncherUpdater.Class.DownloadLaucnher _updater;
        string appName = "Zenith.lnk";
        string appPath = string.Empty;

        public BeginDownload()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ConfirmShutdown();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            Step1.Opacity = 0;
            Step1.IsHitTestVisible = false;
            Step1Btns.Opacity = 0;
            Step1Btns.IsHitTestVisible = false;
            BgImg.Opacity = 0;
            InstallGrid.Opacity = 1;
            RunUpdates();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ConfirmShutdown();
        }

        private void ConfirmShutdown()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit the Zenith installer?", "Close?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private async void RunUpdates()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _destinationPath = Path.Combine(basePath, "Zenith");

            Directory.CreateDirectory(_destinationPath);

            _updater = new ZenithLauncherUpdater.Class.DownloadLaucnher(
                "https://zenith-api.zippywippy.online/api/v1/launcher/download/",
                _destinationPath,
                DownloadText
            );

            await RunUpdateAsync();
        }

        private async Task RunUpdateAsync()
        {
            try
            {
                bool updatesAvailable = await _updater.CheckForUpdatesAsync();
                bool updateSuccessful = await _updater.PerformUpdateAsync();

                if (updateSuccessful)
                {
                    DownloadText.Text = "All done!";
                    await Task.Delay(1500);
                    CreateShortcut();
                    FinishInstall();
                }
            }
            catch (Exception ex)
            {
                DownloadText.Text = $"Failed to install Zenith, please try again later.";
                DownloadText.Text = $"Failed to install Zenith, please try again later. {ex} | DL-0001";
            }
        }

        private void FinishInstall()
        {
            InstallGrid.Opacity = 0;
            Step1Btns.Opacity = 0;
            FinalBtns.Opacity = 1;
            FinalBtns.IsHitTestVisible = true;
            FinishedGrid.Opacity = 1;
            FinishedGrid.IsHitTestVisible = true;
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CreateShortcut()
        {
            try
            {
                string? shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Zenith.url");
                _appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                string powershellScript = $@" $WshShell = New-Object -ComObject WScript.Shell $Shortcut = $WshShell.CreateShortcut('{shortcutPath}') $Shortcut.TargetPath = '{_appPath}' $Shortcut.WorkingDirectory = '{Path.GetDirectoryName(_appPath)}' $Shortcut.Save()";
                string appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), appName);

                try
                {
                    string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Zenith/app/Zenith.exe");
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    Type t = Type.GetTypeFromProgID("WScript.Shell");
                    dynamic wshShell = Activator.CreateInstance(t);
                    var shortcut = wshShell.CreateShortcut(Path.Combine(desktopPath, appName + ""));

                    shortcut.TargetPath = exePath;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                    shortcut.Description = appName;
                    shortcut.Save();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to create Zenith shortcut. {ex} | DL-0002");
                }
            }
            catch(Exception ex) {
                Logger.Log("Not actually quite sure what error could even happen here, contact @isaac on Discord. | DL-0003");
            }
        }
    }
}
