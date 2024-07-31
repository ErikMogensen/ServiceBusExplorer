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

#region Using Directives

using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Text.RegularExpressions;

using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using ServiceBusExplorer.Utilities.Helpers;

#endregion

namespace ServiceBusExplorer.Helpers
{
    public enum HostType
    {
        Custom,
        Cloud,
        OnPremises
    }

    /// <summary>
    /// This class represents a service bus namespace address and authentication credentials
    /// </summary>
    public class MessagingNamespace
    {
        #region Private Constants

        //***************************
        // Messages
        //***************************
        const string ServiceBusNamespacesNotConfigured = "Service bus accounts have not been properly configured in the configuration file.";
        const string ServiceBusNamespaceIsNullOrEmpty = "The connection string for service bus entry {0} is null or empty.";
        const string ServiceBusNamespaceIsWrong = "The connection string for service bus namespace {0} is in the wrong format.";
        const string ServiceBusNamespaceEndpointIsNullOrEmpty = "The endpoint for the service bus namespace {0} is null or empty.";
        const string ServiceBusNamespaceEndpointPrefixedWithSb = "The endpoint for the service bus namespace {0} is being automatically prefixed with \"sb://\".";
        const string ServiceBusNamespaceStsEndpointIsNullOrEmpty = "The sts endpoint for the service bus namespace {0} is null or empty.";
        const string ServiceBusNamespaceRuntimePortIsNullOrEmpty = "The runtime port for the service bus namespace {0} is null or empty.";
        const string ServiceBusNamespaceManagementPortIsNullOrEmpty = "The management port for the service bus namespace {0} is null or empty.";
        const string ServiceBusNamespaceEndpointUriIsInvalid = "The endpoint URI for the service bus namespace {0} is invalid.";
        const string ServiceBusNamespaceSharedAccessKeyNameIsInvalid = "The SharedAccessKeyName for the service bus namespace {0} is invalid.";
        const string ServiceBusNamespaceSharedAccessKeyIsInvalid = "The SharedAccessKey for the service bus namespace {0} is invalid.";

        //***************************
        // Parameters
        //***************************
        const string ConnectionStringEndpoint = "endpoint";
        const string ConnectionStringSharedAccessKeyName = "sharedaccesskeyname";
        const string ConnectionStringSharedAccessKey = "sharedaccesskey";
        const string ConnectionStringStsEndpoint = "stsendpoint";
        const string ConnectionStringRuntimePort = "runtimeport";
        const string ConnectionStringManagementPort = "managementport";
        const string ConnectionStringWindowsUsername = "windowsusername";
        const string ConnectionStringWindowsDomain = "windowsdomain";
        const string ConnectionStringWindowsPassword = "windowspassword";
        const string ConnectionStringTransportType = "transporttype";
        const string ConnectionStringEntityPath = "entitypath";

        #endregion

        #region Public Constants
        //***************************
        // Formats
        //***************************
        public const string SasConnectionStringFormat = "Endpoint={0};SharedAccessKeyName={1};SharedAccessKey={2};TransportType={3}";
        public const string SasConnectionStringEntityPathFormat = "Endpoint={0};SharedAccessKeyName={1};SharedAccessKey={2};TransportType={3};EntityPath={4}";
        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the MessagingNamespace class.
        /// </summary>
        /// <param name="connectionStringType">The service bus namespace connection string type.</param>
        /// <param name="connectionString">The service bus namespace connection string.</param>
        /// <param name="uri">The full address of the service bus namespace.</param>
        /// <param name="ns">The service bus namespace.</param>
        /// <param name="name">The issuer name of the shared secret credentials.</param>
        /// <param name="key">The issuer secret of the shared secret credentials.</param>
        /// <param name="servicePath">The service path that follows the host name section of the URI.</param>
        /// <param name="stsEndpoint">The sts endpoint of the service bus namespace.</param>
        /// <param name="transportType">The transport type to use to access the namespace.</param>
        /// <param name="isSas">True is is SAS connection string, false otherwise.</param>
        /// <param name="entityPath">Entity path connection string scoped to. Otherwise a default.</param>
        public MessagingNamespace(
            ServiceType serviceType,
            HostType connectionStringType,
            string connectionString,
            string uri,
            string ns,
            string servicePath,
            string name,
            string key,
            string stsEndpoint,
            TransportType transportType,
            bool isSas = false,
            string entityPath = "",
            bool isUserCreated = false)
        {
            ServiceType = serviceType;
            ConnectionStringType = connectionStringType;
            Uri = string.IsNullOrWhiteSpace(uri) ?
                  ServiceBusEnvironment.CreateServiceUri("sb", ns, servicePath).ToString() :
                  uri;

            ConnectionString = connectionString;
            Namespace = ns;

            if (isSas)
            {
                SharedAccessKeyName = name;
                SharedAccessKey = key;
            }
            else
            {
                ServicePath = servicePath;
            }

            TransportType = transportType;
            StsEndpoint = stsEndpoint;
            RuntimePort = default(string);
            ManagementPort = default(string);
            EntityPath = entityPath;
            UserCreated = isUserCreated;
        }

        /// <summary>
        /// Initializes a new instance of the MessagingNamespace class for an on-premises namespace.
        /// </summary>
        /// <param name="connectionString">The service bus namespace connection string.</param>
        /// <param name="endpoint">The endpoint of the service bus namespace.</param>
        /// <param name="stsEndpoint">The sts endpoint of the service bus namespace.</param>
        /// <param name="runtimePort">The runtime port.</param>
        /// <param name="managementPort">The management port.</param>
        /// <param name="ns">The service bus namespace.</param>
        /// <param name="transportType">The transport type to use to access the namespace.</param>
        public MessagingNamespace(string connectionString,
                                   string endpoint,
                                   string stsEndpoint,
                                   string runtimePort,
                                   string managementPort,
                                   string ns,
                                   TransportType transportType,
                                   bool isUserCreated = false)
        {
            ConnectionStringType = HostType.OnPremises;
            ConnectionString = connectionString;
            Uri = endpoint;
            var uri = new Uri(endpoint);


            if (string.IsNullOrWhiteSpace(endpoint))
            {
                Uri = ServiceBusEnvironment.CreateServiceUri(uri.Scheme, ns, null).ToString();
            }

            Namespace = ns;
            var settings = new MessagingFactorySettings();
            TransportType = settings.TransportType;
            StsEndpoint = stsEndpoint;
            RuntimePort = runtimePort;
            ManagementPort = managementPort;
            TransportType = transportType;
            UserCreated = isUserCreated;
        }
        #endregion

        #region Public methods
        public static MessagingNamespace Create(ServiceType serviceType, string key, string connectionString,
            WriteToLogDelegate staticWriteToLog)
        {

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                staticWriteToLog(string.Format(CultureInfo.CurrentCulture, ServiceBusNamespaceIsNullOrEmpty, key));
                return null;
            }

            var isUserCreated = !(key == "CustomConnectionString" || key == "SASConnectionString");
            var toLower = connectionString.ToLower();
            var parameters = connectionString.Split(';').ToDictionary(s => s.Substring(0, s.IndexOf('=')).ToLower(), s => s.Substring(s.IndexOf('=') + 1));

            if (toLower.Contains(ConnectionStringEndpoint) &&
                toLower.Contains(ConnectionStringSharedAccessKeyName) &&
                toLower.Contains(ConnectionStringSharedAccessKey))
            {
                return CreateUsingSAS(
                    serviceType,
                    key,
                    connectionString,
                    staticWriteToLog,
                    isUserCreated,
                    parameters);
            }

            return null;
        }

        public static Dictionary<string, MessagingNamespace> GetMessagingNamespaces
            (TwoFilesConfiguration configuration, WriteToLogDelegate writeToLog)
        {
            var messagingNamespaces = new Dictionary<string, MessagingNamespace>();

            foreach (var serviceType in Enum.GetValues(typeof(ServiceType)))
            {
                var namespaces = GetMessagingNamespacesForServiceType(
                    configuration, (ServiceType)serviceType, writeToLog);

                // Add the latest namespaces to the total
                foreach (var messagingNamespace in namespaces)
                {
                    if (!messagingNamespaces.ContainsKey(messagingNamespace.Key))
                    {
                        messagingNamespaces.Add(messagingNamespace.Key, messagingNamespace.Value);
                    }
                }
            }

            var microsoftServiceBusConnectionString =
                configuration.GetStringValue(ConfigurationParameters.MicrosoftServiceBusConnectionString);

            if (!string.IsNullOrWhiteSpace(microsoftServiceBusConnectionString))
            {
                var serviceBusNamespace = MessagingNamespace.Create(
                    ServiceType.ServiceBus,
                    ConfigurationParameters.MicrosoftServiceBusConnectionString,
                    microsoftServiceBusConnectionString,
                    writeToLog);

                if (serviceBusNamespace != null)
                {
                    messagingNamespaces.
                        Add(ConfigurationParameters.MicrosoftServiceBusConnectionString, serviceBusNamespace);
                }
            }

            if (messagingNamespaces == null || messagingNamespaces.Count == 0)
            {
                writeToLog(ServiceBusNamespacesNotConfigured);
            }

            return messagingNamespaces;
        }
        public static Dictionary<string, MessagingNamespace> GetMessagingNamespacesForServiceType
            (TwoFilesConfiguration configuration, ServiceType serviceType, WriteToLogDelegate writeToLog)
        {
            var hashtable = ConfigurationHelper.GetNamespacesForServiceType(configuration, serviceType);

            if (hashtable == null || hashtable.Count == 0)
            {
                writeToLog(ServiceBusNamespacesNotConfigured);
            }

            var messagingNamespaces = new Dictionary<string, MessagingNamespace>();

            if (hashtable == null)
            {
                return messagingNamespaces;
            }

            var e = hashtable.GetEnumerator();

            while (e.MoveNext())
            {
                if (!(e.Key is string) || !(e.Value is string))
                {
                    continue;
                }

                var serviceBusNamespace = MessagingNamespace.Create(serviceType,
                                                                    (string)e.Key,
                                                                    (string)e.Value,
                                                                    writeToLog);

                if (serviceBusNamespace != null)
                {
                    messagingNamespaces.Add((string)e.Key, serviceBusNamespace);
                }
            }

            return messagingNamespaces;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the service bus namespace type.
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the service bus namespace type.
        /// </summary>
        public HostType ConnectionStringType { get; set; }

        /// <summary>
        /// Get or set if this is a connection string added by the user
        /// </summary>
        public bool UserCreated { get; set; }

        /// <summary>
        /// Gets or sets the service bus namespace connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the service bus namespace connection string without transport type.
        /// </summary>
        public string ConnectionStringWithoutTransportType
        {
            get
            {
                var regex = new Regex(@";TransportType=\w*;?");
                var connectionString = regex.Replace(ConnectionString, string.Empty);
                return connectionString;
            }
        }

        /// <summary>
        /// Gets or sets the full address of the service bus namespace.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the service bus namespace.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the service path that follows the host name section of the URI.
        /// </summary>
        public string ServicePath { get; set; }

        /// <summary>
        /// Gets or sets the transport type to use to access the namespace.
        /// </summary>
        public TransportType TransportType { get; set; }

        /// <summary>
        /// Gets or sets the URL of the sts endpoint.
        /// </summary>
        public string StsEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the runtime port.
        /// </summary>
        public string RuntimePort { get; set; }

        /// <summary>
        /// Gets or sets the management port.
        /// </summary>
        public string ManagementPort { get; set; }

        /// <summary>
        /// Gets or sets the SharedAccessKeyName.
        /// </summary>
        public string SharedAccessKeyName { get; set; }

        /// <summary>
        /// Gets or sets the SharedAccessKey.
        /// </summary>
        public string SharedAccessKey { get; set; }

        /// <summary>
        /// Gets or sets the EntityPath
        /// </summary>
        public string EntityPath { get; set; }
        #endregion

        #region Private Methods

        static MessagingNamespace CreateUsingSAS(ServiceType serviceType, string key, string connectionString,
            WriteToLogDelegate staticWriteToLog, bool isUserCreated, Dictionary<string, string> parameters)
        {
            if (parameters.Count < 3)
            {
                staticWriteToLog(string.Format(CultureInfo.CurrentCulture, ServiceBusNamespaceIsWrong, key));
                return null;
            }

            var endpoint = parameters.ContainsKey(ConnectionStringEndpoint) ?
                           parameters[ConnectionStringEndpoint] :
                           null;

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                staticWriteToLog(string.Format(CultureInfo.CurrentCulture,
                    ServiceBusNamespaceEndpointIsNullOrEmpty, key));
                return null;
            }

            if (!endpoint.Contains("://"))
            {
                staticWriteToLog(string.Format(CultureInfo.CurrentCulture,
                    ServiceBusNamespaceEndpointPrefixedWithSb, endpoint));
                endpoint = "sb://" + endpoint;
            }

            var stsEndpoint = parameters.ContainsKey(ConnectionStringStsEndpoint) ?
                              parameters[ConnectionStringStsEndpoint] :
                              null;

            string ns = GetNamespaceNameFromEndpoint(endpoint, staticWriteToLog, key);

            if (!parameters.ContainsKey(ConnectionStringSharedAccessKeyName) ||
                string.IsNullOrWhiteSpace(parameters[ConnectionStringSharedAccessKeyName]))
            {
                staticWriteToLog(string.Format(CultureInfo.CurrentCulture, ServiceBusNamespaceSharedAccessKeyNameIsInvalid, key));
            }

            var sharedAccessKeyName = parameters[ConnectionStringSharedAccessKeyName];

            if (!parameters.ContainsKey(ConnectionStringSharedAccessKey) || string.IsNullOrWhiteSpace(parameters[ConnectionStringSharedAccessKey]))
            {
                staticWriteToLog(string.Format(CultureInfo.CurrentCulture,
                    ServiceBusNamespaceSharedAccessKeyIsInvalid, key));
            }

            var sharedAccessKey = parameters[ConnectionStringSharedAccessKey];
            var settings = new MessagingFactorySettings();
            var transportType = settings.TransportType;

            if (parameters.ContainsKey(ConnectionStringTransportType))
            {
                Enum.TryParse(parameters[ConnectionStringTransportType], true, out transportType);
            }

            string entityPath = string.Empty;

            if (parameters.ContainsKey(ConnectionStringEntityPath))
            {
                entityPath = parameters[ConnectionStringEntityPath];
            }

            return new MessagingNamespace(serviceType, HostType.Cloud, connectionString, endpoint, ns, null,
                sharedAccessKeyName, sharedAccessKey, stsEndpoint, transportType, true,
                entityPath, isUserCreated);
        }

        static string GetNamespaceNameFromEndpoint(string endpoint, WriteToLogDelegate staticWriteToLog,
            string key)
        {
            Uri uri;

            try
            {
                uri = new Uri(endpoint);
            }
            catch (Exception)
            {
                staticWriteToLog(string.Format(CultureInfo.CurrentCulture,
                    ServiceBusNamespaceEndpointUriIsInvalid, key));
                return null;
            }

            return uri.Host.Split('.')[0];
        }

        #endregion
    }
}