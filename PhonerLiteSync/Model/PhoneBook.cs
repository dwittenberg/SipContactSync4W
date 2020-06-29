using System;
using System.Collections.Generic;
using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class PhoneBook
    {

        public PhoneBook()
        {
            Version = "PhoneBook by DW 1.0";
        }

        public string Version { get; set; }
        public int MyId { get; set; }
        public Computer[] Devices { get; set; }

        public Dictionary<string, AddressEntry> Addresses { get; set; }
    }
}
