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
            var p = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "PhonerLite");

            if (p == null)
            {
                return;
            }

            _phonerPath = p.MainModule.FileName;

            while (!p.CloseMainWindow())
            {
                Thread.Sleep(300);
            }

            Thread.Sleep(300);
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
