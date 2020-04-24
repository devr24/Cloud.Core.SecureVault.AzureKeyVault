namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Configuration;
    using System.Linq;
    using Cloud.Core;
    using Cloud.Core.SecureVault.AzureKeyVault;
    using Cloud.Core.SecureVault.AzureKeyVault.Config;

    /// <summary>
    /// Class IServiceCollectionExtensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the KeyVault instance, already used on IConfigurationBuilder to the service collection.  This allows it to also be used
        /// as a dependency within your application.  Requires the IConfigurationBuilder to have had the AddKeyVaultSecrets function on it.
        /// </summary>
        /// <exception cref="InvalidOperationException">Will throw an exception when IConfiguration has not had the AddKeyVaultSecrets function used on it.</exception>
        /// <param name="services">Service collection to add the ISecureVault client as a dependency to.</param>
        /// <param name="config">Configuration object that had items added from secure vault.</param>
        /// <returns>IServiceCollection with the ISecureVault service added.</returns>
        public static IServiceCollection AddKeyVaultFromConfiguration(this IServiceCollection services, IConfiguration config)
        {
            // This will get the secure vault instance (if one was setup).
            var kv = config.GetConfigKeyVault();

            // If it was setup - then add to service collection and return.
            if (kv != null)
            {
                services.AddSingleton(kv);
                return services;
            }

            // If one was not previously setup before making the call to this method, then an invalid operation exception will be thrown.
            throw new InvalidOperationException("KeyVault instance has not been initialised, ensure you've called AddKeyVaultSecrets on the IConfigurationBuilder'");
        }

        /// <summary>
        /// Adds the key vault singleton instance using managed identity config.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddKeyVaultSingleton(this IServiceCollection services, MsiConfig config)
        {
            services.AddSingleton<ISecureVault>(new KeyVault(config));
            AddFactoryIfNotAdded(services);
            return services;
        }

        /// <summary>
        /// Adds the key vault singleton using Msi authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="instanceName">Name of the key vault instance.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddKeyVaultSingleton(this IServiceCollection services, string instanceName)
        {
            services.AddSingleton<ISecureVault>(new KeyVault(new MsiConfig { KeyVaultInstanceName = instanceName }));
            AddFactoryIfNotAdded(services);
            return services;
        }

        /// <summary>
        /// Adds the key vault singleton instance using service principle config.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddKeyVaultSingleton(this IServiceCollection services, ServicePrincipleConfig config)
        {
            services.AddSingleton<ISecureVault>(new KeyVault(config));
            AddFactoryIfNotAdded(services);
            return services;
        }

        /// <summary>
        /// Adds the key vault singleton named with Msi authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="key">The key.</param>
        /// <param name="instanceName">Name of the instance.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddKeyVaultSingletonNamed(this IServiceCollection services, string key, string instanceName)
        {
            var instance = new KeyVault(new MsiConfig {KeyVaultInstanceName = instanceName});

            if (!key.IsNullOrEmpty())
                instance.Name = key;

            services.AddSingleton<ISecureVault>(instance);
            AddFactoryIfNotAdded(services);
            return services;
        }

        /// <summary>
        /// Adds the key vault singleton named with service principle authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="key">The key.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddKeyVaultSingletonNamed(this IServiceCollection services, string key, ServicePrincipleConfig config)
        {
            var instance = new KeyVault(config);

            if (!key.IsNullOrEmpty())
            {
                instance.Name = key;
            }

            services.AddSingleton<ISecureVault>(instance);
            AddFactoryIfNotAdded(services);
            return services;
        }

        /// <summary>
        /// Add the generic service factory from Cloud.Core for the ISecureVault type.  This allows multiple named instances of the same instance.
        /// </summary>
        /// <param name="services">Service collection to extend.</param>
        private static void AddFactoryIfNotAdded(IServiceCollection services)
        {
            if (services.All(x => x.ServiceType != typeof(NamedInstanceFactory<ISecureVault>)))
            {
                // Service Factory doesn't exist, so we add it to ensure it's always available.
                services.AddSingleton<NamedInstanceFactory<ISecureVault>>();
            }
        }
    }
}
