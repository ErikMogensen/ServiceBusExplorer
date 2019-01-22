﻿#region Copyright
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.ServiceBusExplorer.Helpers
{
    public static class ConfigurationHelper
    {
        static readonly string SERVICEBUS_SECTION_NAME = "serviceBusNamespaces";

        static readonly List<string> entities = new List<string> { Constants.QueueEntities, Constants.TopicEntities,
            Constants.EventHubEntities, Constants.NotificationHubEntities, Constants.RelayEntities };

        #region Public methods

        public static void UpdateServiceBusNamespace(ConfigFileUse configFileUse, string key, string newKey, string newValue,
            WriteToLogDelegate writeToLog)
        {
            var configuration = TwoFilesConfiguration.Create(configFileUse, writeToLog);

            configuration.UpdateEntryInDictionarySection(SERVICEBUS_SECTION_NAME, key, newKey, newValue, writeToLog);
        }

        public static void AddServiceBusNamespace(ConfigFileUse configFileUse, string key, string value, WriteToLogDelegate writeToLog)
        {
            var configuration = TwoFilesConfiguration.Create(configFileUse, writeToLog);

            configuration.AddEntryToDictionarySection(SERVICEBUS_SECTION_NAME, key, value);
        }

        public static void RemoveServiceBusNamespace(ConfigFileUse configFileUse, string key, WriteToLogDelegate writeToLog)
        {
            var configuration = TwoFilesConfiguration.Create(configFileUse, writeToLog);

            configuration.RemoveEntryFromDictionarySection(SERVICEBUS_SECTION_NAME, key, writeToLog);
        }

        public static MainSettings GetMainProperties(ConfigFileUse configFileUse,
            MainSettings currentSettings, WriteToLogDelegate writeToLog)
        {
            var configuration = TwoFilesConfiguration.Create(configFileUse, writeToLog);

            return GetMainSettingsUsingConfiguration(configuration, currentSettings, writeToLog);
        }

        #endregion

        #region Public static properties
        public static List<string> Entities
        {
            get
            {
                return entities;
            }
        }
        #endregion

        #region Private static methods
        static List<string> GetEntitiesAsList(string parameter)
        {
            return parameter.Split(',').Select(item => item.Trim()).ToList();
        }

        static List<string> GetSelectedEntities(TwoFilesConfiguration configuration)
        {
            var selectedEntities = new List<string>();
            var parameter = configuration.GetStringValue(ConfigurationParameters.SelectedEntitiesParameter);

            if (!string.IsNullOrEmpty(parameter))
            {
                List<string> items = GetEntitiesAsList(parameter);
                if (items.Count == 0)
                {
                    GetDefaultSelectedEntities(selectedEntities, entities);
                }
                else
                {
                    foreach (var item in items)
                    {
                        if (entities.Contains(item, StringComparer.OrdinalIgnoreCase))
                        {
                            selectedEntities.Add(item);
                        }
                    }
                }
            }
            else
            {
                GetDefaultSelectedEntities(selectedEntities, entities);
            }

            return selectedEntities;
        }

        static void GetDefaultSelectedEntities(List<string> selectedEntities, List<string> entities)
        {
            selectedEntities.AddRange(entities);
        }

        static MainSettings GetMainSettingsUsingConfiguration(TwoFilesConfiguration configuration,
            MainSettings currentSettings, WriteToLogDelegate writeToLog)
        {
            var resultProperties = new MainSettings();

            resultProperties.LogFontSize = configuration.GetDecimalValue(ConfigurationParameters.LogFontSize,
                currentSettings.LogFontSize, writeToLog);

            resultProperties.TreeViewFontSize = configuration.GetDecimalValue(ConfigurationParameters.TreeViewFontSize,
                currentSettings.TreeViewFontSize, writeToLog);

            resultProperties.RetryCount = configuration.GetIntValue(ConfigurationParameters.RetryCountParameter,
                currentSettings.RetryCount, writeToLog);

            resultProperties.RetryTimeout = configuration.GetIntValue(ConfigurationParameters.RetryTimeoutParameter,
                currentSettings.RetryTimeout, writeToLog);

            resultProperties.ReceiveTimeout = configuration.GetIntValue(ConfigurationParameters.ReceiveTimeoutParameter,
                currentSettings.ReceiveTimeout, writeToLog);

            resultProperties.ServerTimeout = configuration.GetIntValue(ConfigurationParameters.ServerTimeoutParameter,
                currentSettings.ServerTimeout, writeToLog);

            resultProperties.PrefetchCount = configuration.GetIntValue(ConfigurationParameters.PrefetchCountParameter,
                currentSettings.PrefetchCount, writeToLog);

            resultProperties.TopCount = configuration.GetIntValue(ConfigurationParameters.TopParameter, 
                currentSettings.TopCount, writeToLog);

            resultProperties.SenderThinkTime = configuration.GetIntValue
                (ConfigurationParameters.SenderThinkTimeParameter, currentSettings.SenderThinkTime, writeToLog);

            resultProperties.ReceiverThinkTime = configuration.GetIntValue
                (ConfigurationParameters.ReceiverThinkTimeParameter, currentSettings.ReceiverThinkTime, writeToLog);

            resultProperties.MonitorRefreshInterval = configuration.GetIntValue
                (ConfigurationParameters.MonitorRefreshIntervalParameter, 
                currentSettings.MonitorRefreshInterval, writeToLog);

            resultProperties.ShowMessageCount = configuration.GetBoolValue
                (ConfigurationParameters.ShowMessageCountParameter,
                currentSettings.ShowMessageCount, writeToLog);

            resultProperties.UseAscii = configuration.GetBoolValue(ConfigurationParameters.UseAsciiParameter,
                currentSettings.UseAscii, writeToLog);

            resultProperties.SaveMessageToFile = configuration.GetBoolValue
                (ConfigurationParameters.SaveMessageToFileParameter, currentSettings.SaveMessageToFile, writeToLog);

            resultProperties.SavePropertiesToFile = configuration.GetBoolValue
                (ConfigurationParameters.SavePropertiesToFileParameter,
                currentSettings.SavePropertiesToFile, writeToLog);

            resultProperties.SaveCheckpointsToFile = configuration.GetBoolValue
                (ConfigurationParameters.SaveCheckpointsToFileParameter,
                currentSettings.SaveCheckpointsToFile, writeToLog);

            resultProperties.Label = configuration.GetStringValue(ConfigurationParameters.LabelParameter, 
                MainSettings.DefaultLabel);

            MessageAndPropertiesHelper.GetMessageTextAndFile(configuration,
                out string messageText, out string messageFile);
            resultProperties.MessageText = messageText;
            resultProperties.MessageFile = messageFile;

            resultProperties.SelectedEntities = ConfigurationHelper.GetSelectedEntities(configuration);

            resultProperties.MessageBodyType = configuration.GetStringValue(ConfigurationParameters.MessageBodyType,
                BodyType.Stream.ToString());

            resultProperties.ConnectivityMode = configuration.GetEnumValue
                (ConfigurationParameters.ConnectivityMode, currentSettings.ConnectivityMode, writeToLog);

            resultProperties.EncodingType = configuration.GetEnumValue
                (ConfigurationParameters.Encoding, currentSettings.EncodingType, writeToLog);

            return resultProperties;
        }
        #endregion
    }
}
