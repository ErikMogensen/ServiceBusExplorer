﻿#region Using Directives

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
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

        // Float testing constants
        const string KeyWhaleWeightWillExistOnlyInUserConfig = "whaleWeight";
        const string KeySharkWeightWhichWillBeOverridden = "sharkWeight";
        const float ValueSharkWeightInAppConfig = 44.12f;
        const float ValueSharkWeightInUserConfig = 64.0f;
        const float ValueWhaleWeightInUserConfig = 8039.30f;

        // Int testing constants
        const string KeyWhaleLengthWillExistOnlyInUserConfig = "whaleLength";
        const string KeySharkLengthWhichWillBeOverridden = "sharkLength";
        const float ValueSharkLengthInAppConfig = 158;
        const float ValueSharkLengthInUserConfig = 172;
        const float ValueWhaleLengthInUserConfig = 634;

        // Hashtable constants
        const string KeyFreshWaterFishesWhichWillOnlyExistInUserConfig = "freshWaterFishes";
        const string KeySaltWaterFishesWhichWillBeMerged = "saltWaterFishes";
        const string ValueAlaskaPollock = "Alaska pollock";
        const string ValueAlaskaPollockOldName = "Theragra chalcogramma";
        const string ValueAlaskaPollockNewName = "Gadus chalcogrammus";

        // MessagingNamespaces constants
        const string KeyNamespaceInUserFile1 = "treasureInUserFile";
        const string KeyNamespaceInUserFile2 = "anotherTreasureInUserFile";
        const string KeyNamespaceInBothFiles = "usedInUserFileAndAppFile";
        const string KeyNamespaceInAppFile1 = "treasureInAppFile";

        readonly int IndexNamespaceInUserFile1 = 0;
        readonly int IndexNamespaceInUserFile2 = 1;
        //readonly int IndexFirstNamespaceInBothFiles = 2;
        //readonly int IndexSecondNamespaceInBothFiles = 3;
        //readonly int IndexNamespaceInAppFile1 = 4;


        // Indent size in config files
        const string indent = "  ";

        #endregion

        #region Private fields

        WriteToLogDelegate writeToLog;
        string logInMemory;

        readonly Dictionary<string, string> saltWaterFishes = new Dictionary<string, string>()
        {
            { "Atlantic chub mackerel", "Scomber colias" },
            { "Atlantic mackerel", "Scomber scombrus" },
            { "Alaska pollock", "Theragra chalcogramma" }
        };

        readonly Dictionary<string, string> freshWaterFishes = new Dictionary<string, string>()
        {
            { "Pike", "Esox lucius" },
            { "Perch", "Perca flavescens" },
            { "Zander","Sander lucioperca" }
        };

        readonly List<KeyValuePair<string, string>> fakeConnectionStrings = new List<KeyValuePair<string, string>>()
        {
            //{ KeyNamespaceInUserFile1,
            //    "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=SomeKey;SharedAccessKey=18347=" },

            //{ KeyNamespaceInUserFile2,
            //    "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=Root;SharedAccessKey=21452=" },

            //{ KeyNamespaceInBothFiles,
            //    "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=UserFile;SharedAccessKey=32345=" },

            //{ KeyNamespaceInBothFiles,
            //    "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=AppFile;SharedAccessKey=444445=" },

            //{ KeyNamespaceInAppFile1,
            //    "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=Root;SharedAccessKey=54442=" }

            { new KeyValuePair<string, string>("treasureInUserFile",
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=SomeKey;SharedAccessKey=18347=") },

            { new KeyValuePair<string, string>("anotherTreasureInUserFile",
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=Root;SharedAccessKey=21452=") },

            { new KeyValuePair<string, string>("usedInUserFileAndAppFile",
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=UserFile;SharedAccessKey=32345=") },

            { new KeyValuePair<string, string>("usedInUserFileAndAppFile",
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=AppFile;SharedAccessKey=444445=") },

            { new KeyValuePair<string, string>("treasureInAppFile",
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=Root;SharedAccessKey=54442=") }
        };


        #endregion

        #region The constructor

        public ConfigurationHelperTests()
        {
            // Initialize the delegate that handles logging
            writeToLog = WriteToLogInMemory;
        }

        #endregion

        #region Public methods   

        [SetUp]
        public void Setup()
        {
            // Make sure the user config file does not exist
            DeleteUserConfigFile();

            // Empty the log buffer
            logInMemory = string.Empty;
        }

        [Test]
        public void TestBoolValuesReadAndWrite()
        {
            var configurationOpenedWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingBoolValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationOpenedWithoutUserFile.SetValue(KeyIsTrueInAppConfig, false);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationOpenedWithoutUserFile.SetValue(KeyWillExistOnlyInUserConfig, true);

            // Test reading config values again
            TestReadingBoolValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: true);

            configurationOpenedWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationOpenedWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingBoolValues(configurationOpenedWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestEnumValuesReadAndWrite()
        {
            // Create the TwoFilesConfiguration object without a user file
            var configurationOpenedWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingEnumValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationOpenedWithoutUserFile.SetValue(KeyConnectivityModeWhichWillBeOverridden,
                ConnectivityMode.Https);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationOpenedWithoutUserFile.SetValue(KeyCrustaceanWillExistOnlyInUserConfig, Crustacean.Crab);

            // Test reading config values again
            TestReadingEnumValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: true);

            // Save the configuration to the user file
            configurationOpenedWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationOpenedWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingEnumValues(configurationOpenedWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestFloatValuesReadAndWrite()
        {
            // Create the TwoFilesConfiguration object without a user file
            var configurationOpenedWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingFloatValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationOpenedWithoutUserFile.SetValue(KeySharkWeightWhichWillBeOverridden,
                ValueSharkWeightInUserConfig);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationOpenedWithoutUserFile.SetValue(KeyWhaleWeightWillExistOnlyInUserConfig,
                ValueWhaleWeightInUserConfig);

            // Test reading config values again
            TestReadingFloatValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: true);

            // Save the configuration to the user file
            configurationOpenedWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationOpenedWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingFloatValues(configurationOpenedWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestIntValuesReadAndWrite()
        {
            // Create the TwoFilesConfiguration object without a user file
            var configurationOpenedWithoutUserFile = TwoFilesConfiguration.Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingIntValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationOpenedWithoutUserFile.SetValue(KeySharkLengthWhichWillBeOverridden,
                ValueSharkLengthInUserConfig);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationOpenedWithoutUserFile.SetValue(KeyWhaleLengthWillExistOnlyInUserConfig,
                ValueWhaleLengthInUserConfig);

            // Test reading config values again
            TestReadingIntValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: true);

            // Save the configuration to the user file
            configurationOpenedWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationOpenedWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingIntValues(configurationOpenedWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestStringValuesReadAndWrite()
        {
            // Create the TwoFilesConfiguration object without a user file
            var configurationOpenedWithoutUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingStringValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: false);

            // Set a value which will end up in the user file and already exists in the application config
            configurationOpenedWithoutUserFile.SetValue(KeySharkWhichWillBeOverridden,
                ValueForOverridingShark);

            // Set a value which will end up in the user file and does not exist in the application config
            configurationOpenedWithoutUserFile.SetValue(KeyWillExistOnlyInUserConfig, AnotherShark);

            // Test reading config values again
            TestReadingStringValues(configurationOpenedWithoutUserFile, userFileShouldHaveValues: true);

            // Save the changes to the user file
            configurationOpenedWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationOpenedWithUserFile = TwoFilesConfiguration
                .Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingStringValues(configurationOpenedWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestHashtableSectionReadAndWrite()
        {
            // Create the TwoFilesConfiguration object without a user file
            var configurationOpenedWithoutUserFile = TwoFilesConfiguration.Create(GetUserSettingsFilePath());

            // Test reading config values 
            TestReadingHashtableSection(configurationOpenedWithoutUserFile, userFileShouldHaveValues: false);

            // Add a new entry to a section existing in the application config that should end up in the user config 
            configurationOpenedWithoutUserFile.AddEntryToDictionarySection(KeySaltWaterFishesWhichWillBeMerged,
                "Atlantic mackerel", "Scomber scombrus");

            // Add an existing entry to a section existing in the application config that 
            // should end up in the user config. 
            configurationOpenedWithoutUserFile.AddEntryToDictionarySection(KeySaltWaterFishesWhichWillBeMerged,
                "Alaska Pollock", ValueAlaskaPollockNewName);

            foreach (var freshWaterFish in freshWaterFishes)
            {
                configurationOpenedWithoutUserFile.AddEntryToDictionarySection
                    (KeyFreshWaterFishesWhichWillOnlyExistInUserConfig, freshWaterFish.Key, freshWaterFish.Value);
            }

            // Test reading config values again
            TestReadingHashtableSection(configurationOpenedWithoutUserFile, userFileShouldHaveValues: true);

            // Persist the configuration
            configurationOpenedWithoutUserFile.Save();

            // Create the TwoFilesConfiguration object when a user file exists
            var configurationOpenedWithUserFile = TwoFilesConfiguration.Create(GetUserSettingsFilePath());

            // Test reading config values again 
            TestReadingHashtableSection(configurationOpenedWithUserFile, userFileShouldHaveValues: true);
        }

        [Test]
        public void TestMessagingNamespacesReadAndWrite()
        {
            // Create the TwoFilesConfiguration object without a user file
            var configurationOpenedWithoutUserFile = TwoFilesConfiguration.Create(GetUserSettingsFilePath());

            // Test reading config values - both application config and user config are missing
            var namespaces = ServiceBusNamespace.GetMessagingNamespaces
                (configurationOpenedWithoutUserFile, writeToLog);
            Assert.AreEqual(0, namespaces.Count);
            Assert.IsTrue(logInMemory.Contains("Service bus accounts have not been properly configured"));
            logInMemory = string.Empty;

            // Add connection strings to the user file config values - application config section is still missing
            SaveConnectionString(configurationOpenedWithoutUserFile, IndexNamespaceInUserFile1);
            SaveConnectionString(configurationOpenedWithoutUserFile, IndexNamespaceInUserFile2);
            Assert.IsEmpty(logInMemory);

            namespaces = ServiceBusNamespace.GetMessagingNamespaces
                (configurationOpenedWithoutUserFile, writeToLog);
            Assert.IsEmpty(logInMemory);
            Assert.AreEqual(2, namespaces.Count);
            Assert.AreEqual(fakeConnectionStrings[IndexNamespaceInUserFile1].Value, 
                namespaces[KeyNamespaceInUserFile1].ConnectionString);

            Assert.AreEqual(fakeConnectionStrings[IndexNamespaceInUserFile2].Value, namespaces[KeyNamespaceInUserFile2].ConnectionString);

            namespaces = ServiceBusNamespace.GetMessagingNamespaces
                (configurationOpenedWithoutUserFile, writeToLog);

            namespaces = ServiceBusNamespace.GetMessagingNamespaces
                (configurationOpenedWithoutUserFile, writeToLog);

            // Add a namespace section existing in the application config that should end up in the user config 

            //configurationOpenedWithoutUserFile.AddEntryToDictionarySection(KeySaltWaterFishesWhichWillBeMerged,
            //    "Atlantic mackerel", "Scomber scombrus");

            //// Add an existing entry to a section existing in the application config that 
            //// should end up in the user config. 
            //configurationOpenedWithoutUserFile.AddEntryToDictionarySection(KeySaltWaterFishesWhichWillBeMerged,
            //    "Alaska Pollock", ValueAlaskaPollockNewName);

            //foreach (var freshWaterFish in freshWaterFishes)
            //{
            //    configurationOpenedWithoutUserFile.AddEntryToDictionarySection
            //        (KeyFreshWaterFishesWhichWillOnlyExistInUserConfig, freshWaterFish.Key, freshWaterFish.Value);
            //}

            //// Test reading config values again
            //TestReadingMessagingNamespacesSection(configurationOpenedWithoutUserFile, userFileShouldHaveValues: true);

            //// Persist the configuration
            //configurationOpenedWithoutUserFile.Save();

            //// Create the TwoFilesConfiguration object when a user file exists
            //var configurationOpenedWithUserFile = TwoFilesConfiguration.Create(GetUserSettingsFilePath());

            //// Test reading config values again 
            //TestReadingMessagingNamespacesSection(configurationOpenedWithUserFile, userFileShouldHaveValues: true);
        }

        void SaveConnectionString(TwoFilesConfiguration configuration, int index)
        {
            Assert.IsEmpty(logInMemory);
            ServiceBusNamespace.SaveConnectionString(configuration, fakeConnectionStrings[index].Key, 
                fakeConnectionStrings[index].Value, writeToLog);
            Assert.IsEmpty(logInMemory);
        }

        ConfigurationSection AquireSectionInApplicationConfig(string sectionName)
        {
            //var configurationFileMap = new ExeConfigurationFileMap(userFilePath);
            var localApplicationConfiguration = ConfigurationManager
                .OpenExeConfiguration(ConfigurationUserLevel.None);

            var section = localApplicationConfiguration.GetSection(sectionName);

            if (null == section)
            {
                // Create the section in the user file
                CreateDictionarySectionInApplicationConfigFile(localApplicationConfiguration, sectionName);

                section = localApplicationConfiguration.GetSection(sectionName);
            }

            return section;
        }

        void CreateDictionarySectionInApplicationConfigFile(Configuration configuration, string sectionName)
        {
            var section = configuration.GetSection(sectionName);

            if (null != section)
            {
                return; // Section already exists
            }

            var document = XDocument.Load(configuration.FilePath);

            CreateSectionUsingRawXml(document, sectionName);

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = indent
            };

            using (var writer = XmlWriter.Create(configuration.FilePath, settings))
            {
                document.Save(writer);
            }

            // Refresh the configuration object
            ConfigurationManager.RefreshSection(sectionName);
            //userConfiguration = null;
            //AquireUserConfiguration();
        }

        void WriteToLogInMemory(string message, bool async = true)
        {
            logInMemory += message;
        }

        static void CreateSectionUsingRawXml(XDocument document, string sectionName)
        {
            var configElement = document.AquireElement("configuration", addFirst: true);
            var configSections = configElement.AquireElement("configSections", addFirst: true);

            // Create the section element
            var newSection = new XElement("section",
                    new XAttribute("name", sectionName),
                    new XAttribute("type", "System.Configuration.DictionarySectionHandler, System, " +
                        "Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));

            configSections.Add(newSection);
            configElement.AquireElement(sectionName);
        }

        #endregion

        #region Private instance methods

        void DeleteUserConfigFile()
        {
            DeleteFile(GetUserSettingsFilePath());
        }

        void TestReadingBoolValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
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

        void TestReadingEnumValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
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

        void TestReadingFloatValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
        {
            const string keyOnlyInAppConfig = "morayEelWeight";
            const float mediumNumber = 472.7865f;

            // Get a value that do not exist in the application config file defaulting to empty
            var nonExistingValueAsDefault = configuration.GetFloatValue(keyDoesNotExistAnywhere);
            Assert.IsTrue(NearlyEqual(nonExistingValueAsDefault, 0),
                $"Value read from {nameof(keyDoesNotExistAnywhere)} was " +
                    $"{nonExistingValueAsDefault} instead of 0.");

            // Get a value that do not exist in the application config file defaulting to mediumNumber
            var nonExistingValueAsMediumNumber = configuration.GetFloatValue(keyDoesNotExistAnywhere,
                mediumNumber);
            Assert.IsTrue(NearlyEqual(nonExistingValueAsMediumNumber, mediumNumber),
                $"Value read from {nameof(keyDoesNotExistAnywhere)} was {nonExistingValueAsMediumNumber}" +
                 " instead of {mediumNumber}.");

            // Get the value from the user file defaulting to empty. If userFileShouldHaveValues
            // is false then we should read from the application config and that value should be
            // ValueSharkWeightInAppConfig
            var sharkWeight = configuration.GetFloatValue(KeySharkWeightWhichWillBeOverridden);
            var expectedWeight = userFileShouldHaveValues ?
                ValueSharkWeightInUserConfig : ValueSharkWeightInAppConfig;
            Assert.IsTrue(NearlyEqual(sharkWeight, expectedWeight),
                $"Value read from {nameof(KeySharkWeightWhichWillBeOverridden)} "
                    + $"was {sharkWeight} instead of {expectedWeight}.");

            // Get the value from the user file defaulting to a large number
            sharkWeight = configuration.GetFloatValue(KeySharkWeightWhichWillBeOverridden,
                4789276579f);
            Assert.IsTrue(NearlyEqual(sharkWeight, expectedWeight),
               $"Value read from {nameof(KeySharkWeightWhichWillBeOverridden)} "
                   + $"was {sharkWeight} instead of {expectedWeight}.");

            // Get a value that do not exist in the user file
            const float valueMorayEelWeight = 588f;
            var morayEelWeight = configuration.GetFloatValue(keyOnlyInAppConfig);
            Assert.IsTrue(NearlyEqual(morayEelWeight, valueMorayEelWeight),
                $"Value read from {nameof(keyDoesNotExistAnywhere)} was {morayEelWeight}" +
                    $" instead of {valueMorayEelWeight}.");

            // Get a value that will only exist in the user file
            var onlyInUserFile = configuration.GetFloatValue(KeyWhaleWeightWillExistOnlyInUserConfig,
                mediumNumber);
            expectedWeight = userFileShouldHaveValues ? ValueWhaleWeightInUserConfig : mediumNumber;
            Assert.IsTrue(NearlyEqual(onlyInUserFile, expectedWeight),
                $"Value read from {nameof(KeyWhaleWeightWillExistOnlyInUserConfig)} was " +
                    $"{onlyInUserFile} instead of {expectedWeight}.");
        }

        void TestReadingIntValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
        {
            const string keyOnlyInAppConfig = "morayEelLength";
            const int mediumNumber = 13500;

            // Get a value that do not exist in the application config file defaulting to empty
            var nonExistingValueAsDefault = configuration.GetIntValue(keyDoesNotExistAnywhere);
            Assert.AreEqual(nonExistingValueAsDefault, 0);

            // Get a value that do not exist in the application config file defaulting to mediumNumber
            var nonExistingValueAsMediumNumber = configuration.GetIntValue(keyDoesNotExistAnywhere,
                mediumNumber);
            Assert.AreEqual(mediumNumber, nonExistingValueAsMediumNumber);

            // Get the value from the user file defaulting to empty. If userFileShouldHaveValues
            // is false then we should read from the application config and that value should be
            // ValueSharkLengthInAppConfig
            var sharkLength = configuration.GetIntValue(KeySharkLengthWhichWillBeOverridden);
            var expectedLength = userFileShouldHaveValues ?
                ValueSharkLengthInUserConfig : ValueSharkLengthInAppConfig;
            Assert.AreEqual(expectedLength, sharkLength);

            // Get the value from the user file defaulting to a large number
            sharkLength = configuration.GetIntValue(KeySharkLengthWhichWillBeOverridden, 3450242);
            Assert.AreEqual(expectedLength, sharkLength);

            // Get a value that do not exist in the user file
            const int valueMorayEelLength = 214;
            var morayEelLength = configuration.GetIntValue(keyOnlyInAppConfig);
            Assert.AreEqual(valueMorayEelLength, morayEelLength);

            // Get a value that will only exist in the user file
            var onlyInUserFile = configuration.GetIntValue(KeyWhaleLengthWillExistOnlyInUserConfig,
                mediumNumber);
            expectedLength = userFileShouldHaveValues ? ValueWhaleLengthInUserConfig : mediumNumber;
            Assert.AreEqual(expectedLength, onlyInUserFile);
        }

        void TestReadingStringValues(TwoFilesConfiguration configuration, bool userFileShouldHaveValues)
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

        void TestReadingHashtableSection(TwoFilesConfiguration configuration,
            bool userFileShouldHaveValues)
        {
            // Get a value that do not exist in the application config file defaulting to empty
            var nonExistingSection = configuration.GetHashtableFromSection(keyDoesNotExistAnywhere);
            Assert.AreEqual(nonExistingSection, null);

            // Get the Hashtable of the saltwater fishes. There are three of them in the app config.
            var saltWaterFishes = configuration.GetHashtableFromSection(KeySaltWaterFishesWhichWillBeMerged);

            if (!userFileShouldHaveValues)
            {
                Assert.AreEqual(2, saltWaterFishes.Count);
                Assert.AreEqual(ValueAlaskaPollockOldName, saltWaterFishes["Alaska pollock"]);
            }
            else
            {
                Assert.AreEqual(3, saltWaterFishes.Count);
                Assert.AreEqual(ValueAlaskaPollockNewName, saltWaterFishes["Alaska pollock"]);
                Assert.AreEqual("Scomber scombrus", saltWaterFishes["Atlantic mackerel"]);
            }

            Assert.AreEqual("Scomber colias", saltWaterFishes["Atlantic chub mackerel"]);

            // Read a section that only exists in the user file
            var freshWaterFishes = configuration.GetHashtableFromSection
                (KeyFreshWaterFishesWhichWillOnlyExistInUserConfig);

            if (!userFileShouldHaveValues)
            {
                Assert.AreEqual(null, freshWaterFishes);
            }
            else
            {
                Assert.AreEqual(3, freshWaterFishes.Count);
                Assert.AreEqual("Perca flavescens", freshWaterFishes["Perch"]);
                Assert.AreEqual("Sander lucioperca", freshWaterFishes["Zander"]);
                Assert.AreEqual("Esox lucius", freshWaterFishes["Pike"]);
            }
        }

        #endregion

        #region Private static methods

        static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        static string GetUserSettingsFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                TestDirectoryName,
                "UserSettings.config");
        }

        // From https://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp
        static bool NearlyEqual(double a, double b, double epsilon = 0.0000f)
        {
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a == b)
            {
                // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < Double.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < epsilon;
            }
            else
            {
                // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        #endregion
    }
}
