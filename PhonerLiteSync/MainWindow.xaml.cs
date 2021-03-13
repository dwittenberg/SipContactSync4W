using System;
using System.Windows;
using System.Windows.Media;

using Microsoft.Win32;

using PhonerLiteSync.Model;
using Colors = PhonerLiteSync.Model.Colors;

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
            const bool automaticStart = false; // args != null;
#endif

            Settings = IoHandler.LoadSettings(CsvHandler.SettingsPath);
            
            var timeSinceLastRestart = DateTime.Now - Settings.LastRestart;
            if (timeSinceLastRestart.TotalMinutes < Settings.WaitingTimeInMinutes && automaticStart)
            {
                this.Close();
                return;
            }

            DataContext = Settings;

            if (automaticStart && StartSync())
            {
                this.Close();
            }
        }

        private void StartSync(object sender, RoutedEventArgs e)
            => StartSync();

        private bool StartSync()
        {
            if (!Settings.AllOk) return false;

            BtnRun.Content = "Sync - Run";

            IoHandler.SaveSettings(Settings, CsvHandler.SettingsPath);

            if (CsvHandler.Run(Settings.LocalPath, Settings.ExternPath))
            {
                BtnRun.Content = "Sync - Finished";
                BtnRun.Background = Brushes.DarkSeaGreen;
                BtnRun.BorderThickness = new Thickness(2);
                BtnRun.BorderBrush = Brushes.Green;
                return true;
            }

            BtnRun.Background = Colors.StatusBrushesBg[(int)StatusColor.Problematic];
            BtnRun.Content = "Sync - Problem";
            BtnRun.BorderThickness = new Thickness(2);
            BtnRun.BorderBrush = Brushes.Orange;
            return false;
        }

        private void BtnSource_Click(object sender, RoutedEventArgs e)
        => Settings.LocalPath = ShowMyDialog(Settings.LocalPath, "csv");

        private void BtnDestination_Click(object sender, RoutedEventArgs e)
        => Settings.ExternPath = ShowMyDialog(Settings.ExternPath, "json");

        private static string ShowMyDialog(string path, string type)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = type switch
            {
                "csv" => "CSV File (*.csv)|*.csv|All files (*.*)|*.*",
                "json" => "JSON File (*.json)|*.json|All files (*.*)|*.*",
                _ => openFileDialog.Filter
            };

            openFileDialog.InitialDirectory = path;

            return openFileDialog.ShowDialog() == true
                ? openFileDialog.FileName
                : path;
        }
    }
}
