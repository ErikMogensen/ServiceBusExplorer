#region Copyright
//=======================================================================================
// Microsoft Azure Customer Advisory Team 
//
// This sample is supplemental to the technical guidance published on my personal
// blog at http://blogs.msdn.com/b/paolos/. 
// 
// Author: Paolo Salvatori
//=======================================================================================
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// LICENSED UNDER THE APACHE LICENSE, VERSION 2.0 (THE "LICENSE"); YOU MAY NOT USE THESE 
// FILES EXCEPT IN COMPLIANCE WITH THE LICENSE. YOU MAY OBTAIN A COPY OF THE LICENSE AT 
// http://www.apache.org/licenses/LICENSE-2.0
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING, SOFTWARE DISTRIBUTED UNDER THE 
// LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY 
// KIND, EITHER EXPRESS OR IMPLIED. SEE THE LICENSE FOR THE SPECIFIC LANGUAGE GOVERNING 
// PERMISSIONS AND LIMITATIONS UNDER THE LICENSE.
//=======================================================================================
#endregion

#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.ServiceBus.Messaging;

#endregion

namespace Microsoft.Azure.ServiceBusExplorer.Helpers
{
    // This class is not thread safe since it relies on the Configuration class which is not thread safe when writing
    class TwoFilesConfiguration
    {
        #region Constants

        #endregion

        #region Private fields
        string userConfigFilePath;

        Configuration applicationConfiguration;
        Configuration userConfiguration;
        #endregion

        #region Private constructor

        private TwoFilesConfiguration(Configuration applicationConfiguration, string userConfigFilePath,
            Configuration userConfiguration)
        {
            this.applicationConfiguration = applicationConfiguration;
            this.userConfigFilePath = userConfigFilePath;
            this.userConfiguration = userConfiguration;
        }

        #endregion

        #region Static Create methods - different accessability
        /// <summary>
        /// This method is meant to only be called for unit testing, to avoid polluting 
        /// neither the application config file for the executable running the unit 
        /// tests nor the user config file.
        /// </summary>
        internal static TwoFilesConfiguration Create(string userConfigFilePath)
        {
            var applicationConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            return TwoFilesConfiguration.CreateConfiguration(applicationConfiguration, userConfigFilePath);
        }

        internal static TwoFilesConfiguration Create()
        {
            var localApplicationConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var userConfigFilePath = GetUserSettingsFilePath();

            return TwoFilesConfiguration.CreateConfiguration(localApplicationConfiguration, userConfigFilePath);
        }

        private static TwoFilesConfiguration CreateConfiguration(Configuration applicationConfiguration, string userConfigFilePath)
        {
            if (File.Exists(userConfigFilePath))
            {
                Configuration userConfiguration = OpenConfiguration(userConfigFilePath);
                return new TwoFilesConfiguration(applicationConfiguration, userConfigFilePath,
                    userConfiguration);
            }

            return new TwoFilesConfiguration(applicationConfiguration, userConfigFilePath, null);
        }
        #endregion

        #region Public methods
        public string GetStringValue(string AppSettingKey, string defaultValue = "")
        {
            string result = null;

            if (userConfiguration != null)
            {
                result = userConfiguration.AppSettings.Settings[AppSettingKey]?.Value;
            }

            if (string.IsNullOrEmpty(result))
            {
                result = applicationConfiguration.AppSettings.Settings[AppSettingKey]?.Value;
            }

            if (string.IsNullOrEmpty(result))
            {
                result = defaultValue;
            }

            return result;
        }

        public bool GetBoolValue(string AppSettingKey, bool defaultValue,
            WriteToLogDelegate writeToLogDelegate = null)
        {
            if (userConfiguration != null)
            {
                string resultStringUser = userConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

                if (bool.TryParse(resultStringUser, out var result))
                {
                    return result;
                }
                else
                {
                    WriteParsingFailure(writeToLogDelegate, userConfigFilePath,
                        AppSettingKey, resultStringUser);
                }
            }

            string resultStringApp = applicationConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

            if (!string.IsNullOrWhiteSpace(resultStringApp))
            {
                if (bool.TryParse(resultStringApp, out var result))
                {
                    return result;
                }
                else
                {
                    WriteParsingFailure(writeToLogDelegate, userConfigFilePath,
                        AppSettingKey, resultStringApp);
                }
            }

            return defaultValue;
        }

        public T GetEnumValue<T>(string AppSettingKey, T defaultValue = default) where T : struct
        {
            if (userConfiguration != null)
            {
                string resultStringUser = userConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

                if (Enum.TryParse<T>(resultStringUser, out var result))
                {
                    return result;
                }

                // TODO Add handling of unparsed
            }

            string resultStringApp = applicationConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

            if (!string.IsNullOrWhiteSpace(resultStringApp))
            {
                if (Enum.TryParse<T>(resultStringApp, out var result))
                {
                    return result;
                }

                // TODO Add handling of unparsed
            }

            return defaultValue;
        }

        public float GetFloatValue(string AppSettingKey, float defaultValue = default)
        {
            if (userConfiguration != null)
            {
                string resultStringUser = userConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

                if (float.TryParse(resultStringUser, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }

                // TODO Add handling of unparsed
            }

            string resultStringApp = applicationConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

            if (!string.IsNullOrWhiteSpace(resultStringApp))
            {
                if (float.TryParse(resultStringApp, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }

                // TODO Add handling of unparsed
            }

            return defaultValue;
        }

        public int GetIntValue(string AppSettingKey, int defaultValue = default)
        {
            if (userConfiguration != null)
            {
                string resultStringUser = userConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

                if (int.TryParse(resultStringUser, out var result))
                {
                    return result;
                }

                // TODO Add handling of unparsed
            }

            string resultStringApp = applicationConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

            if (!string.IsNullOrWhiteSpace(resultStringApp))
            {
                if (int.TryParse(resultStringApp, out var result))
                {
                    return result;
                }

                // TODO Add handling of unparsed
            }

            return defaultValue;
        }

        public ConfigurationSectionCollection Sections
        {
            get
            {
                var sectionCollection = applicationConfiguration.Sections;
                var userCollection = userConfiguration.Sections;

                int i = 0;
                var enumerator = userCollection.GetEnumerator();
                foreach(var section in userCollection)
                {
                    sectionCollection.Add((++i).ToString(), section);
                }

                return sectionCollection;
            }

        }

        public void SetValue<T>(string AppSettingKey, T value)
        {
            AquireUserConfiguration();

            if (value is string)
            {
                SetValueInUserConfiguration(AppSettingKey, value as string);
            }
            else
            {
                var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                SetValueInUserConfiguration(AppSettingKey, stringValue);
            }
        }

        public void Save()
        {
            // We are only making changes to the user configuration
            userConfiguration.Save();
        }
        #endregion

        #region Private methods
        private void AquireUserConfiguration()
        {
            if (userConfiguration == null)
            {
                EnsureUserFileExists();
                userConfiguration = OpenConfiguration(userConfigFilePath);
            }
        }

        private void EnsureUserFileExists()
        {
            if (!File.Exists(userConfigFilePath))
            {
                // Make sure the directory exists
                var userConfigDirectory = Path.GetDirectoryName(userConfigFilePath);
                Directory.CreateDirectory(userConfigDirectory);

                // Create the config file 
                var rootElement = new XElement("configuration");
            
                rootElement.Add(new XElement("appSettings"));

                // Create the serviceBusNamespaces section
                var section = new XElement("section",
                        new XAttribute("name", "serviceBusNamespaces"),
                        new XAttribute("type", "System.Configuration.DictionarySectionHandler, System, " +
                            "Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));

                XElement configSections = new XElement("configSections"); 
                configSections.Add(section);
                rootElement.AddFirst(configSections);

                var document = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    rootElement);

                document.Save(userConfigFilePath);
            }
        }

        private static Configuration OpenConfiguration(string userFilePath)
        {
            Configuration userConfiguration;
            //var configurationFileMap = new ExeConfigurationFileMap(userFilePath);

            var exeConfigurationFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = userFilePath
            };

            userConfiguration = ConfigurationManager.OpenMappedExeConfiguration(exeConfigurationFileMap,
                ConfigurationUserLevel.None); //, preLoad: true);
            return userConfiguration;
        }

        private static string GetUserSettingsFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Service Bus Explorer", "UserSettings.config");
        }

        private void SetValueInUserConfiguration(string AppSettingKey, string stringValue)
        {
            if (userConfiguration.AppSettings.Settings[AppSettingKey] == null)
            {
                userConfiguration.AppSettings.Settings.Add(AppSettingKey, stringValue);
            }
            else
            {
                userConfiguration.AppSettings.Settings[AppSettingKey].Value = stringValue;
            }
        }

        private void WriteParsingFailure(WriteToLogDelegate writeToLogDelegate, string configFile,
            string appSettingsKey, string value)
        {
            if (null != writeToLogDelegate)
            {
                writeToLogDelegate($"The configuration file {configFile} has a setting, {appSettingsKey}" +
                    $" which has the invalid value: {value}. It has been ignored.");
            }
        }
        #endregion
    }
}
