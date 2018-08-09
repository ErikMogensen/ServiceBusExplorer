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

        #region Constructors
        public TwoFilesConfiguration(Configuration applicationConfiguration, Configuration userConfiguration)
        {
            this.applicationConfiguration = applicationConfiguration;
            this.userConfiguration = userConfiguration;
        }

        public TwoFilesConfiguration(Configuration applicationConfiguration, string userConfigFilePath)
        {
            this.applicationConfiguration = applicationConfiguration;
            this.userConfigFilePath = userConfigFilePath;
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

            return TwoFilesConfiguration.Create(applicationConfiguration, userConfigFilePath);
        }

        internal static TwoFilesConfiguration Create()
        {
            var localApplicationConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var userConfigFilePath = GetUserSettingsFilePath();

            return TwoFilesConfiguration.Create(localApplicationConfiguration, userConfigFilePath);
        }

        private static TwoFilesConfiguration Create(Configuration applicationConfiguration, string userConfigFilePath)
        {
            if (File.Exists(userConfigFilePath))
            {
                Configuration userConfiguration = OpenConfiguration(userConfigFilePath);
                return new TwoFilesConfiguration(applicationConfiguration, userConfiguration);
            }

            return new TwoFilesConfiguration(applicationConfiguration, userConfigFilePath);
        }
        #endregion

        #region Public methods
        public string GetStringValue(string AppSettingKey)
        {
            string result = null;

            if (userConfiguration != null)
            {
                result = userConfiguration.AppSettings.Settings[AppSettingKey].Value;
            }

            if (result == null)
            {
                result = applicationConfiguration.AppSettings.Settings[AppSettingKey].Value;
            }

            return result;
        }

        public bool GetBoolValue(string AppSettingKey, bool defaultValue)
        {
            if (userConfiguration != null)
            {
                string resultStringUser = userConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

                if (!string.IsNullOrWhiteSpace(resultStringUser))
                {
                    return bool.Parse(resultStringUser);
                }
            }


            string resultStringApp = applicationConfiguration.AppSettings.Settings[AppSettingKey]?.Value;

            if (!string.IsNullOrWhiteSpace(resultStringApp))
            {
                if (bool.TryParse(resultStringApp, out var result))
                {
                    return result;
                }

                // TODO Add handling of unparsed
            }

            return defaultValue;
        }

        public void SetStringValue(string AppSettingKey, string value)
        {
            AquireUserConfiguration();

            if (userConfiguration.AppSettings.Settings[AppSettingKey] == null)
            {
                userConfiguration.AppSettings.Settings.Add(AppSettingKey, value);
            }
            else
            {
                userConfiguration.AppSettings.Settings[AppSettingKey].Value = value;
            }
        }

        public void SetBoolValue(string AppSettingKey, bool value)
        {
            AquireUserConfiguration();

            var stringValue = Convert.ToString(value);

            if (userConfiguration.AppSettings.Settings[AppSettingKey] == null)
            {
                userConfiguration.AppSettings.Settings.Add(AppSettingKey, stringValue);
            }
            else
            {
                userConfiguration.AppSettings.Settings[AppSettingKey].Value = stringValue;
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
                var stringValue = Convert.ToString(value);
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

        #endregion
    }
}
