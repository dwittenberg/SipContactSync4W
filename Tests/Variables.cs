using System;
using System.Collections.Generic;
using System.Text;
using PhonerLiteSync;
using PhonerLiteSync.Model;

namespace Tests
{
    public static class Variables
    {
        public static string SettingsPath = @"C:\tmp\settings.json";
        public static string ExternPath = @"C:\tmp\externPhonebook.json";
        public static string InternPath = @"C:\tmp\internPhonebook.csv";

        public static Settings Settings()
            => new Settings
            {
                ExternPath = ExternPath,
                LastRestart = DateTime.Now,
                LocalPath = @"C:\Users\Häschen\AppData\Roaming\PhonerLite\phonebook.csv",
                WaitingTime = 30
            };

        public static PhoneBook LittlePhoneBook()
        {
            var pcName = Environment.MachineName;
            Computer[] deviceArray = { new Computer { Id = 0, Name = pcName }, };

            ComputerStatus computerStatus =
                new ComputerStatus(0, new DateTime(2020, 6, 30, 14, 30, 0), Status.UpToDate);
            ComputerStatus[] computerStatusArray = { computerStatus };

            AddressEntry addressEntry = new AddressEntry()
            {
                MyStatus = computerStatus,
                LastChanger = computerStatus,
                Number = "01352 4423223",
                Name = "Häschen Liese",
                Comment = "Blumenstraus",
                AllComputers = computerStatusArray,
            };

            var addressDictionary = new Dictionary<string, AddressEntry>
            {
                {addressEntry.Number, addressEntry}
            };

            var phoneBook = new PhoneBook
            {
                Addresses = addressDictionary,
                Devices = deviceArray,
                MyId = 0,
            };

            return phoneBook;
        }

        public static PhoneBook LittlePhoneBook2()
        {

            ComputerStatus computerStatus =
                new ComputerStatus(0, DateTime.Now, Status.UpToDate);
            ComputerStatus[] computerStatusArray = { computerStatus };

            AddressEntry addressEntry = new AddressEntry()
            {
                MyStatus = computerStatus,
                LastChanger = computerStatus,
                Number = "0178222222",
                Name = "Test1",
                Comment = "Thx",
                AllComputers = computerStatusArray,
            };

            var pb = LittlePhoneBook();
            pb.MyId = -1;
            pb.Addresses.Add(addressEntry.Number, addressEntry);

            return pb;
        }

        //{
        //    "Version": "PhoneBook by DW 1.0",
        //    "MyId": -1,
        //    "Devices": [
        //    {
        //        "Id": 0,
        //        "Name": "WERKBANK"
        //    }
        //    ],
        //    "Addresses": {
        //        "01352 4423223": {
        //            "Name": "H\u00E4schen Liese",
        //            "Number": "01352 4423223",
        //            "Comment": "Blumenstraus",
        //            "AllComputers": [
        //            {
        //                "Id": 0,
        //                "Status": 4,
        //                "LastChange": "2020-06-30T14:30:00"
        //            }
        //            ],
        //            "LastChanger": {
        //                "Id": 0,
        //                "Status": 4,
        //                "LastChange": "2020-06-30T14:30:00"
        //            },
        //            "MyStatus": {
        //                "Id": 0,
        //                "Status": 4,
        //                "LastChange": "2020-06-30T14:30:00"
        //            }
        //        }
        //    }
        //}
        public static string Extern1 = LittlePhoneBook().ToJson();
        public static string Extern2 = LittlePhoneBook2().ToJson();

        public static string Intern1 = "0178222222;Test1;;Thx";

        public static string Intern2 = "0178222222;Test1;;Thx\r\n01352 4423223;H\u00E4schen Liese;;Blumenstraus";
    }
}
