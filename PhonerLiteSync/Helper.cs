using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class Helper
    {
        public static string KillPhoner()
        {
            var p = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "PhonerLite");

            if (p == null)
            {
                return string.Empty;
            }
            
            while (!p.CloseMainWindow())
            {
                Thread.Sleep(300);
            }

            Thread.Sleep(300);
            return p.MainModule.FileName;
        }

        public static bool RunPhonerLite(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            Process.Start(path);
            return true;
        }
    }
}
