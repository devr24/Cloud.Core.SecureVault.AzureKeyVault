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
                KeyVaultInstanceName = _config.GetValue<string>("KeyVaultInstanceName"),
                AppSecret = _config.GetValue<string>("AppSecret"),
                TenantId = _config.GetValue<string>("TenantId"),
                AppId = _config.GetValue<string>("AppId"),
            });
        }

        /// <summary>[Using ServicePrinciple Config) Verify values in KeyVault can be set and got.</summary>
        [Theory]
        [InlineData("testKey", "testVal")]
        public void Test_KeyVault_SetAndGetSecret(string key, string val)
        {
            var kvClient = new KeyVault(new MsiConfig { KeyVaultInstanceName = _config.GetValue<string>("KeyVaultInstanceName") });
            Assert.Throws<KeyVaultErrorException>(() => kvClient.GetSecret("test").GetAwaiter().GetResult());
            Assert.Throws<KeyVaultErrorException>(() => kvClient.GetSecret("test").GetAwaiter().GetResult());

            // Principle needs "Set" permissions to run this.
            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret(key, val).GetAwaiter().GetResult());

            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret("testKey", "testVal").GetAwaiter().GetResult());
            _kvClient.GetSecret("testKey").GetAwaiter().GetResult().Should().Be("testVal");
        }

        /// <summary>Check the config collection extension method loads secrets as expected.</summary>
        [Fact]
        public void Test_ConfigExtensions_AddKeyVault()
        {
            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret("test1", "test1").GetAwaiter().GetResult());

            var config = new ConfigurationBuilder();

            config.AddKeyVaultSecrets(new ServicePrincipleConfig
            {
                KeyVaultInstanceName = _config.GetValue<string>("KeyVaultInstanceName"),
                AppSecret = _config.GetValue<string>("AppSecret"),
                TenantId = _config.GetValue<string>("TenantId"),
                AppId = _config.GetValue<string>("AppId"),
            }, new[] { "test1" });

            var builtConfig = config.Build();
            builtConfig.GetValue<string>("test1").Should().Be("test1");
        }


        /// <summary>Check the config collection extension method loads secrets as expected.</summary>
        [Fact]
        public void Test_ConfigExtensions_AddKeyVaultToServiceCollection()
        {
            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret("test1", "test1").GetAwaiter().GetResult());

            var config = new ConfigurationBuilder();

            config.AddKeyVaultSecrets(new ServicePrincipleConfig
            {
                KeyVaultInstanceName = _config.GetValue<string>("KeyVaultInstanceName"),
                AppSecret = _config.GetValue<string>("AppSecret"),
                TenantId = _config.GetValue<string>("TenantId"),
                AppId = _config.GetValue<string>("AppId"),
            }, new[] { "test1" });

            var builtConfig = config.Build();
            builtConfig.GetValue<string>("test1").Should().Be("test1");

            var serviceCollection = new FakeServiceCollection();
            serviceCollection.AddKeyVaultFromConfiguration(builtConfig);

            var descriptor = new ServiceDescriptor(typeof(ISecureVault), typeof(KeyVault));
            serviceCollection.Contains(descriptor).Should().BeTrue();

            var keyVaultService = (ISecureVault)serviceCollection.GetService(descriptor);
            keyVaultService.GetSecret("test1").GetAwaiter().GetResult().Should().Be("test1");
        }

        /// <summary>Check the config extension method throws the expected "invalid operation" error when Msi auth is used but does we do not have msi access.</summary>
        [Fact]
        public void Test_ConfigExtensions_AddKeyVaultMsi()
        {
            var config = new ConfigurationBuilder();

            Assert.Throws<InvalidOperationException>(() =>
            {
                config.AddKeyVaultSecrets(
                    new MsiConfig { KeyVaultInstanceName = _config.GetValue<string>("KeyVaultInstanceName") },
                    new[] { "test1" });
            });
        }

        /// <summary>Check the config extension method throws the expected "invalid operation" error when default (msi) auth is used but does we do not have msi access.</summary>
        [Fact]
        public void Test_ConfigExtensions_AddKeyVaultDefaultsToMsi()
        {
            var config = new ConfigurationBuilder();

            Assert.Throws<InvalidOperationException>(() =>
            {
                config.AddKeyVaultSecrets("test1", "test2", "test3", "test4");
            });
        }

        /// <summary>Verify an error is thrown when an attempt to pull secrets is carried out for an instance name that does not exist.</summary>
        [Fact]
        public void Test_ConfigExtensions_AddKeyVault_IncorrectInstanceName()
        {
            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret("test1", "test1").GetAwaiter().GetResult());

            var config = new ConfigurationBuilder();

            Assert.Throws<InvalidOperationException>(() =>
            config.AddKeyVaultSecrets(new ServicePrincipleConfig
            {
                KeyVaultInstanceName = "test",
                AppSecret = "test",
                TenantId = "test",
                AppId = "test"
            }, new[] { "test1" }));
        }
    }

    public class FakeServiceCollection : IServiceCollection
    {
        IEnumerable<ServiceDescriptor> _serviceDescriptors = new List<ServiceDescriptor>();

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _serviceDescriptors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            ((List<ServiceDescriptor>)_serviceDescriptors).Add(item);
        }

        public void Clear()
        {
            ((List<ServiceDescriptor>)_serviceDescriptors).Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(ServiceDescriptor item)
        {
            for (int i = 0; i < ((List<ServiceDescriptor>)_serviceDescriptors).Count; i++)
            {
                if (((List<ServiceDescriptor>)_serviceDescriptors)[i].ServiceType == item?.ServiceType)
                    return i;
            }

            return -1;
        }

        public object GetService(ServiceDescriptor item)
        {
            var index = IndexOf(item);
            var serviceDescriptor = ((List<ServiceDescriptor>)_serviceDescriptors)[index];
            return serviceDescriptor.ImplementationInstance;
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public ServiceDescriptor this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
