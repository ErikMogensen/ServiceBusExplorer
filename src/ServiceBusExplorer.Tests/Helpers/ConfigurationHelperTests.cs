#region Using Directives

using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Azure.ServiceBusExplorer.Helpers;
using NUnit.Framework;

#endregion

namespace Microsoft.Azure.ServiceBusExplorer.Tests.Helpers
{
    [TestFixture]
    public class ConfigurationHelperTests
    {
        private const string KeyIsTrueInAppConfig = "useAscii";
        private const string KeyWillExistOnlyInUserConfig = "willExistOnlyInUserConfig";
        private const string TestDirectoryName = "Service Bus Explorer Tests";


        //private static string GetFakeApplicationConfigFilePath()
        //{
        //    // Put it in the users non roaming personal folder
        //    return Path.Combine(Environment.
        //        GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        //        TestDirectoryName, 
        //        "ApplicationSettings.config");
        //}

        private static string GetUserSettingsFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                TestDirectoryName, 
                "UserSettings.config");
        }

        //private void CreateNewFakeApplicationConfigFile(string build)
        //{
        //    // Delete the old one if it exists
        //    DeleteFakeApplicationConfigFile();

        //    // Make sure the directory for the application config file exists
        //    var applicationConfigDirectory = Path.GetDirectoryName(GetFakeApplicationConfigFilePath());
        //    Directory.CreateDirectory(applicationConfigDirectory);

        //    // Copy the config file from the build
        //    var sbeConfigpath = Path.Combine(
        //        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        //        @"..\..\..\ServiceBusExplorer\bin",
        //        build,
        //        "ServiceBusExplorer.exe.config");

        //    File.Copy(sbeConfigpath, GetFakeApplicationConfigFilePath());
        //}

        //private void DeleteFakeApplicationConfigFile()
        //{
        //    DeleteFile(GetFakeApplicationConfigFilePath());
        //}

        private void DeleteUserConfigFile()
        {
            DeleteFile(GetUserSettingsFilePath());
        }

        private void TestReadingBoolValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
        {
            const string keyDoesNotExistAnywhere = "nonExistingKey";
            const string keyOnlyInAppConfig = "savePropertiesToFile";

            // Get a value that do not exist in the application config file defaulting to true
            var nonExistingValueAsTrue = configuration.GetBoolValue(keyDoesNotExistAnywhere, true);
            Assert.AreEqual(nonExistingValueAsTrue, true);

            // Get a value that do not exist in the application config file defaulting to false
            var nonExistingValueAsFalse = configuration.GetBoolValue(keyDoesNotExistAnywhere, false);
            Assert.AreEqual(nonExistingValueAsFalse, false);

            // Get the value from the user file defaulting to true. If userFileShouldHaveUseAsciiKeyAsFalse
            // is false then we should read from the application config and that value should be true
            var useAsciiDefTrue = configuration.GetBoolValue(KeyIsTrueInAppConfig, true);
            Assert.AreEqual(useAsciiDefTrue, !userFileShouldHaveValues);

            // Get the value from the user file defaulting to false
            var useAsciiDefFalse = configuration.GetBoolValue(KeyIsTrueInAppConfig, false);
            Assert.AreEqual(useAsciiDefFalse, !userFileShouldHaveValues);

            // Get a value that do not exist in the user file
            var savePropertiesToFile = configuration.GetBoolValue(keyOnlyInAppConfig, false);
            Assert.AreEqual(savePropertiesToFile, true);
        }

        private static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        [SetUp]
        public void Setup()
        {
            // Find out which type of build we should target by checking the name of the parent 
            // directory
            //var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //var startPosition = path.LastIndexOf('\\') + 1;
            //var buildName = path.Substring(startPosition);

            //CreateNewFakeApplicationConfigFile(buildName);
        }

        [TearDown]
        public void TearDown()
        {
            //DeleteFakeApplicationConfigFile();
        }

        [Test]
        public void TestBoolValuesReadAndWrite()
        {
            // Make sure the user config file does not exist
            DeleteUserConfigFile();

            // Create the TwoFilesConfiguration object without a user file
            var configurationWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingBoolValues(configurationWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationWithoutUserFile.SetValue(KeyIsTrueInAppConfig, false);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationWithoutUserFile.SetValue(KeyWillExistOnlyInUserConfig, false);

            // Test reading config values again
            TestReadingBoolValues(configurationWithoutUserFile, userFileShouldHaveValues: true);

            configurationWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object with a user file
            var configurationWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingBoolValues(configurationWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestStringValuesReadAndWrite()
        {
            // Make sure the user config file does not exist
            DeleteUserConfigFile();

            // Create the TwoFilesConfiguration object without a user file
            var configurationWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingBoolValues(configurationWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationWithoutUserFile.SetValue(KeyIsTrueInAppConfig, false);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationWithoutUserFile.SetValue(KeyWillExistOnlyInUserConfig, false);

            // Test reading config values again
            TestReadingBoolValues(configurationWithoutUserFile, userFileShouldHaveValues: true);

            configurationWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object with a user file
            var configurationWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingBoolValues(configurationWithUserFile, userFileShouldHaveValues: true);
        }


        //[Test]
        public void GetValuesFromApplicationFile()
        {
            TwoFilesConfiguration configuration = ConfigurationHandler.OpenConfiguration();

            // Initially the user
            var onlyInUserFile = configuration.GetBoolValue("OnlyInUserFile", false);
            Assert.AreEqual(onlyInUserFile, false);
        }

        //[Test]
        public void GetValuesFromUserFile()
        {

        }
    }
}
