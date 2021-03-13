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
        private string _phonerPath;

        public void KillPhoner()
        {
            var proc = Process.GetProcesses().
                Where(p => p.ProcessName.Contains("PhonerLite")).ToList();

            foreach (var p in proc)
            {
                if (p == null)
                {
                    continue;
                }

                _phonerPath = p.MainModule.FileName;

                try
                {
                    int i = 0;
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
            if (!File.Exists(_phonerPath))
            {
                return false;
            }

            Process.Start(_phonerPath);
            return true;
        }

        public void CheckAutorunSetting()
        {
            var appData = @"%appData%/PhonerLite/PhonerLite.ini";
            var text = File.ReadAllText(Environment.ExpandEnvironmentVariables(appData));

            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(exePath) ?? "";

            if (text.Contains(exeDir))
            {
                return;
            }

            var newText = new Regex("terminated=(.)*").Replace(text, $"terminated={exeDir}\\syncstart.bat");
            File.WriteAllText(Environment.ExpandEnvironmentVariables(appData), newText);
        }
    }
}
