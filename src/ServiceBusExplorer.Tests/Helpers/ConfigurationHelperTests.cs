
using NUnit.Framework;
using ServiceBusExplorer.Helpers;
using ServiceBusExplorer.Utilities.Helpers;
using System.Collections;
using System.Collections.Generic;

namespace ServiceBusExplorer.Tests.Helpers
{
    [TestFixture]
    public class ConfigurationHelperTests
    {
        private TwoFilesConfiguration _mockConfiguration;
        private WriteToLogDelegate _mockWriteToLog;

        [SetUp]
        public void SetUp()
        {
            _mockConfiguration = new Mock<TwoFilesConfiguration>().Object;
            _mockWriteToLog = new Mock<WriteToLogDelegate>().Object;
        }

        [Test]
        public void SuperUpsertMessagingNamespace_ShouldUpsertCorrectly()
        {
            // Arrange
            var configFileUse = ConfigFileUse.Local;
            var serviceTypeName = Constants.ServiceBusServiceType;
            var oldKey = "oldKey";
            var newKey = "newKey";
            var newValue = "newValue";

            // Act
            ConfigurationHelper.SuperUpsertMessagingNamespace(configFileUse, serviceTypeName, oldKey, newKey, newValue, _mockWriteToLog);

            // Assert
            // Add assertions to verify the behavior
        }

        [Test]
        public void UpdateMessagingNamespace_ShouldUpdateCorrectly()
        {
            // Arrange
            var serviceTypeName = Constants.ServiceBusServiceType;
            var key = "key";
            var newKey = "newKey";
            var newValue = "newValue";

            // Act
            ConfigurationHelper.UpdateMessagingNamespace(_mockConfiguration, serviceTypeName, key, newKey, newValue, _mockWriteToLog);

            // Assert
            // Add assertions to verify the behavior
        }

        [Test]
        public void AddMessagingNamespace_ShouldAddCorrectly()
        {
            // Arrange
            var serviceTypeName = Constants.ServiceBusServiceType;
            var key = "key";
            var value = "value";

            // Act
            ConfigurationHelper.AddMessagingNamespace(_mockConfiguration, serviceTypeName, key, value, _mockWriteToLog);

            // Assert
            // Add assertions to verify the behavior
        }

        [Test]
        public void RemoveMessagingNamespace_ShouldRemoveCorrectly()
        {
            // Arrange
            var serviceTypeName = Constants.ServiceBusServiceType;
            var key = "key";

            // Act
            ConfigurationHelper.RemoveMessagingNamespace(_mockConfiguration, serviceTypeName, key, _mockWriteToLog);

            // Assert
            // Add assertions to verify the behavior
        }

        [Test]
        public void GetMainProperties_ShouldReturnCorrectProperties()
        {
            // Arrange
            var configFileUse = ConfigFileUse.Local;
            var currentSettings = new MainSettings();

            // Act
            var result = ConfigurationHelper.GetMainProperties(configFileUse, currentSettings, _mockWriteToLog);

            // Assert
            Assert.IsNotNull(result);
            // Add more assertions to verify the properties
        }

        [Test]
        public void GetNamespacesForServiceType_ShouldReturnCorrectNamespaces()
        {
            // Arrange
            var serviceType = ServiceType.ServiceBus;

            // Act
            var result = ConfigurationHelper.GetNamespacesForServiceType(_mockConfiguration, serviceType);

            // Assert
            Assert.IsInstanceOf<Hashtable>(result);
            // Add more assertions to verify the contents of the hashtable
        }
    }
}
