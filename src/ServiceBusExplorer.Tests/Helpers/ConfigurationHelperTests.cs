#region Using Directives

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Microsoft.Azure.ServiceBusExplorer.Helpers;
using Microsoft.ServiceBus;
using NUnit.Framework;

#endregion

namespace Microsoft.Azure.ServiceBusExplorer.Tests.Helpers
{
    public enum Monster
    {
        Godzilla,
        KingKong,
        Zombie
    }

    public enum Crustacean
    {
        Shrimp,
        Crab,
        Lobster
    }

    [TestFixture]
    public class ConfigurationHelperTests
    {
        #region Constants
        // Common values
        const string keyDoesNotExistAnywhere = "nonExistingKey";

        // Bool keys
        const string KeyIsTrueInAppConfig = "useAscii";
        const string KeyWillExistOnlyInUserConfig = "willExistOnlyInUserConfig";

        // String testing constants
        const string TestDirectoryName = "Service Bus Explorer Tests";
        const string KeySharkWhichWillBeOverridden = "shark";
        const string ValueForOverridingShark = "Great white";
        const string AnotherShark = "Black reef shark";
        const string SharkValueInAppConfig = "Blue shark";

        // Enum testing constants
        const string KeyConnectivityModeWhichWillBeOverridden = "connectivityMode";
        const string KeyCrustaceanWillExistOnlyInUserConfig = "crustacean";

        // Float testings constants
        const string KeyWhaleWeightWhichWillBeOverridden = "whaleWeight";
        const string KeySharkWeightWillExistOnlyInUserConfig = "sharkWeight";

        // Int testings constants
        const string KeyWhaleLengthWhichWillBeOverridden = "whaleLength";
        const string KeySharkLengthWillExistOnlyInUserConfig = "sharkLength";
        #endregion

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

            // Get a value that does not exist in the user file
            var savePropertiesToFile = configuration.GetBoolValue(keyOnlyInAppConfig, false);
            Assert.AreEqual(savePropertiesToFile, true);

            // Get a value that will only exist in the user file
            var onlyInUserFile = configuration.GetBoolValue(KeyWillExistOnlyInUserConfig, false);
            Assert.AreEqual(onlyInUserFile, userFileShouldHaveValues);
        }

        private void TestReadingEnumValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
        {
            const string keyOnlyInAppConfig = "monster";

            // Get a value that do not exist in the application config file defaulting to empty
            var nonExistingValueAsDefault = configuration.GetEnumValue<RelayType>(keyDoesNotExistAnywhere);
            Assert.AreEqual(nonExistingValueAsDefault, RelayType.None);

            // Get a value that do not exist in the application config file defaulting to false
            var nonExistingValueAsNetEvent = configuration.GetEnumValue<RelayType>(keyDoesNotExistAnywhere,
                RelayType.NetEvent);
            Assert.AreEqual(nonExistingValueAsNetEvent, RelayType.NetEvent);

            // Get the value from the user file defaulting to empty. If userFileShouldHaveValues
            // is false then we should read from the application config and that value should be
            // AutoDetect
            var connectivityMode = configuration.GetEnumValue<ConnectivityMode>(KeyConnectivityModeWhichWillBeOverridden);
            Assert.AreEqual(connectivityMode, userFileShouldHaveValues ?
                ConnectivityMode.Https : ConnectivityMode.AutoDetect);

            // Get the value from the user file defaulting to Http
            connectivityMode = configuration.GetEnumValue<ConnectivityMode>(KeyConnectivityModeWhichWillBeOverridden, ConnectivityMode.Http);
            Assert.AreEqual(connectivityMode, userFileShouldHaveValues ?
                ConnectivityMode.Https : ConnectivityMode.AutoDetect);

            // Get a value that do not exist in the user file
            var monster = configuration.GetEnumValue<Monster>(keyOnlyInAppConfig);
            Assert.AreEqual(monster, Monster.KingKong);

            // Get a value that will only exist in the user file
            var onlyInUserFile = configuration.GetEnumValue<Crustacean>(KeyCrustaceanWillExistOnlyInUserConfig,
                Crustacean.Shrimp);
            Assert.AreEqual(onlyInUserFile, userFileShouldHaveValues ? Crustacean.Crab : Crustacean.Shrimp);
        }


        private void TestReadingFloatValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
        {
            const string keyOnlyInAppConfig = "monsterWeight";
            const float mediumNumber = 472.7865f;

            // Get a value that do not exist in the application config file defaulting to empty
            var nonExistingValueAsDefault = configuration.GetFloatValue(keyDoesNotExistAnywhere);
            Assert.IsTrue(NearlyEqual(nonExistingValueAsDefault, 0), 
                @"Value read from {nameof(keyDoesNotExistAnywhere)} was {nonExistingValueAsDefault} instead of 0.");

            // Get a value that do not exist in the application config file defaulting to false
            var nonExistingValueAsNetEvent = configuration.GetFloatValue(keyDoesNotExistAnywhere,
                mediumNumber);
            Assert.IsTrue(NearlyEqual(nonExistingValueAsDefault, mediumNumber),
                @"Value read from {nameof(keyDoesNotExistAnywhere)} was {nonExistingValueAsDefault} instead of 0.");

            // Get the value from the user file defaulting to empty. If userFileShouldHaveValues
            // is false then we should read from the application config and that value should be
            // AutoDetect
            var whaleWeight = configuration.GetFloatValue(KeyWhaleWeightWhichWillBeOverridden);
            Assert.AreEqual(connectivityMode, userFileShouldHaveValues ?
                ConnectivityMode.Https : ConnectivityMode.AutoDetect);
            Assert.IsTrue(NearlyEqual(whaleWeight, 0),
    @"Value read from {nameof(keyDoesNotExistAnywhere)} was {nonExistingValueAsDefault} instead of 0.");

            // Get the value from the user file defaulting to Http
            connectivityMode = configuration.GetFloatValue(KeyConnectivityModeWhichWillBeOverridden, ConnectivityMode.Http);
            Assert.AreEqual(connectivityMode, userFileShouldHaveValues ?
                ConnectivityMode.Https : ConnectivityMode.AutoDetect);
            Assert.IsTrue(NearlyEqual(nonExistingValueAsDefault, 0),
    @"Value read from {nameof(keyDoesNotExistAnywhere)} was {nonExistingValueAsDefault} instead of 0.");

            // Get a value that do not exist in the user file
            var monster = configuration.GetFloatValue(keyOnlyInAppConfig);
            Assert.IsTrue(NearlyEqual(nonExistingValueAsDefault, 0),
    @"Value read from {nameof(keyDoesNotExistAnywhere)} was {nonExistingValueAsDefault} instead of 0.");

            // Get a value that will only exist in the user file
            var onlyInUserFile = configuration.GetFloatValue(KeyCrustaceanWillExistOnlyInUserConfig,
                Crustacean.Shrimp);
            Assert.AreEqual(onlyInUserFile, userFileShouldHaveValues ? Crustacean.Crab : Crustacean.Shrimp);
            Assert.IsTrue(NearlyEqual(nonExistingValueAsDefault, 0),
    @"Value read from {nameof(keyDoesNotExistAnywhere)} was {nonExistingValueAsDefault} instead of 0.");
        }

        private void TestReadingStringValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
        {
            const string keyOnlyInAppConfig = "whale";
            const string ExtinctShark = "Megalodon";

            // Get a value that do not exist in the application config file defaulting to empty
            var nonExistingValueAsTrue = configuration.GetStringValue(keyDoesNotExistAnywhere, string.Empty);
            Assert.AreEqual(nonExistingValueAsTrue, string.Empty);

            // Get a value that do not exist in the application config file defaulting to false
            var nonExistingValueAsFalse = configuration.GetStringValue(keyDoesNotExistAnywhere, ExtinctShark);
            Assert.AreEqual(nonExistingValueAsFalse, ExtinctShark);

            // Get the value from the user file defaulting to empty. If userFileShouldHaveValues
            // is false then we should read from the application config and that value should be
            // sharkValueInAppConfig
            var sharkSpecies = configuration.GetStringValue(KeySharkWhichWillBeOverridden);
            Assert.AreEqual(sharkSpecies, userFileShouldHaveValues ?
                ValueForOverridingShark : SharkValueInAppConfig);

            // Get the value from the user file defaulting to false
            sharkSpecies = configuration.GetStringValue(KeySharkWhichWillBeOverridden, "Something else");
            Assert.AreEqual(sharkSpecies, userFileShouldHaveValues ?
                 ValueForOverridingShark : SharkValueInAppConfig);

            // Get a value that do not exist in the user file
            var whale = configuration.GetStringValue(keyOnlyInAppConfig);
            Assert.AreEqual(whale, "Gray whale");

            // Get a value that will only exist in the user file
            var onlyInUserFile = configuration.GetStringValue(KeyWillExistOnlyInUserConfig,
                ExtinctShark);
            Assert.AreEqual(onlyInUserFile, userFileShouldHaveValues ? AnotherShark : ExtinctShark);
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
            configurationWithoutUserFile.SetValue(KeyWillExistOnlyInUserConfig, true);

            // Test reading config values again
            TestReadingBoolValues(configurationWithoutUserFile, userFileShouldHaveValues: true);

            configurationWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingBoolValues(configurationWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestEnumValuesReadAndWrite()
        {
            // Make sure the user config file does not exist
            DeleteUserConfigFile();

            // Create the TwoFilesConfiguration object without a user file
            var configurationWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingEnumValues(configurationWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationWithoutUserFile.SetValue(KeyConnectivityModeWhichWillBeOverridden,
                ConnectivityMode.Https);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationWithoutUserFile.SetValue(KeyCrustaceanWillExistOnlyInUserConfig, Crustacean.Crab);

            // Test reading config values again
            TestReadingEnumValues(configurationWithoutUserFile, userFileShouldHaveValues: true);

            // Save the configuration to the user file
            configurationWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingEnumValues(configurationWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestFloatValuesReadAndWrite()
        {
            // Make sure the user config file does not exist
            DeleteUserConfigFile();

            // Create the TwoFilesConfiguration object without a user file
            var configurationWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingFloatValues(configurationWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationWithoutUserFile.SetValue(KeyConnectivityModeWhichWillBeOverridden,
                ConnectivityMode.Https);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationWithoutUserFile.SetValue(KeyCrustaceanWillExistOnlyInUserConfig, Crustacean.Crab);

            // Test reading config values again
            TestReadingFloatValues(configurationWithoutUserFile, userFileShouldHaveValues: true);

            // Save the configuration to the user file
            configurationWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingFloatValues(configurationWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestIntValuesReadAndWrite()
        {
            // Make sure the user config file does not exist
            DeleteUserConfigFile();

            // Create the TwoFilesConfiguration object without a user file
            var configurationWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingIntValues(configurationWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationWithoutUserFile.SetValue(KeyConnectivityModeWhichWillBeOverridden,
                ConnectivityMode.Https);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationWithoutUserFile.SetValue(KeyCrustaceanWillExistOnlyInUserConfig, Crustacean.Crab);

            // Test reading config values again
            TestReadingIntValues(configurationWithoutUserFile, userFileShouldHaveValues: true);

            // Save the configuration to the user file
            configurationWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingIntValues(configurationWithUserFile, userFileShouldHaveValues: true);
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
            TestReadingStringValues(configurationWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationWithoutUserFile.SetValue(KeySharkWhichWillBeOverridden,
                ValueForOverridingShark);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationWithoutUserFile.SetValue(KeyWillExistOnlyInUserConfig, AnotherShark);

            // Test reading config values again
            TestReadingStringValues(configurationWithoutUserFile, userFileShouldHaveValues: true);

            // Save the changes to the user file
            configurationWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingStringValues(configurationWithUserFile, userFileShouldHaveValues: true);
        }

        #region Private methods
        // From https://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp
        public static bool NearlyEqual(double a, double b, double epsilon = 0.0000f)
        {
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a == b)
            { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < Double.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < epsilon;
            }
            else
            { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }
        #endregion
    }
}
