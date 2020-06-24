using System;
using System.Collections.Generic;
using System.Text;
using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class PhonebookStruct
    {
        public int MyId { get; set; }
        public Computer[] Devices { get; set; }

        public Dictionary<string, AddressEntry> Addresses { get; set; }
    }
}
