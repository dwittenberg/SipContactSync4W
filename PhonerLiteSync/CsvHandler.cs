using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class CsvHandler
    {
        private PhonebookStruct _externFile;
        private Dictionary<string, AddressEntry> _localFile = new Dictionary<string, AddressEntry>();
        public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss,fff";
        private string _localPath;
        private string _externPath;

        public void run()
        {
            _localPath = Environment.ExpandEnvironmentVariables(@"%APPDATA%\PhonerLite\phonebook.csv");
            _externPath = Environment.ExpandEnvironmentVariables(@"C:\Users\dowi\OneDrive\ArrowBackup\PhonerLite\phonebook.csv");

            Console.WriteLine("Read Files");
            _externFile = ReadExternalCsv(_externPath);
            _localFile = ReadLocalCsv(_localPath);

            Console.WriteLine("Run Update");
            _localFile = UpdateLocal(_externFile, _localFile);
            _externFile = UpdateExternal(_localFile, _externFile);

            Console.WriteLine("Write Files");
            WriteExternal(_externFile);
            WriteLocal(_localFile);
        }

        private static Dictionary<string, AddressEntry> ReadLocalCsv(string path)
        {
            var result = new Dictionary<string, AddressEntry>();
            if (!File.Exists(path))
            {
                return result;
            }

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");

                while (!parser.EndOfData)
                {
                    //Process row
                    string[] fields = parser.ReadFields();

                    var entry = new AddressEntry(fields);

                    if (entry != null)
                    {
                        result.Add(entry.Number, entry);
                    }
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

            using TextFieldParser parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(";");

            if (!parser.EndOfData)
            {
                var headline = parser.ReadFields();
                result.MyPosition = ReadHeadLine(headline);
                result.Devices = headline.Where(m => m != "#").Select(m => new Computer(m)).ToArray();
                result.Addresses = new Dictionary<string, AddressEntry>();
            }

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                var entry = new AddressEntry(fields, result);

                if (entry != null)
                {
                    result.Addresses.Add(entry.Number, entry);
                }
            }

            return result;
        }

        private static int ReadHeadLine(string[] fields)
        {
            if (fields[0] != "#")
            {
                // No Correct Headline
                return -1;
            }

            var myName = Environment.MachineName;
            for (var i = 0; i < fields.Length; i++)
            {
                if (fields[i] == myName)
                {
                    return i - 1;
                }
            }

            // His computer read syc file first time
            return 0;
        }

        private static Dictionary<string, AddressEntry> UpdateLocal(PhonebookStruct externFile, Dictionary<string, AddressEntry> localFile)
        {
            if (externFile.Addresses == null)
            {
                return localFile;
            }

            var list = externFile.Addresses.Values.Where(m => m.ToDoStatus != Status.UpToDate && m.ToDoLastChange != m.MyLastReadWrithe).ToList();

            foreach (var exEntry in list)
            {
                var newEntry = new AddressEntry
                {
                    Number = exEntry.Number,
                    Name = exEntry.Name,
                    Comment = exEntry.Comment,
                    AllComputers = new ComputerStatus[0], // ToDo Check
                };

                switch (exEntry.ToDoStatus)
                {
                    case Status.NewEntry:
                        localFile.Add(newEntry.Number, newEntry);
                        exEntry.ToDoStatus = Status.UpToDate;
                        break;

                    case Status.Removed:
                        if (localFile.ContainsKey(exEntry.Number))
                        {
                            localFile.Remove(exEntry.Number);
                        }

                        break;

                    case Status.Edited:
                        if (localFile.ContainsKey(exEntry.Number))
                        {
                            localFile.Remove(exEntry.Number);
                        }

                        localFile.Add(newEntry.Number, newEntry);
                        exEntry.ToDoStatus = Status.UpToDate;
                        break;
                }

                exEntry.AllComputers[externFile.MyPosition].LastChange = DateTime.Now;
                exEntry.MyLastReadWrithe = DateTime.Now;
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
                    MyPosition = 0
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
                                ToDoStatus = Status.NewEntry,
                                AllComputers = new ComputerStatus[0], // ToDo Check
                                MyLastReadWrithe = DateTime.Now,
                            }));

                return externFile;
            }

            // Add new Entry
            localFile
                  .Where(m => !externFile.Addresses.ContainsKey(m.Key))
                  .Select(m => m.Value)
                  .ToList()
                  .ForEach(contact =>
                  {
                      contact.MyLastReadWrithe = DateTime.Now;
                      contact.ToDoStatus = Status.NewEntry;
                      contact.AllComputers[externFile.MyPosition].Status = Status.NewEntry;
                      externFile.Addresses.Add(contact.Number, contact);
                  });

            // Remove Entry
            externFile.Addresses
                .Where(m => !localFile.ContainsKey(m.Key))
                .Select(m => m.Value)
                .ToList()
                .ForEach(contact =>
                {
                    contact.MyLastReadWrithe = DateTime.Now;
                    contact.ToDoStatus = Status.Removed;
                    contact.AllComputers[externFile.MyPosition].Status = Status.Removed;
                });

            // Update Entry
            foreach (var exContact in externFile.Addresses.Values)
            {
                if (localFile.TryGetValue(exContact.Number, out var locContact) && !locContact.Equal(exContact))
                {
                    exContact.Name = locContact.Name;
                    exContact.Comment = locContact.Comment;
                    exContact.ToDoStatus = Status.Edited;
                    exContact.MyLastReadWrithe = DateTime.Now;
                    exContact.AllComputers.ToList().ForEach(m => m.Status = Status.UpToDate);
                }
            }

            return externFile;
        }

        private void WriteLocal(Dictionary<string, AddressEntry> localFile)
        {
            var csv = new StringBuilder();
            localFile.Values.ToList().ForEach(m => csv.AppendLine(m.ToLocalString()));
            WriteToFile(_localPath, csv.ToString());
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
                csv.AppendLine(m.ToExternString(externFile.MyPosition, DateTimeFormat)));

            // Writhe to file
            WriteToFile(_externPath, csv.ToString());
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
