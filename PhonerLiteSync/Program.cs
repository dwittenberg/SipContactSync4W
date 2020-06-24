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
    class Program
    {
        static void Main(string[] args)
        {

            var handler = new CsvHandler();
            handler.run();
        }
    }
}
