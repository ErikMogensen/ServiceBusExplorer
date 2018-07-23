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
        Configuration applicationConfiguration;
        Configuration userConfiguration;
        #endregion

        #region The only constructor
        public TwoFilesConfiguration(Configuration localApplicationConfiguration, Configuration localUserConfiguration)
        {
            applicationConfiguration = localApplicationConfiguration;
            userConfiguration = localUserConfiguration;
        }
        #endregion

        #region Static Create methods - different accessability
        /// <summary>
        /// This method is meant to only be called for unit testing, to avoid polluting the application config
        /// file for the executable running the unit tests and the user config file.
        /// </summary>
        internal static TwoFilesConfiguration Create(string applicationConfigPath, string userConfigFilePath)
        {
            var localApplicationConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            return TwoFilesConfiguration.Create(localApplicationConfiguration, userConfigFilePath);
        }
        internal static TwoFilesConfiguration Create()
        {
            var localApplicationConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var userConfigFilePath = GetUserSettingsFilePath();

            return TwoFilesConfiguration.Create(localApplicationConfiguration, userConfigFilePath);
        }

        private static TwoFilesConfiguration Create(Configuration applicationConfiguration, string userConfigFilePath)
        {
            Configuration localUserConfiguration = null;

            if (File.Exists(userConfigFilePath))
            {
                localUserConfiguration = OpenConfiguration(userConfigFilePath);
            }

            return new TwoFilesConfiguration(applicationConfiguration, localUserConfiguration);
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
                string resultStringUser = userConfiguration.AppSettings.Settings[AppSettingKey].Value;

                if (!string.IsNullOrWhiteSpace(resultStringUser))
                {
                    return bool.Parse(resultStringUser);
                }
            }


            string resultStringApp = applicationConfiguration.AppSettings.Settings[AppSettingKey].Value;

            if (!string.IsNullOrWhiteSpace(resultStringApp))
            {
                return bool.Parse(resultStringApp);
            }

            return defaultValue;
        }

        public void SetValue<T>(string AppSettingKey, T value)
        {
            AquireUserConfiguration();

            if (userConfiguration.AppSettings.Settings[AppSettingKey]==null)
            {
                userConfiguration.AppSettings.Settings.Add(AppSettingKey, value.ToString());
            }
            else
            {
                userConfiguration.AppSettings.Settings[AppSettingKey].Value = value.ToString();
            }
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

        }
        #endregion

        #region Private methods
        private void AquireUserConfiguration()
        {
            if (userConfiguration == null)
            {
                EnsureUserFileExists();
                userConfiguration = OpenConfiguration(GetUserSettingsFilePath());
            }
        }

        private void EnsureUserFileExists()
        {
            if (!File.Exists(GetUserSettingsFilePath()))
            {
                // Create the config file 
                var rootElement = new XElement("Configuration");
                rootElement.Add(new XElement("AppSettings"));
                var document = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    rootElement);

                document.Save(GetUserSettingsFilePath());
            }
        }


        private static Configuration OpenConfiguration(string userFilePath)
        {
            Configuration localUserConfiguration;
            var configurationFileMap = new ExeConfigurationFileMap(userFilePath);
            localUserConfiguration = ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap,
                ConfigurationUserLevel.None, preLoad: true);
            return localUserConfiguration;
        }

        private static string GetUserSettingsFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Service Bus Explorer", "UserSettings.config");
        }

        #endregion
    }
}
