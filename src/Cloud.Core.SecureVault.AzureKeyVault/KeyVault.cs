namespace Cloud.Core.SecureVault.AzureKeyVault
{
    using Config;
    using System;
    using Microsoft.Azure.KeyVault;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Azure.Services.AppAuthentication;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Azure KeyVault specific implementation of Secure Vault.
    /// </summary>
    /// <seealso cref="Cloud.Core.ISecureVault" />
    public class KeyVault : ISecureVault
    {
        internal readonly MsiConfig MsiConfig;
        internal readonly ServicePrincipleConfig ServicePrincipleConfig;
        internal readonly string InstanceUri;
        internal DateTimeOffset? TokenExpiryTime;

        private IKeyVaultClient _client; // only to be used by the "Client" property.

        public string Name { get; set; }

        internal IKeyVaultClient Client
        {
            get
            {
                if (_client == null || TokenExpiryTime <= DateTime.UtcNow)
                {
                    if (MsiConfig != null)
                    {
                        // Default expiry to 1 day.
                        TokenExpiryTime = DateTime.Now.AddDays(1);

                        // Create MSI authenticated client.
                        var azureServiceTokenProvider = new AzureServiceTokenProvider();
                        _client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                    }
                    else
                        // Create Service Principle client.
                        _client = new KeyVaultClient(GetKvAccessToken);
                }

                return _client;
            }
        }

        /// <summary>Initializes a new instance of the KeyVault class using Managed Service Principle (MSI) security.</summary>
        /// <param name="config">Msi configuration for this instance.</param>
        public KeyVault([NotNull] MsiConfig config)
        {
            // Ensure the required config values are all set.
            config.ThrowIfInvalid();

            MsiConfig = config;
            InstanceUri = config.Uri;
            Name = config.KeyVaultInstanceName;
        }

        /// <summary>Initializes a new instance of the KeyVault class using Service Principle security.</summary>
        /// <param name="config">Service Principle configuration the instance.</param>
        public KeyVault([NotNull] ServicePrincipleConfig config)
        {
            // Ensure the required config values are all set.
            config.ThrowIfInvalid();

            ServicePrincipleConfig = config;
            InstanceUri = config.Uri;
            Name = config.KeyVaultInstanceName;
        }

        /// <inheritdoc />
        public async Task<string> GetSecret([NotNull] string key)
        {
            var secret = await Client.GetSecretAsync(InstanceUri, key);
            return secret.Value;
        }

        /// <inheritdoc />
        public async Task SetSecret([NotNull] string key, [NotNull] string value)
        {
            await Client.SetSecretAsync(InstanceUri, key, value);
        }

        /// <summary>
        /// Gets the KeyVault access token (used in service principle auth).
        /// </summary>
        /// <param name="authority">The authority (login uri).</param>
        /// <param name="resource">The resource (resource being accessed, in this case KeyVault).</param>
        /// <param name="scope">The scope of the accessor (not used).</param>
        /// <returns>Async <see cref="Task"/> <see cref="string"/></returns>
        internal async Task<string> GetKvAccessToken(string authority, string resource, string scope)
        {
            const string windowsLoginAuthority = "https://login.windows.net/";

            var context = new AuthenticationContext($"{windowsLoginAuthority}{ServicePrincipleConfig.TenantId}");
            var credential = new ClientCredential(ServicePrincipleConfig.AppId, ServicePrincipleConfig.AppSecret);
            var tokenResult = await context.AcquireTokenAsync(resource, credential);

            TokenExpiryTime = tokenResult.ExpiresOn;

            if (tokenResult == null)
                throw new InvalidOperationException($"Could not authenticate to {windowsLoginAuthority}{ServicePrincipleConfig.TenantId} using supplied AppId: {ServicePrincipleConfig.AppId}");

            return tokenResult.AccessToken;
        }
    }
}
