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
        private static string GetFakeApplicationConfigFilePath()
        {
            // Put it in the users non roaming personal folder
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
     "Service Bus Explorer", "UserSettings.config");

        }

        [SetUp]
        public void CreateNewFakeApplicationConfigFile()
        {
            // Delete the old one if it exists
            DeleteFakeApplicationConfigFile();

            // Copy the config file from the build
            var thisAssembly = Assembly.GetExecutingAssembly();
            var sbeConfigpath = Path.Combine(thisAssembly.CodeBase, 
                @"..\..\ServiceBusExplorer\bin\release",
                "ServiceBusExplorer.exe");
            File.Copy(sbeConfigpath, GetFakeApplicationConfigFilePath());
        }

        [TearDown]
        public void DeleteFakeApplicationConfigFile()
        {
            var fakeFile = GetFakeApplicationConfigFilePath();

            if (File.Exists(fakeFile))
            {
                File.Delete(fakeFile);
            }
        }

        [Test]
        public void GetValuesFromUserFile()
        {
            TwoFilesConfiguration configuration = ConfigurationHandler.GetConfiguration();

            // Initially the user
            var onlyInUserFile = configuration.GetBoolValue("OnlyInUserFile", false);
            Assert.AreEqual(onlyInUserFile, false);
        }

        [Test]
        public void GetValuesFromUserFile()
        {

        }
    }
