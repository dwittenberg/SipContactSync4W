using System;
using System.IO;

namespace PhonerLiteSync.Model
{
    [Serializable]
    public class Settings
    {
        public Settings()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            LocalPath = Environment.ExpandEnvironmentVariables(appData + @"\PhonerLite\phonebook.csv");

            var userData = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            ExternPath = Environment.ExpandEnvironmentVariables(userData + @"\OneDrive\PhonerLite\phonebook.csv");
            
            WaitingTime = 30;
            LastRestart = DateTime.MinValue;
        }

        private string l;

        public string LocalPath
        {
            get => l;
            set
            {
                l = value;
                LocalPathOk = File.Exists(l);
                SetAllOk();
            }
        }

        public bool LocalPathOk { get; private set; }

        private string e;

        public string ExternPath
        {
            get => e;
            set
            {
                if (value == null || string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                e = value;
                var f = new FileInfo(value);

                if (Directory.Exists(f.DirectoryName))
                {
                    ExternPathOk = true;
                    SetAllOk();
                    return;
                }

                var d = Directory.CreateDirectory(f.DirectoryName);
                ExternPathOk = d.Exists;
                SetAllOk();
            }
        }

        public bool ExternPathOk { get; private set; }

        public bool AllOk { get; private set; }

        private void SetAllOk()
        {
            AllOk = LocalPathOk && ExternPathOk;
        }

        public DateTime LastRestart { get; set; }

        // WaitingTime in minutes
        public int WaitingTime { get; set; }
    }
}
