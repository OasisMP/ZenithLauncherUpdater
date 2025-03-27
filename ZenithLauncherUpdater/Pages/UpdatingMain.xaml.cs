using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
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

namespace ZenithLauncherUpdater.Pages
{
    /// <summary>
    /// Interaction logic for UpdatingMain.xaml
    /// </summary>
    public partial class UpdatingMain : Page
    {
        private readonly HttpClient _httpClient;
        private string _destinationPath;
        private ZenithLauncherUpdater.Class.FindManifest _updater;

        public UpdatingMain()
        {
            InitializeComponent();
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _destinationPath = Path.Combine(basePath, "Zenith", "app");

            _updater = new ZenithLauncherUpdater.Class.FindManifest(
                "https://zenith-api.zippywippy.online/api/v1/launcher/update/",
                _destinationPath,
                UpdateText
            );

            Loaded += UpdateWindow_Loaded;
        }

        private async void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RunUpdateAsync();
        }

        private async Task RunUpdateAsync()
        {
            try
            {
                bool updatesAvailable = await _updater.CheckForUpdatesAsync();

                if (!updatesAvailable)
                {
                    LaunchApplication();
                    return;
                }

                bool updateSuccessful = await _updater.PerformUpdateAsync();

                if (updateSuccessful)
                {
                    UpdateText.Text = "All done!";
                    await Task.Delay(1500);
                    LaunchApplication();
                }
                else
                {
                    UpdateText.Text = "Update failed. Please try again later. | UD-0001";
                }
            }
            catch (Exception)
            {
                UpdateText.Text = $"An error stopped us from working on your update. | UD-0002";
            }
        }

        private void LaunchApplication()
        {
            try
            {
                string appPath = Path.Combine(_destinationPath, "Zenith.exe");
                System.Diagnostics.Process.Start(appPath);
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                UpdateText.Text = $"For some unknown reason, we failed to start the launcher. Try again later. | UD-0003";
            }
        }
    }
}
