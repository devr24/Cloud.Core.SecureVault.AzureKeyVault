using System;
using System.Linq;
using Cloud.Core.SecureVault.AzureKeyVault.Config;
using Cloud.Core.Testing;
using FluentAssertions;
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
            // Arrange - Principle needs "Set" permissions to run this.
            IServiceCollection serviceCollection = new ServiceCollection();

            // Act/Assert
            serviceCollection.AddKeyVaultSingleton(new MsiConfig { KeyVaultInstanceName = "instance" });
            serviceCollection.Any(x => x.ServiceType == typeof(ISecureVault)).Should().BeTrue();
            serviceCollection.Any(x => x.ServiceType == typeof(NamedInstanceFactory<ISecureVault>)).Should().BeTrue();
            serviceCollection.Clear();

            serviceCollection.AddKeyVaultSingleton(new ServicePrincipleConfig { KeyVaultInstanceName = "instance", AppId = "test", AppSecret = "test", TenantId = "test" });
            serviceCollection.Any(x => x.ServiceType == typeof(ISecureVault)).Should().BeTrue();
            serviceCollection.Any(x => x.ServiceType == typeof(NamedInstanceFactory<ISecureVault>)).Should().BeTrue();
            serviceCollection.Clear();

            serviceCollection.AddKeyVaultSingleton("instance");
            serviceCollection.Any(x => x.ServiceType == typeof(ISecureVault)).Should().BeTrue();
            serviceCollection.Any(x => x.ServiceType == typeof(NamedInstanceFactory<ISecureVault>)).Should().BeTrue();
            serviceCollection.Clear();

            serviceCollection.AddKeyVaultSingletonNamed("key", "instance");
            serviceCollection.Any(x => x.ServiceType == typeof(ISecureVault)).Should().BeTrue();
            serviceCollection.Any(x => x.ServiceType == typeof(NamedInstanceFactory<ISecureVault>)).Should().BeTrue();
            serviceCollection.Clear();

            serviceCollection.AddKeyVaultSingletonNamed("key", new ServicePrincipleConfig { KeyVaultInstanceName = "instance", AppId = "test", AppSecret = "test", TenantId = "test" });
            serviceCollection.Any(x => x.ServiceType == typeof(ISecureVault)).Should().BeTrue();
            serviceCollection.Any(x => x.ServiceType == typeof(NamedInstanceFactory<ISecureVault>)).Should().BeTrue();
        }

        /// <summary>Check the validate method carries out the validation as expected for managed service identity.</summary>
        [Fact]
        public void Test_KeyVault_MsiConfigValidate()
        {
            // Arrange
            var msiConfigGood = new MsiConfig { KeyVaultInstanceName = "test" };
            var msiConfigBad = new MsiConfig();
            var msiConfigString = msiConfigGood.ToString();

            // Act/Assert
            AssertExtensions.DoesNotThrow(() => msiConfigGood.Validate());
            var validationResult = msiConfigGood.Validate();
            validationResult.IsValid.Should().BeTrue();
            validationResult.Errors.Count().Should().Be(0);

            validationResult = msiConfigBad.Validate();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Count().Should().Be(1);

            msiConfigBad.KeyVaultInstanceName = "test";
            validationResult = msiConfigBad.Validate();
            validationResult.IsValid.Should().BeTrue();
            validationResult.Errors.Count().Should().Be(0);
        }

        /// <summary>Check the validate method carries out the validation as expected for service principle.</summary>
        [Fact]
        public void Test_KeyVault_ServicePrincipleConfigValidate()
        {
            // Arrange
            var spConfigGood = new ServicePrincipleConfig { KeyVaultInstanceName = "test", AppSecret = "test", TenantId = "test", AppId = "test" };
            var spConfigBad = new ServicePrincipleConfig();
            var spConfigString = spConfigGood.ToString();

            // Act/Assert
            AssertExtensions.DoesNotThrow(() => spConfigGood.Validate());

            var validationResult = spConfigBad.Validate();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Count().Should().Be(4);

            spConfigBad.KeyVaultInstanceName = "test";
            validationResult = spConfigBad.Validate();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Count().Should().Be(3);

            spConfigBad.AppId = "test";
            validationResult = spConfigBad.Validate();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Count().Should().Be(2);

            spConfigBad.AppSecret = "test";
            validationResult = spConfigBad.Validate();
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Count().Should().Be(1);

            spConfigBad.TenantId = "test";
            validationResult = spConfigBad.Validate();
            validationResult.IsValid.Should().BeTrue();
            validationResult.Errors.Count().Should().Be(0);
        }

        /// <summary>Verify the ToString method contains the expected output.</summary>
        [Fact]
        public void Test_ServicePrincipleConfig_ToString()
        {
            // Arrange
            var spConfig = new ServicePrincipleConfig
            {
                KeyVaultInstanceName = "test"
            };

            // Act/Assert
            Assert.Contains("test", spConfig.ToString());
        }
    }
}
