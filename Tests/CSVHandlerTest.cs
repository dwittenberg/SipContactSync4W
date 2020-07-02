using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PhonerLiteSync;
using PhonerLiteSync.Model;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tests
{
    public class CSVHandlerTest
    {
        [Fact]
        public void ReadLocalCsvSettings()
        {
            IOHandler.WriteToFile(Variables.SettingsPath, Variables.Settings().ToJson());
            IOHandler.WriteToFile(Variables.ExternPath, Variables.Extern1);
            IOHandler.WriteToFile(Variables.InternPath, Variables.Intern1);

            var handler = new CsvHandler();
            handler.Run(Variables.InternPath, Variables.ExternPath);

            var now = DateTime.Now;
            var s1 = new Settings(Variables.SettingsPath)
            {
                LastRestart = now,
            };

            var s2 = Variables.Settings();
            s2.LastRestart = now;

            Assert.Equal(s2.ToJson(), s1.ToJson());
        }

        [Fact]
        public void ReadLocalCsvExtern()
        {
            IOHandler.WriteToFile(Variables.SettingsPath, Variables.Settings().ToJson());
            IOHandler.WriteToFile(Variables.ExternPath, Variables.Extern1);
            IOHandler.WriteToFile(Variables.InternPath, Variables.Intern1);

            var handler = new CsvHandler();
            handler.Run(Variables.InternPath, Variables.ExternPath);

            var e1 = File.ReadAllText(Variables.ExternPath);

            Assert.Equal(Variables.Extern2.Trim(), e1.Trim());
        }

        [Fact]
        public void ReadLocalCsvIntern()
        {
            IOHandler.WriteToFile(Variables.SettingsPath, Variables.Settings().ToJson());
            IOHandler.WriteToFile(Variables.ExternPath, Variables.Extern1);
            IOHandler.WriteToFile(Variables.InternPath, Variables.Intern1);

            var handler = new CsvHandler();
            handler.Run(Variables.InternPath, Variables.ExternPath);

            Assert.Equal(Variables.Intern2.Trim(), File.ReadAllText(Variables.InternPath).Trim());
        }
    }
}
