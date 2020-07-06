namespace Cloud.Core.SecureVault.AzureKeyVault
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using Config;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Azure KeyVault specific implementation of Secure Vault.
    /// </summary>
    /// <seealso cref="ISecureVault" />
    public class KeyVault : ISecureVault
    {
        internal readonly MsiConfig MsiConfig;
        internal readonly ServicePrincipleConfig ServicePrincipleConfig;
        internal readonly string InstanceUri;
        internal DateTimeOffset? TokenExpiryTime;

        private IKeyVaultClient _client; // only to be used by the "Client" property.

        /// <summary>
        /// Gets or sets the instance name for the implementor of the INamedInstance interface.
        /// </summary>
        /// <value>The instance name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public object Config { get; internal set; }

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
            Config = config;
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
            Config = config;
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
            try
            {
                await Client.SetSecretAsync(InstanceUri, key, value);
            }
            catch (KeyVaultErrorException e)
                when (e.Response.StatusCode == HttpStatusCode.Conflict)
            {
                // This process kicks in when a soft delete has occurred.  Calling the recover will force the soft deleted
                // key to be recovered, and then it can be set again.
                _ = await Client.RecoverDeletedSecretAsync(InstanceUri, key);
                await Task.Delay(15000);
                await Client.SetSecretAsync(InstanceUri, key, value);
            }
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
