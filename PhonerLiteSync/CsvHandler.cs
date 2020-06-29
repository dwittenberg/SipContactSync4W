using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class CsvHandler
    {
        public static readonly string DateTimeFormat = "yyyy-MM-dd HH:mm:ss,fff";
        public static readonly string SavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PhonerLiteContactSync\Settings.json";


        public void Run(string localPath, string externPath)
        {
            Console.WriteLine("Stop Phoner");
            var phonerPath = Helper.KillPhoner();

            Console.WriteLine("Read Files");
            var externFile = IOHandler.LoadExternPhoneBook(externPath);
            var localFile = ReadLocalCsv(localPath);

            Console.WriteLine("Run Update");
            localFile = UpdateLocal(externFile, localFile);
            externFile = UpdateExternal(localFile, externFile);

            Console.WriteLine("Write Files");
            IOHandler.SaveExternPhoneBook(externFile, externPath);
            WriteLocalCsv(localFile, localPath);


            Console.WriteLine("Start Phoner");
            Helper.RunPhonerLite(phonerPath);
        }

        private static Dictionary<string, AddressEntry> ReadLocalCsv(string path)
        {
            var result = new Dictionary<string, AddressEntry>();
            if (!File.Exists(path))
            {
                return result;
            }

            TextFieldParser parser = new TextFieldParser(path)
            {
                TextFieldType = FieldType.Delimited
            };

            parser.SetDelimiters(";");

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                var entry = new AddressEntry(fields);

                if (!string.IsNullOrEmpty(entry.Number))
                {
                    result.Add(entry.Number, entry);
                }
            }

            return result;
        }

        private static Dictionary<string, AddressEntry> UpdateLocal(PhoneBook externFile, Dictionary<string, AddressEntry> localFile)
        {
            if (externFile.Addresses == null)
            {
                return localFile;
            }

            var listOfChanges = externFile.Addresses.Values.Where(m =>
                (m.LastChanger != null
                && m.LastChanger.Status != Status.UpToDate
                && m.LastChanger.Id != externFile.MyId)
                || m.MyStatus.Status == Status.NewEntry).ToList();

            foreach (var exEntry in listOfChanges)
            {
                var newEntry = new AddressEntry
                {
                    Number = exEntry.Number,
                    Name = exEntry.Name,
                    Comment = exEntry.Comment,
                    AllComputers = exEntry.AllComputers,
                };

                switch (exEntry.LastChanger.Status)
                {
                    case Status.NewEntry:
                        if (localFile.ContainsKey(newEntry.Number))
                        {
                            localFile.Remove(newEntry.Number);
                        }

                        localFile.Add(newEntry.Number, newEntry);
                        exEntry.MyStatus.Status = Status.UpToDate;
                        break;

                    case Status.Removed:
                        if (localFile.ContainsKey(exEntry.Number))
                        {
                            localFile.Remove(exEntry.Number);
                            exEntry.MyStatus.Status = Status.Removed;
                        }

                        break;

                    case Status.Edited:
                        if (localFile.ContainsKey(exEntry.Number))
                        {
                            localFile.Remove(exEntry.Number);
                        }

                        localFile.Add(newEntry.Number, newEntry);

                        exEntry.MyStatus.Status = Status.UpToDate;
                        break;
                }

                exEntry.MyStatus.LastChange = DateTime.Now;
            }

            return localFile;
        }

        private static PhoneBook UpdateExternal(Dictionary<string, AddressEntry> localFile, PhoneBook externFile)
        {
            // Create new, when not exist
            if (externFile.Addresses == null)
            {
                var pcName = Environment.MachineName;
                Computer[] array = { new Computer { Id = 0, Name = pcName }, };
                externFile = new PhoneBook
                {
                    Addresses = new Dictionary<string, AddressEntry>(),
                    Devices = array,
                    MyId = 0
                };

                localFile.Values.ToList()
                    .ForEach(m =>
                        externFile.Addresses.Add(
                            m.Number,
                            new AddressEntry
                            {
                                Number = m.Number,
                                Name = m.Name,
                                Comment = m.Comment,
                                MyStatus = new ComputerStatus(externFile.MyId, DateTime.Now, Status.NewEntry),
                                AllComputers = new ComputerStatus[1] { new ComputerStatus(0, DateTime.Now, Status.UpToDate) }, // ToDo Check
                            }));

                return externFile;
            }

            // Add new Entry
            var a = localFile
                .Where(m => !externFile.Addresses.ContainsKey(m.Key))
                .Select(m => m.Value)
                .ToList();
            foreach (var exContact in a)
            {
                exContact.MyStatus = new ComputerStatus(externFile.MyId, DateTime.Now, Status.NewEntry);

                exContact.AllComputers = externFile.Devices.Select(m => new ComputerStatus(m.Id, DateTime.MinValue, Status.Undefined)).ToArray();
                exContact.AllComputers[exContact.MyStatus.Id] = exContact.MyStatus;
                externFile.Addresses.Add(exContact.Number, exContact);
            }

            // Remove Entry
            externFile.Addresses
                .Where(m => !localFile.ContainsKey(m.Key))
                .Select(m => m.Value)
                .ToList()
                .ForEach(exContact =>
                {
                    exContact.MyStatus = new ComputerStatus(externFile.MyId, DateTime.Now, Status.Removed);
                    exContact.AllComputers[exContact.MyStatus.Id] = exContact.MyStatus;
                });

            // Update Entry
            foreach (var exContact in externFile.Addresses.Values)
            {
                if (localFile.TryGetValue(exContact.Number, out var locContact) && !locContact.Equal(exContact))
                {
                    exContact.Name = locContact.Name;
                    exContact.Comment = locContact.Comment;
                    exContact.MyStatus = new ComputerStatus(externFile.MyId, DateTime.Now, Status.Edited);
                    exContact.AllComputers[exContact.MyStatus.Id] = exContact.MyStatus;
                }
            }

            return externFile;
        }

        private static void WriteLocalCsv(Dictionary<string, AddressEntry> localFile, string path)
        {
            var csv = new StringBuilder();
            localFile.Values.ToList().ForEach(m => csv.AppendLine(m.ToLocalString()));

            IOHandler.WriteToFile(path, csv.ToString());
        }
    }
}
