namespace Cloud.Core.SecureVault.AzureKeyVault.Config
{
    using System;

    /// <summary>
    /// Msi Configuration for Azure KeyVault connection.
    /// </summary>
    public class MsiConfig
    {
        /// <summary>
        /// Gets or sets the name of the key vault instance.
        /// </summary>
        /// <value>
        /// The name of the key vault instance.
        /// </value>
        public string KeyVaultInstanceName { get; set; }

        /// <summary>
        /// Gets the URI of KeyVault.
        /// </summary>
        /// <value>.Config
        /// The URI for KeyVault.
        /// </value>
        public string Uri => $"https://{KeyVaultInstanceName}.vault.azure.net";

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"KeyVaultInstanceName: {KeyVaultInstanceName}, Uri: {Uri}";
        }

        /// <summary>
        /// Ensure mandatory properties are set.
        /// </summary>
        /// <exception cref="ArgumentException">KeyVaultInstanceName must be set</exception>
        public void Validate()
        {
            if (KeyVaultInstanceName.IsNullOrEmpty())
                throw new ArgumentException("KeyVaultInstanceName must be set");
        }
    }

    /// <summary>
    /// Service Principle Configuration for Azure KeyVault connection.
    /// </summary>
    public class ServicePrincipleConfig {

        /// <summary>
        /// Gets or sets the name of the key vault instance.
        /// </summary>
        /// <value>
        /// The name of the key vault instance.
        /// </value>
        public string KeyVaultInstanceName { get; set; }

        /// <summary>
        /// Gets the URI of KeyVault.
        /// </summary>
        /// <value>
        /// The URI for KeyVault.
        /// </value>
        public string Uri => $"https://{KeyVaultInstanceName}.vault.azure.net";

        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the application secret.
        /// </summary>
        /// <value>
        /// The application secret string.
        /// </value>
        public string AppSecret { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        /// <value>
        /// The tenant identifier.
        /// </value>
        public string TenantId { get; set; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"AppId: {AppId}, TenantId: {TenantId}, KeyVaultInstanceName: {KeyVaultInstanceName}, Uri: {Uri}";
        }

        /// <summary>
        /// Ensure mandatory properties are set.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// KeyVaultInstanceName must be set
        /// or
        /// AppId must be set
        /// or
        /// AppSecret must be set
        /// or
        /// TenantId must be set
        /// </exception>
        public void Validate()
        {
            if (KeyVaultInstanceName.IsNullOrEmpty())
                throw new ArgumentException("KeyVaultInstanceName must be set");

            if (AppId.IsNullOrEmpty())
                throw new ArgumentException("AppId must be set");

            if (AppSecret.IsNullOrEmpty())
                throw new ArgumentException("AppSecret must be set");

            if (TenantId.IsNullOrEmpty())
                throw new ArgumentException("TenantId must be set");
        }
    }
}
