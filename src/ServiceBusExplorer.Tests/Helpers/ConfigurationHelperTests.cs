#region Using Directives

using System;
using System.IO;
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
        public void CreateFakeApplicationConfigFile()
        {

        }

        [Test]
        public void GetValuesFromUserFile()
        {
            var guid = new Guid("2E9DB8C4-8803-4BD7-B860-8932CF13835E");
            var convertedGuid = ConversionHelper.MapStringTypeToCLRType("Guid", guid);
            Assert.AreEqual(guid, convertedGuid);
        }

        [TearDown]
        public void DeleteFakeApplicationConfigFile()
        {

        }
    }
}
