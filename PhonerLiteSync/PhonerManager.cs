using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class PhonerManager
    {
        private string _phonerExePath;

        private static readonly string WhereAreTheSettings =
            Environment.ExpandEnvironmentVariables(@"%appData%\PhonerLiteSync\SyncSettingsPath.txt");

        public void KillPhoner()
        {
            var proc = Process.GetProcesses().
                Where(p => p.ProcessName.Contains("PhonerLite") && p.ProcessName != "PhonerLiteSync").ToList();

            foreach (var p in proc.Where(p => p != null))
            {
                _phonerExePath = p.MainModule.FileName;

                try
                {
                    var i = 0;
                    while (i < 40 && p.MainModule != null)
                    {
                        p.CloseMainWindow();
                        Thread.Sleep(500);
                        i++;
                    }

                    p.Close();
                }
                catch (Exception)
                { }
            }
        }

        public bool RunPhonerLite()
        {
            if (!File.Exists(_phonerExePath))
            {
                return false;
            }

            Process.Start(_phonerExePath);
            return true;
        }

        public void CheckAutorunSetting(string phonerConfigPath)
        {
            var text = File.ReadAllText(phonerConfigPath);
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(exePath) ?? "";

            if (text.Contains(exeDir))
            {
                return;
            }

            var newText = new Regex("terminated=(.)*").Replace(text, $"terminated={exeDir}\\syncstart.bat");
            File.WriteAllText(phonerConfigPath, newText);
        }

        public static Settings LoadSettings()
        {
            try
            {
                var settingsPath = File.ReadAllText(WhereAreTheSettings);
                var jsonString = File.ReadAllText(settingsPath);
                return JsonSerializer.Deserialize<Settings>(jsonString);
            }
            catch
            {
                return new Settings();
            }
        }

        public static void SaveSettings(Settings settings)
        {
            try
            {
                var f = new FileInfo(WhereAreTheSettings);
                if (!Directory.Exists(f.DirectoryName))
                {
                    Directory.CreateDirectory(f.DirectoryName);
                }

                File.WriteAllText(WhereAreTheSettings, settings.SettingsPath);

                settings.LastRestart = DateTime.Now;

                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(settings, options);

                IoHandler.WriteToFile(settings.SettingsPath, jsonString);
            }
            catch
            {
                // ignored
            }
        }
    }
}
