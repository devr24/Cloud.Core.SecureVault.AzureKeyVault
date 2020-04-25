using System;
using System.Linq;
using Cloud.Core.SecureVault.AzureKeyVault.Config;
using Cloud.Core.Testing;
using FluentAssertions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cloud.Core.SecureVault.AzureKeyVault.Tests
{
    [IsIntegration]
    public class KeyVaultIntegrationTests
    {
        private readonly ISecureVault _kvClient;
        private readonly IConfiguration _config;

        public KeyVaultIntegrationTests()
        {
            _config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            _kvClient = new KeyVault(new ServicePrincipleConfig
            {
                KeyVaultInstanceName = _config.GetValue<string>("InstanceName"),
                AppSecret = _config.GetValue<string>("AppSecret"),
                TenantId = _config.GetValue<string>("TenantId"),
                AppId = _config.GetValue<string>("AppId"),
            });
        }

        /// <summary>Check error when attempting to use Msi Auth.</summary>
        [Fact]
        public void Test_KeyVault_MsiError()
        {
            // Arrange
            var kvClient = new KeyVault(new MsiConfig { KeyVaultInstanceName = _config.GetValue<string>("InstanceName") });

            // Act/Assert
            kvClient.Name.Should().Be(_config.GetValue<string>("InstanceName"));
            (kvClient.Config as MsiConfig).Should().NotBeNull();
            Assert.Throws<Exception>(() => kvClient.GetSecret("test").GetAwaiter().GetResult());
        }

        /// <summary>Check the config collection extension method loads secrets as expected.</summary>
        [Fact]
        public void Test_ConfigExtensions_AddKeyVault()
        {
            // Arrange
            AssertExtensions.DoesNotThrow(() =>
            {
                _kvClient.SetSecret("test1", "test1").GetAwaiter().GetResult();
                _kvClient.GetSecret("test1").GetAwaiter().GetResult().Should().Be("test1");
            });

            var config = new ConfigurationBuilder();

            config.AddKeyVaultSecrets(new ServicePrincipleConfig
            {
                KeyVaultInstanceName = _config.GetValue<string>("InstanceName"),
                AppSecret = _config.GetValue<string>("AppSecret"),
                TenantId = _config.GetValue<string>("TenantId"),
                AppId = _config.GetValue<string>("AppId"),
            }, new[] { "test1" });

            // Act
            var builtConfig = config.Build();

            // Assert
            builtConfig.GetValue<string>("test1").Should().Be("test1");
        }

        /// <summary>Check the config collection extension method loads secrets as expected.</summary>
        [Fact]
        public void Test_ConfigExtensions_AddKeyVaultToServiceCollection()
        {
            // Arrange
            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret("test1", "test1").GetAwaiter().GetResult());

            var config = new ConfigurationBuilder();

            config.AddKeyVaultSecrets(new ServicePrincipleConfig
            {
                KeyVaultInstanceName = _config.GetValue<string>("InstanceName"),
                AppSecret = _config.GetValue<string>("AppSecret"),
                TenantId = _config.GetValue<string>("TenantId"),
                AppId = _config.GetValue<string>("AppId"),
            }, new[] { "test1" });

            var builtConfig = config.Build();
            builtConfig.GetValue<string>("test1").Should().Be("test1");

            // Act
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKeyVaultFromConfiguration(builtConfig);
            serviceCollection.Any(x => x.ServiceType == typeof(ISecureVault)).Should().BeTrue();

            var keyVaultService = serviceCollection.BuildServiceProvider().GetService<ISecureVault>();

            // Assert
            keyVaultService.GetSecret("test1").GetAwaiter().GetResult().Should().Be("test1");
        }

        /// <summary>Check the config extension method throws the expected "invalid operation" error when Msi auth is used but does we do not have msi access.</summary>
        [Fact]
        public void Test_ConfigExtensions_ErrorUsingMsi()
        {
            // Arrange
            var config = new ConfigurationBuilder();

            // Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                // Act
                config.AddKeyVaultSecrets(
                    new MsiConfig { KeyVaultInstanceName = _config.GetValue<string>("DoesNotExist") },
                    new[] { "test1" });
            });
        }

        /// <summary>Check the config extension method throws the expected "invalid operation" error when default (msi) auth is used but does we do not have msi access.</summary>
        [Fact]
        public void Test_ConfigExtensions_ErrorDefaultingToMsi()
        {
            // Arrange
            var config = new ConfigurationBuilder();

            // Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                // Act
                config.AddKeyVaultSecrets("test1", "test2", "test3", "test4");
            });
        }

        /// <summary>Verify an error is thrown when an attempt to pull secrets is carried out for an instance name that does not exist.</summary>
        [Fact]
        public void Test_ConfigExtensions_AddKeyVault_IncorrectInstanceName()
        {
            // Arrange
            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret("test1", "test1").GetAwaiter().GetResult());

            var config = new ConfigurationBuilder();
            
            // Assert
            Assert.Throws<InvalidOperationException>(() =>

                // Act
                config.AddKeyVaultSecrets(new ServicePrincipleConfig
                {
                    KeyVaultInstanceName = "test",
                    AppSecret = "test",
                    TenantId = "test",
                    AppId = "test"
                }, new[] { "test1" }));
        }
    }
}
