using System;
using System.Collections;
using System.Collections.Generic;
using Cloud.Core.SecureVault.AzureKeyVault.Config;
using Cloud.Core.Testing;
using FluentAssertions;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cloud.Core.SecureVault.AzureKeyVault.Tests
{
    [IsUnit]
    public class KeyVaultUnitTests
    {
        /// <summary>Check the ISecureVault is added to the service collection when using the new extension method.</summary>
        [Fact]
        public void Test_KeyVault_ServiceCollectionAddKeyVault()
        {
            // Principle needs "Set" permissions to run this.
            IServiceCollection serviceCollection = new FakeServiceCollection();
            serviceCollection.AddKeyVaultSingleton(new MsiConfig { KeyVaultInstanceName = "instance" });

            serviceCollection.Contains(new ServiceDescriptor(typeof(ISecureVault), typeof(KeyVault))).Should().BeTrue();
            serviceCollection.Clear();

            serviceCollection.AddKeyVaultSingleton(new ServicePrincipleConfig { KeyVaultInstanceName = "instance", AppId = "test", AppSecret = "test", TenantId = "test" });
            serviceCollection.Contains(new ServiceDescriptor(typeof(ISecureVault), typeof(KeyVault))).Should().BeTrue();
            serviceCollection.Clear();

            serviceCollection.AddKeyVaultSingleton("instance");
            serviceCollection.Contains(new ServiceDescriptor(typeof(ISecureVault), typeof(KeyVault))).Should().BeTrue();
            serviceCollection.Clear();

            serviceCollection.AddKeyVaultSingletonNamed("key", "instance");
            serviceCollection.Contains(new ServiceDescriptor(typeof(ISecureVault), typeof(KeyVault))).Should().BeTrue();
            serviceCollection.Clear();

            serviceCollection.AddKeyVaultSingletonNamed("key", new ServicePrincipleConfig { KeyVaultInstanceName = "instance", AppId = "test", AppSecret = "test", TenantId = "test" });
            serviceCollection.Contains(new ServiceDescriptor(typeof(ISecureVault), typeof(KeyVault))).Should().BeTrue();
        }

        /// <summary>Check the validate method carries out the validation as expected.</summary>
        [Fact]
        public void Test_KeyVault_ConfigValidate()
        {
            var spConfigGood = new ServicePrincipleConfig { KeyVaultInstanceName = "test", AppSecret = "test", TenantId = "test", AppId = "test" };
            var spConfigBad = new ServicePrincipleConfig();
            var spConfigString = spConfigGood.ToString();

            var msiConfigGood = new MsiConfig { KeyVaultInstanceName = "test" };
            var msiConfigBad = new MsiConfig();
            var msiConfigString = msiConfigGood.ToString();

            AssertExtensions.DoesNotThrow(() => spConfigGood.Validate());
            Assert.Throws<ArgumentException>(() => spConfigBad.Validate());

            spConfigBad.KeyVaultInstanceName = "test";
            Assert.Throws<ArgumentException>(() => spConfigBad.Validate());

            spConfigBad.AppId = "test";
            Assert.Throws<ArgumentException>(() => spConfigBad.Validate());

            spConfigBad.AppSecret = "test";
            Assert.Throws<ArgumentException>(() => spConfigBad.Validate());

            AssertExtensions.DoesNotThrow(() => msiConfigGood.Validate());
            Assert.Throws<ArgumentException>(() => msiConfigBad.Validate());
        }

        /// <summary>Verify the ToString method contains the expected output.</summary>
        [Fact]
        public void Test_ServicePrincipleConfig_ToString()
        {
            var spConfig = new ServicePrincipleConfig
            {
                KeyVaultInstanceName = "test"
            };
            Assert.Contains("test", spConfig.ToString());
        }
    }
}
