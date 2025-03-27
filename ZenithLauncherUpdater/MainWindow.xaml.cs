using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;

namespace ZenithLauncherUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OnStartup();
        }

        private void OnStartup()
        {
            CheckPurpose();
        }

        private void CheckPurpose()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string destinationPath = Path.Combine(basePath, "Zenith", "app", "Zenith.exe");

            if (File.Exists(destinationPath))
            {
                MainFrame.Navigate(new Pages.UpdatingMain());
            }
            else
            {
                MainFrame.Navigate(new Pages.InstallLauncher.BeginDownload());
                ResizeToInstall();
            }
        }

        private void MoveWindow(object sender, RoutedEventArgs e)
        {
            this.DragMove();
        }

        public void ResizeToInstall()
        {
            this.Width = 493;
            this.Height = 385;
        }
    }
}