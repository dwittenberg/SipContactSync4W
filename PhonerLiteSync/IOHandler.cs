using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public static class IOHandler
    {
        public static void SaveSettings(Settings settings)
        {
            try
            {
                settings.LastRestart = DateTime.Now;

                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(settings, options);

                WriteToFile(CsvHandler.SavePath, jsonString);
            }
            catch
            {
                // ignored
            }
        }

        public static Settings LoadSettings()
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

        public static void SaveExternPhoneBook(PhoneBook pb, string path)
        {
            try
            {
                pb.MyId = -1;

                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(pb, options);

                WriteToFile(path, jsonString);
            }
            catch
            {
                // ignored
            }
        }

        public static PhoneBook LoadExternPhoneBook(string path)
        {
            try
            {
                var jsonString = File.ReadAllText(path);
                var pb =  JsonSerializer.Deserialize<PhoneBook>(jsonString);

               pb.MyId = pb.Devices.First(c => c.Name == Environment.MachineName).Id;

               return pb;
            }
            catch
            {
                return new PhoneBook();
            }
        }

        public static void WriteToFile(string path, string contents)
        {
            var file = new FileInfo(path);

            if (!Directory.Exists(file.Directory.FullName))
            {
                Directory.CreateDirectory(file.Directory.FullName);
            }

            File.WriteAllText(path, contents);
        }
    }
}
