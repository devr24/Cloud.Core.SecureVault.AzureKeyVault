namespace Cloud.Core.SecureVault.AzureKeyVault.Config
{
    using System.ComponentModel.DataAnnotations;
    using Attributes;

    /// <summary>
    /// Msi Configuration for Azure KeyVault connection.
    /// </summary>
    public class MsiConfig : AttributeValidator
    {
        /// <summary>
        /// Gets or sets the name of the key vault instance.
        /// </summary>
        /// <value>
        /// The name of the key vault instance.
        /// </value>
        [Required]
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
    }

    /// <summary>
    /// Service Principle Configuration for Azure KeyVault connection.
    /// </summary>
    public class ServicePrincipleConfig : AttributeValidator 
    {

        /// <summary>
        /// Gets the URI of KeyVault.
        /// </summary>
        /// <value>
        /// The URI for KeyVault.
        /// </value>
        public string Uri => $"https://{KeyVaultInstanceName}.vault.azure.net";

        /// <summary>
        /// Gets or sets the name of the key vault instance.
        /// </summary>
        /// <value>
        /// The name of the key vault instance.
        /// </value>
        [Required]
        public string KeyVaultInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        [Required]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the application secret.
        /// </summary>
        /// <value>
        /// The application secret string.
        /// </value>
        [Required]
        public string AppSecret { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        /// <value>
        /// The tenant identifier.
        /// </value>
        [Required]
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
    }
}
