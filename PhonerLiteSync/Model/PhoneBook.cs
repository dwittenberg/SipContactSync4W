using System;
using System.Collections.Generic;
using System.Text.Json;
using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class PhoneBook
    {

        public PhoneBook()
        {
            Version = "PhoneBook by DW 1.0";
        }

        public string Version { get; }
        public int MyId { get; set; }
        public Computer[] Devices { get; set; }

        public Dictionary<string, AddressEntry> Addresses { get; set; }

        public string ToJson()
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
