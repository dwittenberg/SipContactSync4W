using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;
using PhonerLiteSync;
using PhonerLiteSync.Model;
using Xunit;

namespace Tests
{
    public class IOTester
    {
        private string pathSettings = Variables.SettingsPath;
        private string pathPhoneBook = Variables.SettingsPath;

        [Fact]
        public void SettingsWriteRead()
        {
            IoHandler.SaveSettings(Variables.Settings(), pathSettings);
            var settings2 = IoHandler.LoadSettings(pathSettings);

            Assert.Equal(Variables.Settings().ToString(), settings2.ToString());
        }

        [Fact]
        public void SettingsReadWrite()
        {
            var before = File.ReadAllLines(pathSettings).ToString();

            var settings = IoHandler.LoadSettings(pathSettings);
            IoHandler.SaveSettings(settings, pathSettings);

            var after = File.ReadAllLines(pathSettings).ToString();

            Assert.Equal(before, after);
        }

        [Fact]
        public void PhoneBookWriteRead()
        {

            IoHandler.SaveExternPhoneBook(Variables.LittlePhoneBook(), pathPhoneBook);
            var phoneBook2 = IoHandler.LoadExternPhoneBook(pathPhoneBook);

            Assert.Equal(Variables.LittlePhoneBook().ToString(), phoneBook2.ToString());
        }

        [Fact]
        public void PhoneBookReadWrite()
        {
            var before = File.ReadAllLines(pathPhoneBook).ToString();

            var phoneBook2 = IoHandler.LoadExternPhoneBook(pathPhoneBook);
            IoHandler.SaveExternPhoneBook(Variables.LittlePhoneBook(), pathPhoneBook);

            var after = File.ReadAllLines(pathPhoneBook).ToString();

            Assert.Equal(before, after);
        }
    }
}
