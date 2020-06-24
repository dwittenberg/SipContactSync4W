using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PathGui settings = new PathGui();

        public MainWindow()
        {
            InitializeComponent();

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var userData = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var helper = new Helper();
            var load = helper.LoadSettings();

            settings.LocalPath = load != null ? load.LocalPath : Environment.ExpandEnvironmentVariables(appData + @"\PhonerLite\phonebook.csv");
            settings.ExternPath = load != null ? load.ExternPath : Environment.ExpandEnvironmentVariables(userData + @"\OneDrive\PhonerLite\phonebook.csv");

            DataContext = settings;
            UpdateGui();
        }

        private void StartSync(object sender, RoutedEventArgs e)
        {
            UpdateGui();

            if (settings.AllOk)
            {
                var helper = new Helper();
                helper.SaveSettings(settings);

                var handler = new CsvHandler();
                handler.run(settings.LocalPath, settings.ExternPath);
            }
        }

        private void UpdateGui()
        {
            tbDestination.BorderBrush = settings.ExternPathOk ? Brushes.Gray : Brushes.Crimson;
            tbSoure.BorderBrush = settings.LocalPathOk ? Brushes.Gray : Brushes.Crimson;
        }

        private void btnSource_Click(object sender, RoutedEventArgs e)
        {
            settings.LocalPath = ShowMyDialog(settings.LocalPath);
            UpdateGui();
        }

        private string ShowMyDialog(string path)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = path;
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return string.Empty;
        }

        private void btnDestination_Click(object sender, RoutedEventArgs e)
        {
            settings.ExternPath = ShowMyDialog(settings.LocalPath);
            UpdateGui();
        }
    }
}
