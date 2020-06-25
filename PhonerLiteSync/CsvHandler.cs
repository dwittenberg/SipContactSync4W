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

        private string localPath;

        private string externPath;

        private PhonebookStruct externFile;

        private Dictionary<string, AddressEntry> localFile = new Dictionary<string, AddressEntry>();

        public void run(string local, string ext)
        {
            var phonerPath = Helper.KillPhoner();

            localPath = local;
            externPath = ext;

            Console.WriteLine("Read Files");
            externFile = ReadExternalCsv(externPath);
            localFile = ReadLocalCsv(localPath);

            Console.WriteLine("Run Update");
            localFile = UpdateLocal(externFile, localFile);
            externFile = UpdateExternal(localFile, externFile);

            Console.WriteLine("Write Files");
            WriteExternal(externFile);
            WriteLocal(localFile);

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

        private static PhonebookStruct ReadExternalCsv(string path)
        {
            var result = new PhonebookStruct();
            if (!File.Exists(path))
            {
                return result;
            }

            TextFieldParser parser = new TextFieldParser(path)
            {
                TextFieldType = FieldType.Delimited
            };

            parser.SetDelimiters(";");

            if (!parser.EndOfData)
            {
                var headline = parser.ReadFields();
                result.MyId = ReadHeadLine(headline);
                result.Devices = headline.Where(m => m != "#").Select(m => new Computer(m)).ToArray();
                if (result.Devices.Length == result.MyId)
                {
                    result.Devices = result.Devices.Append(new Computer(result.MyId + Environment.MachineName)).ToArray();
                }

                result.Addresses = new Dictionary<string, AddressEntry>();
            }

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                var entry = new AddressEntry(fields, result);

                if (!string.IsNullOrEmpty(entry.Number))
                {
                    result.Addresses.Add(entry.Number, entry);
                }
            }

            return result;
        }

        private static int ReadHeadLine(string[] fields)
        {
            int i = -1;
            if (fields[0] != "#")
            {
                // No Correct Headline
                return i;
            }

            var myName = Environment.MachineName;
            for (i = 0; i < fields.Length; i++)
            {
                if (fields[i] == (i - 1) + myName)
                {
                    return i - 1;
                }
            }

            // His computer read syc file first time
            return i - 1;
        }

        private static Dictionary<string, AddressEntry> UpdateLocal(PhonebookStruct externFile, Dictionary<string, AddressEntry> localFile)
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

        private PhonebookStruct UpdateExternal(Dictionary<string, AddressEntry> localFile, PhonebookStruct externFile)
        {
            // Create new, when not exist
            if (externFile.Addresses == null)
            {
                var pcName = Environment.MachineName;
                Computer[] array = { new Computer { Id = 0, Name = pcName }, };
                externFile = new PhonebookStruct
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

        private void WriteLocal(Dictionary<string, AddressEntry> localFile)
        {
            var csv = new StringBuilder();
            localFile.Values.ToList().ForEach(m => csv.AppendLine(m.ToLocalString()));
            WriteToFile(localPath, csv.ToString());
        }

        private void WriteExternal(PhonebookStruct externFile)
        {
            var csv = new StringBuilder();

            // Writhe Headline
            var headline = "#;";
            externFile.Devices.ToList().ForEach(m => headline += m + ";");
            csv.AppendLine(headline.Substring(0, headline.Length - 1));

            // Writhe Contacts
            externFile.Addresses.Values.ToList().ForEach(m =>
                csv.AppendLine(m.ToExternString(externFile.MyId, DateTimeFormat)));

            // Writhe to file
            WriteToFile(externPath, csv.ToString());
        }

        private void WriteToFile(string path, string contents)
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
