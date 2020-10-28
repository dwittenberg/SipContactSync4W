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
                    //p.CloseMainWindow();




                    Process.Start("CMD.exe", "taskkill /IM \"PhonerLite.exe\"");



                    //p.Close();
                    while (!p.HasExited)
                    {
                        Thread.Sleep(300);
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
