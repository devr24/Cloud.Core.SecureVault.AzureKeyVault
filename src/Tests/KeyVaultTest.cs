using Cloud.Core.SecureVault.AzureKeyVault.Config;
using Cloud.Core.Testing;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cloud.Core.SecureVault.AzureKeyVault.Tests
{
    public class KeyVaultTest
    {
        private readonly ISecureVault _kvClient;
        private readonly IConfiguration _config;

        public KeyVaultTest()
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

        [Fact, IsIntegration]
        public void Test_KeyVault_ToString()
        {
            Assert.Contains(_config.GetValue<string>("KeyVaultInstanceName"), ((KeyVault)_kvClient).ServicePrincipleConfig.ToString());
        }

        [Fact, IsIntegration]
        public void Test_KeyVault_UsingMSI()
        {
            var kvClient = new KeyVault(new MsiConfig { KeyVaultInstanceName = _config.GetValue<string>("KeyVaultInstanceName") });
            kvClient.GetSecret("testKey").GetAwaiter().GetResult().Should().Be("testVal");
        }

        [Theory, IsIntegration]
        [InlineData("testKey", "testVal")]
        public void Test_KeyVault_SetSecret(string key, string val)
        {
            // Principle needs "Set" permissions to run this.
            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret(key, val).GetAwaiter().GetResult()); 
        }

        [Fact, IsIntegration]
        public void Test_KeyVault_GetSecret()
        {
            // Principle needs "Set" and "Get" permissions to run this.
            AssertExtensions.DoesNotThrow(() => _kvClient.SetSecret("testKey", "testVal").GetAwaiter().GetResult());
            _kvClient.GetSecret("testKey").GetAwaiter().GetResult().Should().Be("testVal");

        }
    }
}
