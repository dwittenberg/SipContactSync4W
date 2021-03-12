using System;
using System.Windows;
using System.Windows.Media;

using Microsoft.Win32;

using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Settings Settings;

        public MainWindow()
        {
            InitializeComponent(); 

            var args = App.Args;
#if RELEASE
            var automaticStart = args != null;
#else
            var automaticStart = false;// args != null;
#endif
            
            Settings = IoHandler.LoadSettings(CsvHandler.SettingsPath);

            var timeSinceLastRestart = DateTime.Now - Settings.LastRestart;
            if (timeSinceLastRestart.TotalMinutes < Settings.WaitingTime && automaticStart)
            {
                this.Close();
                return;
            }

            DataContext = Settings;
            UpdateGui();

            if (automaticStart && StartSync())
            {
                this.Close();
            }
        }

        private void StartSync(object sender, RoutedEventArgs e)
        {
            UpdateGui();
            StartSync();
        }

        private bool StartSync()
        {
            if (Settings.AllOk)
            {
                IoHandler.SaveSettings(Settings, CsvHandler.SettingsPath);

                var handler = new CsvHandler();
                handler.Run(Settings.LocalPath, Settings.ExternPath);
                btnRun.Content = "Sync - Finished";
                btnRun.Background = Brushes.DarkSeaGreen;
                return true;
            }

            return false;
        }

        private void UpdateGui()
        {
            tbDestination.BorderBrush = Settings.ExternPathOk ? Brushes.Gray : Brushes.Crimson;
            tbDestination.BorderThickness = new Thickness(Settings.ExternPathOk ? 1 : 2);
            tbDestination.Text = Settings.ExternPath;
            tbSoure.BorderBrush = Settings.LocalPathOk ? Brushes.Gray : Brushes.Crimson;
            tbSoure.BorderThickness = new Thickness(Settings.LocalPathOk ? 1 : 2);
            tbSoure.Text = Settings.LocalPath;
        }

        private void btnSource_Click(object sender, RoutedEventArgs e)
        {
            Settings.LocalPath = ShowMyDialog(Settings.LocalPath, "csv");
            UpdateGui();
        }
        
        private void btnDestination_Click(object sender, RoutedEventArgs e)
        {
            Settings.ExternPath = ShowMyDialog(Settings.ExternPath, "json");
            UpdateGui();
        }

        private static string ShowMyDialog(string path, string type)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            switch (type)
            {
                case "csv":
                    openFileDialog.Filter = "CSV File (*.csv)|*.csv|All files (*.*)|*.*";
                    break;
                case "json":
                    openFileDialog.Filter = "JSON File (*.json)|*.json|All files (*.*)|*.*";
                    break;
            }
       
            openFileDialog.InitialDirectory = path;
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return string.Empty;
        }
    }
}
