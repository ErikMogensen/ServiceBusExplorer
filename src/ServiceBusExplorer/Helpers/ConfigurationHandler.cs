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
using Microsoft.ServiceBus.Messaging;

#endregion

namespace Microsoft.Azure.ServiceBusExplorer.Helpers
{
    class ConfigurationHandler
    {
        #region Constants

        //***************************
        // Constants
        //***************************
        private const string ServiceBusNamespaces = "serviceBusNamespaces";

        //***************************
        // Messages
        //***************************
        private const string ServiceBusNamespacesNotConfigured = "Service bus accounts have not been properly configured in the configuration file.";


        #endregion

        #region Private fields
        static string userFilePath;
        #endregion

        #region Static constructor
        static ConfigurationHandler()
        {
            
        }
        #endregion

        #region Public methods
        static public void GetMessagingNamespacesFromConfiguration(ServiceBusHelper serviceBusHelper,
            WriteToLogDelegate writeToLog)
        {
            if (serviceBusHelper == null)
            {
                return;
            }
            var hashtable = ConfigurationManager.GetSection(ServiceBusNamespaces) as Hashtable;

            if (hashtable == null || hashtable.Count == 0)
            {
                writeToLog(ServiceBusNamespacesNotConfigured);
            }
            serviceBusHelper.ServiceBusNamespaces = new Dictionary<string, ServiceBusNamespace>();
            if (hashtable == null)
            {
                return;
            }
            var e = hashtable.GetEnumerator();

            while (e.MoveNext())
            {
                if (!(e.Key is string) || !(e.Value is string))
                {
                    continue;
                }
                var serviceBusNamespace = ServiceBusNamespace.GetServiceBusNamespace((string)e.Key, (string)e.Value, writeToLog);
                if (serviceBusNamespace != null)
                {
                    serviceBusHelper.ServiceBusNamespaces.Add((string)e.Key, serviceBusNamespace);
                }
            }
            var microsoftServiceBusConnectionString = ConfigurationManager.AppSettings[ConfigurationParameters.MicrosoftServiceBusConnectionString];
            if (!string.IsNullOrWhiteSpace(microsoftServiceBusConnectionString))
            {
                var serviceBusNamespace = ServiceBusNamespace.GetServiceBusNamespace(ConfigurationParameters.MicrosoftServiceBusConnectionString, microsoftServiceBusConnectionString, writeToLog);
                if (serviceBusNamespace != null)
                {
                    serviceBusHelper.ServiceBusNamespaces.Add(ConfigurationParameters.MicrosoftServiceBusConnectionString, serviceBusNamespace);
                }
            }
        }

        static public void SaveConnectionString(string key, string value, 
            WriteToLogDelegate staticWriteToLog)
        {
            // Check where it should be saved TODO
            var userConfig = false;
            SaveConnectionString(userConfig, key, value, staticWriteToLog);
        }

        static public TwoFilesConfiguration GetConfiguration()
        {
            return TwoFilesConfiguration.Create();
        }
        #endregion

        #region Private methods
        static private void EnsureUserFileExists()
        {

        }

        static private void SaveConnectionString(bool userConfig, string key, string value, WriteToLogDelegate staticWriteToLog)
        {
            Configuration configuration;
            ConfigurationSection configurationSection;

            if (userConfig)
            {
                configuration = null;
                configurationSection = null;
            }
            else
            {
                configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configurationSection = configuration.Sections[ServiceBusNamespaces];
            }

            var directory = Path.GetDirectoryName(configuration.FilePath);

            if (string.IsNullOrEmpty(directory))
            {
                staticWriteToLog("The directory of the configuration file cannot be null.");
                return;
            }

            configurationSection.SectionInformation.ForceSave = true;
           
            UpdateConfigFile(configuration, configurationSection, key, value);

            if (!userConfig)
            {
                var appConfig = Path.Combine(directory, "..\\..\\App.config");

                if (File.Exists(appConfig))
                {
                    var exeConfigurationFileMap = new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = appConfig
                    };

                    configuration = ConfigurationManager.OpenMappedExeConfiguration(exeConfigurationFileMap, 
                        ConfigurationUserLevel.None);
                    configurationSection = configuration.Sections[ServiceBusNamespaces];
                    configurationSection.SectionInformation.ForceSave = true;

                    UpdateConfigFile(configuration, configurationSection, key, value);
                }
            }
        }

        private static void UpdateConfigFile(Configuration configuration,
            ConfigurationSection configurationSection, string key, string value)
        {
            var xml = configurationSection.SectionInformation.GetRawXml();
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlElement node = xmlDocument.CreateElement("add");
            node.SetAttribute("key", key);
            node.SetAttribute("value", value);
            xmlDocument.DocumentElement?.AppendChild(node);
            configurationSection.SectionInformation.SetRawXml(xmlDocument.OuterXml);
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("serviceBusNamespaces");
        }

        #endregion
    }
}
