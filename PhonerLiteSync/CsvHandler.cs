using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public static class CsvHandler
    {
        public static readonly string DateTimeFormat = "yyyy-MM-dd HH:mm:ss,fff";
        public static readonly string SettingsPath = Environment.ExpandEnvironmentVariables(@"%appData%\PhonerLite\ContactSyncSettings.json");


        public static bool Run(string localPath, string externPath)
        {
            try
            {
                Console.WriteLine("Stop Phoner");
                    var pm = new PhonerManager();
                pm.KillPhoner();
                pm.CheckAutorunSetting();

                Console.WriteLine("Read Files");
                var localFile = IoHandler.LoadLocalCsv(localPath);
                var externFile = IoHandler.LoadExternPhoneBook(externPath);

                Console.WriteLine("Run Update");
                localFile = UpdateLocal(externFile, localFile);
                externFile = UpdateExternal(localFile, externFile);

                externFile = CleanUpExternal(externFile);

                Console.WriteLine("Write Files");
                IoHandler.SaveLocalCsv(localFile, localPath);
                IoHandler.SaveExternPhoneBook(externFile, externPath);


                Console.WriteLine("Start Phoner");
                pm.RunPhonerLite();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Dictionary<string, AddressEntry> UpdateLocal(PhoneBook externFile, Dictionary<string, AddressEntry> localFile)
        {
            if (externFile.Addresses == null)
            {
                return localFile;
            }
            
            var listOfChanges = externFile.Addresses.Values.Where(m =>
                m.LastChanger != null
                && m.LastChanger.Status != Status.UpToDate
                && m.LastChanger.Id != externFile.MyId).ToList();

            foreach (var exEntry in listOfChanges)
            {
                var newEntry = new AddressEntry
                {
                    Number = exEntry.Number,
                    Name = exEntry.Name,
                    Comment = exEntry.Comment,
                    AllComputers = exEntry.AllComputers,
                };
                
                if (localFile.ContainsKey(exEntry.Number))
                {
                    localFile.Remove(exEntry.Number);
                }

                if (exEntry.LastChanger.Status == Status.Removed)
                {
                    exEntry.MyStatus.Status = Status.Removed;
                }
                else // New, Update, Undefined
                {
                    localFile.Add(exEntry.Number, newEntry);
                    exEntry.MyStatus.Status = Status.UpToDate;
                }

                exEntry.MyStatus.LastChange = DateTime.Now;
            }

            return localFile;
        }

        private static PhoneBook UpdateExternal(Dictionary<string, AddressEntry> localFile, PhoneBook externFile)
        {
            // Create new Dictionary, when not exist
            if (externFile.Addresses == null)
            {
                var pcName = Environment.MachineName;
                Computer[] array = { new Computer { Id = 0, Name = pcName }, };
                externFile = new PhoneBook
                {
                    Addresses = new Dictionary<string, AddressEntry>(),
                    Computers = array,
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
                                LastChanger = new ComputerStatus(externFile.MyId, DateTime.Now, Status.UpToDate),
                                MyStatus = new ComputerStatus(externFile.MyId, DateTime.Now, Status.NewEntry),
                                AllComputers = new ComputerStatus[1]
                                    {new ComputerStatus(0, DateTime.Now, Status.UpToDate)}, // ToDo Check
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
                exContact.LastChanger =
                exContact.MyStatus = new ComputerStatus(externFile.MyId, DateTime.Now, Status.NewEntry);

                exContact.AllComputers = externFile.Computers
                    .Select(m => new ComputerStatus(m.Id, DateTime.MinValue, Status.Undefined)).ToArray();
                exContact.AllComputers[exContact.MyStatus.Id] = exContact.MyStatus;
                externFile.Addresses.Add(exContact.Number, exContact);
            }

            // Remove Entry
            externFile.Addresses
                .Where(m => m.Value.LastChanger.Status != Status.Removed && !localFile.ContainsKey(m.Key))
                .Select(m => m.Value)
                .ToList()
                .ForEach(exContact =>
                {
                    exContact.LastChanger =
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
                    exContact.LastChanger =
                    exContact.MyStatus = new ComputerStatus(externFile.MyId, DateTime.Now, Status.Edited);
                    exContact.AllComputers[exContact.MyStatus.Id] = exContact.MyStatus;
                }
            }

            return externFile;
        }

        private static PhoneBook CleanUpExternal(PhoneBook externFile)
        {
           var list = 
               externFile.Addresses.Values.ToList()
               .Where(address => address.AllComputers.Count(x => 
                   x.Status == address.AllComputers.First().Status 
                   && x.LastChange >= address.LastChanger.LastChange) 
                                 == address.AllComputers.Length)
               .ToArray();

           foreach (var entry in list)
           {
               if (entry.LastChanger.Status == Status.Removed)
               {
                   externFile.Addresses.Remove(entry.Number);
                   continue;
               }

               entry.LastChanger.Status = Status.UpToDate;
               entry.AllComputers.ToList().ForEach(c => c.Status = Status.UpToDate);
           }

           return externFile;
        }
    }
}
