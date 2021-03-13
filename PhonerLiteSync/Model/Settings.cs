using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace PhonerLiteSync.Model
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        public Settings()
        {
            PhoneBookPath = Environment.ExpandEnvironmentVariables(@"%appData%\PhonerLite\phonebook.csv");
            ExternPath = string.Empty;

            WaitingTimeInMinutes = 30;
            LastRestart = DateTime.MinValue;
        }

        public Settings(string path)
        {
            var jsonString = File.ReadAllText(path);
            var s = JsonSerializer.Deserialize<Settings>(jsonString);
            if (s == null)
            {
                return;
            }

            PhoneBookPath = s.PhoneBookPath;
            ExternPath = s.ExternPath;
            WaitingTimeInMinutes = s.WaitingTimeInMinutes;
            LastRestart = s.LastRestart;
        }

        [JsonIgnore]
        public string SettingsPath { get; private set; }
        [JsonIgnore]
        public string PhonerConfigPath { get; private set; }

        #region LocalPath
        private string _l;

        public string PhoneBookPath
        {
            get => _l;
            set
            {
                try
                {
                    _l = value;
                    var f = new FileInfo(value);
                    LocalPathOk = (Directory.Exists(f.DirectoryName) ? 1 : 0)
                                  + (f.Exists ? 1 : 0);

                    if (LocalPathOk == 2 && File.Exists(Path.Combine(f.DirectoryName, "ContactSyncSettings.json")))
                    {
                        SettingsPath = Path.Combine(f.DirectoryName, "ContactSyncSettings.json");
                        PhonerConfigPath = Path.Combine(f.DirectoryName, "PhonerLite.ini");
                    }
                }
                catch (Exception)
                {
                    LocalPathOk = 0;
                }

                LocalBackground = Colors.StatusBrushesBg[LocalPathOk];
                LocalThickness = new Thickness(LocalPathOk > 0 ? 1 : 2);

                RaisePropertyChanged(nameof(PhoneBookPath));
                RaisePropertyChanged(nameof(LocalBackground));
                RaisePropertyChanged(nameof(LocalThickness));
                SetAllOk();
            }
        }

        [JsonIgnore]
        public int LocalPathOk { get; private set; }
        [JsonIgnore]
        public SolidColorBrush LocalBackground { get; set; }
        [JsonIgnore]
        public Thickness LocalThickness { get; set; }
        #endregion

        #region Extern
        private string _e;

        public string ExternPath
        {
            get => _e;
            set
            {
                _e = value;
                ExternPathOk = RateFileName(value);

                ExternBackground = Colors.StatusBrushesBg[ExternPathOk];
                ExternThickness = new Thickness(ExternPathOk > 0 ? 1 : 2);

                RaisePropertyChanged(nameof(ExternPath));
                RaisePropertyChanged(nameof(ExternBackground));
                RaisePropertyChanged(nameof(ExternThickness));
                SetAllOk();
            }
        }

        private static int RateFileName(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return 0;
                }

                var f = new FileInfo(value);

                // Existing File - all ok
                if (f.Exists)
                {
                    return (int)StatusColor.Ok;
                }

                if (string.IsNullOrWhiteSpace(f.Extension))
                {
                    return 0;
                }

                // Filename is valid - can be created
                File.Create(value);
                File.Delete(value);

                return (int)StatusColor.Problematic;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        [JsonIgnore]
        public int ExternPathOk { get; private set; }
        [JsonIgnore]
        public SolidColorBrush ExternBackground { get; set; }
        [JsonIgnore]
        public Thickness ExternThickness { get; set; }
        #endregion

        [JsonIgnore]
        public bool AllOk { get; private set; }

        private void SetAllOk()
        {
            AllOk = LocalPathOk > 0 && ExternPathOk > 0;
            RaisePropertyChanged(nameof(AllOk));
        }

        public DateTime LastRestart { get; set; }

        public int WaitingTimeInMinutes { get; set; }

        public string ToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
