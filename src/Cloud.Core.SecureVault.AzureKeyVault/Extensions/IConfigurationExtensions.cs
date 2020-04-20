namespace Microsoft.Extensions.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Cloud.Core;
    using Cloud.Core.SecureVault.AzureKeyVault;
    using Cloud.Core.SecureVault.AzureKeyVault.Config;
    using Microsoft.Azure.KeyVault.Models;

    /// <summary>
    /// Class IConfigurationExtensions.
    /// </summary>
    public static class IConfigurationExtensions
    {
        private static ISecureVault _keyVaultInstance { get; set; }

        internal static ISecureVault GetConfigKeyVault(this IConfiguration config)
        {
            return _keyVaultInstance;
        }

        /// <summary>
        /// Adds the key vault secrets specified.  Uses Msi auth and builds the instance name on the fly.
        /// </summary>
        /// <param name="builder">The builder to extend.</param>
        /// <param name="instanceName">KeyVault instance name to connect to.</param>
        /// <param name="params">The list of keys to load.</param>
        /// <returns>IConfigurationBuilder with param keys as settings.</returns>
        /// <exception cref="InvalidOperationException">
        /// Expecting setting \"KeyVaultInstanceName\" to infer instance name
        /// or
        /// Problem occurred retrieving secrets from KeyVault using Managed Identity
        /// </exception>
        public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder builder, string instanceName, params string[] @params)
        {
            var mem = new Dictionary<string, string>();
            mem.Add("KeyVaultInstanceName", instanceName);
            builder.AddInMemoryCollection(mem);

            return AddKeyVaultSecrets(builder, @params.ToList());
        }

        /// <summary>
        /// Adds the key vault secrets specified.  Uses Msi auth and builds the instance name on the fly.
        /// Needs config value "KeyVaultInstanceName" to work.
        /// </summary>
        /// <param name="builder">The builder to extend.</param>
        /// <param name="params">The list of keys to load.</param>
        /// <returns>IConfigurationBuilder with param keys as settings.</returns>
        /// <exception cref="InvalidOperationException">
        /// Expecting setting \"KeyVaultInstanceName\" to infer instance name
        /// or
        /// Problem occurred retrieving secrets from KeyVault using Managed Identity
        /// </exception>
        public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder builder, params string[] @params)
        {
            return AddKeyVaultSecrets(builder, @params.ToList());
        }

        /// <summary>
        /// Adds the key vault secrets specified.  Uses Msi auth and builds the instance name on the fly.
        /// Needs config value "KeyVaultInstanceName" to work.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="keys">The list of keys to load.</param>
        /// <param name="throwNotFoundErrors">If {true}, when a key is missing an invalid operation exception will be thrown. If {false}, the
        /// error will be supressed and it will just not add the key to the returned collection.</param>
        /// <returns>IConfigurationBuilder.</returns>
        /// <exception cref="InvalidOperationException">
        /// Expecting setting \"KeyVaultInstanceName\" to infer instance name
        /// or
        /// Problem occurred retrieving secrets from KeyVault using Managed Identity
        /// </exception>
        public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder builder, List<string> keys, bool throwNotFoundErrors = false)
        {
            try
            {
                var instanceName = builder.Build().GetValue<string>("KeyVaultInstanceName");

                if (instanceName.IsNullOrEmpty())
                    throw new InvalidOperationException("Expecting setting \"KeyVaultInstanceName\" to infer instance name");

                var vault = new KeyVault(new MsiConfig { KeyVaultInstanceName = instanceName });
                var secrets = new List<KeyValuePair<string, string>>();

                // Gather secrets from Key Vault
                foreach (var key in keys)
                {
                    try
                    {
                        var value = vault.GetSecret(key).GetAwaiter().GetResult();
                        secrets.Add(new KeyValuePair<string, string>(key, value));
                    }
                    catch (KeyVaultErrorException e)
                        when (e.Response.StatusCode == HttpStatusCode.NotFound)
                    { 
                        // Throw an exception if requested.
                        if (throwNotFoundErrors)
                            throw;

                        // Do nothing if it fails to find the value.
                        Console.WriteLine($"Failed to find keyvault setting: {key}, exception: {e.Message}");
                    }
                }

                // Add them to config.
                builder.AddInMemoryCollection(secrets);

                // Keep track of instance.
                _keyVaultInstance = vault;

                // Return updated builder.
                return builder;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Problem occurred retrieving secrets from KeyVault using Managed Identity", ex);
            }
        }

        /// <summary>
        /// Adds key vault secrets to the configuration builder.
        /// Uses MSI configuration for security.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="keys">The keys for the secrets to lookup.</param>
        /// <returns>IConfigurationBuilder.</returns>
        /// <exception cref="InvalidOperationException">Problem occurred retrieving secrets from KeyVault</exception>
        public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder builder, MsiConfig config, params string[] keys)
        {
            try
            {
                var vault = new KeyVault(config);
                var secrets = new List<KeyValuePair<string, string>>();

                // Gather secrets from Key Vault
                foreach (var key in keys)
                {
                    try
                    {
                        var value = vault.GetSecret(key).GetAwaiter().GetResult();
                        secrets.Add(new KeyValuePair<string, string>(key, value));
                    }
                    catch (KeyVaultErrorException e) 
                        when (e.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Do nothing if it fails to find the value.
                        Console.WriteLine($"Failed to find keyvault setting: {key}, exception: {e.Message}");
                    }
                }

                // Add them to config.
                builder.AddInMemoryCollection(secrets);

                // Keep track of instance.
                _keyVaultInstance = vault;

                // Return updated builder.
                return builder;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Problem occurred retrieving secrets from KeyVault using Managed Identity", ex);
            }
        }

        /// <summary>
        /// Adds key vault secrets to the configuration builder.
        /// Uses Service Principle configuration for security.
        /// </summary>
        /// <param name="builder">The builder to extend.</param>
        /// <param name="config">The service principle configuration information.</param>
        /// <param name="keys">The keys to lookup.</param>
        /// <returns>IConfigurationBuilder.</returns>
        /// <exception cref="InvalidOperationException">Problem occurred retrieving secrets from KeyVault</exception>
        public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder builder, ServicePrincipleConfig config, params string[] keys)
        {
            try
            {
                var vault = new KeyVault(config);
                var secrets = new List<KeyValuePair<string, string>>();

                // Gather secrets from Key Vault
                foreach (var key in keys)
                {
                    try
                    {
                        var value = vault.GetSecret(key).GetAwaiter().GetResult();
                        secrets.Add(new KeyValuePair<string, string>(key, value));
                    }
                    catch (KeyVaultErrorException e)
                        when (e.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Do nothing if it fails to find the value.
                        Console.WriteLine($"Failed to find keyvault setting: {key}, exception: {e.Message}");
                    }
                }

                // Add them to config.
                builder.AddInMemoryCollection(secrets);

                // Keep track of instance.
                _keyVaultInstance = vault;

                // Return updated builder.
                return builder;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Problem occurred retrieving secrets from KeyVault using Service Principle", ex);
            }
        }
    }
}
