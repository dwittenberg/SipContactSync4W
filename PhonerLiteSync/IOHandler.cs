using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.VisualBasic.FileIO;
using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public static class IoHandler
    {
        public static Settings LoadSettings(string path)
        {
            try
            {
                var jsonString = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Settings>(jsonString);
            }
            catch
            {
                return new Settings();
            }
        }

        public static void SaveSettings(Settings settings, string path)
        {
            try
            {
                settings.LastRestart = DateTime.Now;

                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(settings, options);

                WriteToFile(path, jsonString);
            }
            catch
            {
                // ignored
            }
        }

        public static Dictionary<string, AddressEntry> LoadLocalCsv(string path)
        {
            var result = new Dictionary<string, AddressEntry>();
            if (!File.Exists(path))
            {
                return result;
            }

            var parser = new TextFieldParser(path)
            {
                TextFieldType = FieldType.Delimited
            };

            parser.SetDelimiters(";");

            while (!parser.EndOfData)
            {
                //Process row
                var fields = parser.ReadFields();

                var entry = new AddressEntry(fields);

                if (!string.IsNullOrEmpty(entry.Number))
                {
                    result.Add(entry.Number, entry);
                }
            }

            return result;
        }

        public static void SaveLocalCsv(Dictionary<string, AddressEntry> localFile, string path)
        {
            var csv = new StringBuilder();
            localFile.Values.ToList().ForEach(m => csv.AppendLine(m.ToLocalString()));

            WriteToFile(path, csv.ToString());
        }

        public static PhoneBook LoadExternPhoneBook(string path)
        {
            try
            {
                var jsonString = File.ReadAllText(path);
                var pb = JsonSerializer.Deserialize<PhoneBook>(jsonString);

                var myComputer = pb.Computers.FirstOrDefault(c => c.Name == Environment.MachineName);
                if (myComputer == null)
                {
                    // Add this PC to PhoneBook
                    var id = pb.Computers.Length;
                    myComputer = new Computer { Id = id, Name = Environment.MachineName };
                    pb.Computers = pb.Computers.Append(myComputer).ToArray();

                    pb.Addresses.Values.ToList().ForEach(e => e.AllComputers = e.AllComputers.Append(new ComputerStatus { Id = id, LastChange = DateTime.MinValue, Status = Status.NewEntry }).ToArray());
                }

                pb.MyId = myComputer.Id;
                pb.Addresses.Values.ToList().ForEach(e => 
                    e.MyStatus = e.AllComputers[pb.MyId]);

                return pb;
            }
            catch (Exception)
            {
                if (!File.Exists(path))
                {
                    File.Create(path);
                }

                return new PhoneBook();
            }
        }

        public static void SaveExternPhoneBook(PhoneBook pb, string path)
        {
            try
            {
                pb.MyId = -1;
                WriteToFile(path, pb.ToJson());
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteToFile(string path, string contents)
        {
            var file = new FileInfo(path);

            if (!Directory.Exists(file.Directory.FullName))
            {
                Directory.CreateDirectory(file.Directory.FullName);
            }

            File.WriteAllText(path, contents,Encoding.UTF8);
        }
    }
}
