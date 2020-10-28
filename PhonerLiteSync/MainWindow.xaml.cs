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
        public Settings settings = new Settings();

        public MainWindow()
        {
            InitializeComponent(); 
            string[] args = App.Args;
            var automaticStart = args != null;

            var helper = new Helper();
            settings = IOHandler.LoadSettings(CsvHandler.SavePath);

            var timeSinceLastRestart = DateTime.Now - settings.LastRestart;
            if (timeSinceLastRestart.TotalMinutes < settings.WaitingTime && automaticStart)
            {
                this.Close();
                return;
            }

            DataContext = settings;
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
            if (settings.AllOk)
            {
                var helper = new Helper();
                IOHandler.SaveSettings(settings, CsvHandler.SavePath);

                var handler = new CsvHandler();
                handler.Run(settings.LocalPath, settings.ExternPath);
                btnRun.Content = "Sync - Finished";
                btnRun.Background = Brushes.DarkSeaGreen;
                return true;
            }

            return false;
        }

        private void UpdateGui()
        {
            tbDestination.BorderBrush = settings.ExternPathOk ? Brushes.Gray : Brushes.Crimson;
            tbDestination.BorderThickness = new Thickness(settings.ExternPathOk ? 1 : 2);
            tbDestination.Text = settings.ExternPath;
            tbSoure.BorderBrush = settings.LocalPathOk ? Brushes.Gray : Brushes.Crimson;
            tbSoure.BorderThickness = new Thickness(settings.LocalPathOk ? 1 : 2);
            tbSoure.Text = settings.LocalPath;
        }

        private void btnSource_Click(object sender, RoutedEventArgs e)
        {
            settings.LocalPath = ShowMyDialog(settings.LocalPath, "csv");
            UpdateGui();
        }
        
        private void btnDestination_Click(object sender, RoutedEventArgs e)
        {
            settings.ExternPath = ShowMyDialog(settings.ExternPath, "json");
            UpdateGui();
        }

        private string ShowMyDialog(string path, string type)
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
