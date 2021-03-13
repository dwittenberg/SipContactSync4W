using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;
using PhonerLiteSync;
using PhonerLiteSync.Model;
using Xunit;

namespace Tests
{
    public class IoHandlerTest
    {
        private readonly string _pathSettings = Variables.SettingsPath;
        private readonly string _pathPhoneBook = Variables.SettingsPath;

        [Fact]
        public void SettingsWriteRead()
        {
            PhonerManager.SaveSettings(Variables.Settings());
            var settings2 = PhonerManager.LoadSettings();

            Assert.Equal(Variables.Settings().ToString(), settings2.ToString());
        }

        [Fact]
        public void SettingsReadWrite()
        {
            var before = File.ReadAllLines(_pathSettings).ToString();

            var settings = PhonerManager.LoadSettings();
            PhonerManager.SaveSettings(settings);

            var after = File.ReadAllLines(_pathSettings).ToString();

            Assert.Equal(before, after);
        }

        [Fact]
        public void PhoneBookWriteRead()
        {

            IoHandler.SaveExternPhoneBook(Variables.LittlePhoneBook(), _pathPhoneBook);
            var phoneBook2 = IoHandler.LoadExternPhoneBook(_pathPhoneBook);

            Assert.Equal(Variables.LittlePhoneBook().ToString(), phoneBook2.ToString());
        }

        [Fact]
        public void PhoneBookReadWrite()
        {
            var before = File.ReadAllLines(_pathPhoneBook).ToString();

            var phoneBook2 = IoHandler.LoadExternPhoneBook(_pathPhoneBook);
            IoHandler.SaveExternPhoneBook(Variables.LittlePhoneBook(), _pathPhoneBook);

            var after = File.ReadAllLines(_pathPhoneBook).ToString();

            Assert.Equal(before, after);
        }
    }
}
