using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class Helper
    {
        public void SaveSettings(Settings settings)
        {
            try
            {
                settings.LastRestart = DateTime.Now;
                   
                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize<Settings>(settings, options);

                var file = new FileInfo(CsvHandler.SavePath);
                if (!Directory.Exists(file.DirectoryName))
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }

                File.WriteAllText(CsvHandler.SavePath, jsonString);
            }
            catch
            {
                // ignored
            }
        }

        public Settings LoadSettings()
        {
            try
            {
                var jsonString = File.ReadAllText(CsvHandler.SavePath);
                return JsonSerializer.Deserialize<Settings>(jsonString);
            }
            catch
            {
                return new Settings();
            }
        }

        public static string KillPhoner()
        {
            // Store all running process in the system
            Process[] runingProcess = Process.GetProcesses();
            for (int i = 0; i < runingProcess.Length; i++)
            {
                // compare equivalent process by their name
                if (runingProcess[i].ProcessName == "PhonerLite")
                {
                    // kill  running process
                    var p = runingProcess[i];
                    var path = p.MainModule.FileName;
                    p.Kill();
                    while (!p.HasExited)
                    {
                        Thread.Sleep(1000);
                    }

                    return path;
                }
            }

            return string.Empty;
        }

        public static bool RunPhonerLite(string path)
        {
            // create the  process to launch another exe file
            if (File.Exists(path))
            {
                Process.Start(path);
                return true;
            }

            return false;
        }
    }
}
