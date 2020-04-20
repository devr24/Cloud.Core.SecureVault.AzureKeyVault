# **Cloud.Core.SecureVault.AzureKeyVault** 
[![Build status](https://dev.azure.com/cloudcoreproject/CloudCore/_apis/build/status/Cloud.Core/Cloud.Core.SecureStore.AzureKeyVault_Package)](https://dev.azure.com/cloudcoreproject/CloudCore/_build/latest?definitionId=16) ![Code Coverage](https://cloud1core.blob.core.windows.net/codecoveragebadges/Cloud.Core.SecureVault.AzureKeyVault-LineCoverage.png) 
[![Cloud.Core.SecureVault.AzureKeyVault package in Cloud.Core feed in Azure Artifacts](https://feeds.dev.azure.com/cloudcoreproject/dfc5e3d0-a562-46fe-8070-7901ac8e64a0/_apis/public/Packaging/Feeds/8949198b-5c74-42af-9d30-e8c462acada6/Packages/3dc38659-5215-4eea-92b6-4d0064638677/Badge)](https://dev.azure.com/cloudcoreproject/CloudCore/_packaging?_a=package&feed=8949198b-5c74-42af-9d30-e8c462acada6&package=3dc38659-5215-4eea-92b6-4d0064638677&preferRelease=true)



<div id="description">

An Azure specific Key Vault implementation, enabling users to store and use cryptographic keys within the Microsoft Azure environment.

</div>

## Usage
There are two ways you can instantiate the KeyVault Client.  Each way dictates the security mechanism the client uses to connect.  The two mechanisms are:

1. Service Principle
2. Managed Service Identity (MSI)

Below are examples of instantiating each type.

### 1. Service Principle
Create an instance of the KeyVault client with KeyVaultConfig for Service Principle as follows:

```
var kvConfig = new ServicePrincipleConfig
    {
        AppId = "<appid>",
        AppSecret = "<appsecret>",
        TenantId = "<tenantid>",
        KeyVaultInstanceName = "<kvInstanceName>"
    };
	
// KeyVault client.
var kv = new KeyVault(kvConfig);
```

Usually the AppId, AppSecret (both of which are setup when creating a new service principle within Azure) and TenantId are specified in 
Configuration (environment variables/AppSetting.json file/key value pair files [for Kubernetes secret store] or command line arguments).

SubscriptionId can be accessed through the secret store (this should not be stored in configuration).

### 2. Managed Service Identity (MSI)
Create an instance of the Blob Storage client with MSI authentication as follows:

```
var kvConfig = new MsiConfig
    {
        KeyVaultInstanceName = "<kvInstanceName>"
    };
	
// KeyVault client.
var kv = new KeyVault(kvConfig);
```

All that's required is the instance name to connect to.  Authentication runs under the context the application is running.

### Getting secrets

The following code shows an example of grabbing a SubscriptionId from KeyVault (client initialised in samples above):

```
var subscriptionId = await kv.GetSecret("SubscriptionId");
```

### Useful Extension Methods

**Explicitly Specify Instance Name**

You can connect to KeyVault using a specific config method (Msi or Service Principle) by specifying an instance name and the keys you wish to pull.  They will automatically be added to config builder:

```csharp
public class Startup
{
   ...
   
    public void ConfigureAppConfiguration(IConfigurationBuilder builder)
    {
        builder.UseDefaultConfigs();

        // Get key vault instance name u
        var kvInstanceName = builder.GetValue<string>("KeyVaultInstanceName");

        // Pass the KeyVault instance you wish to connect to and the keys.
        builder.AddKeyVaultSecrets(new MsiConfig
        {
            KeyVaultInstanceName = kvInstanceName
        }, new [] {"TenantId", "SubscriptionId", "AppInsightsInstrumentationKey" });
    }
    
    ...
   
}
```
Or you can do the shorthand version of:


```csharp
// PREFERRED USAGE...
public class Startup
{
   ...
   
    public void ConfigureAppConfiguration(IConfigurationBuilder builder)
    {
        builder.UseDefaultConfigs();

        // Get key vault instance name u
        var kvInstanceName = builder.GetValue<string>("KeyVaultInstanceName");

        // Pass the KeyVault instance you wish to connect to and the keys.
        builder.AddKeyVaultSecrets(kvInstanceName,  
			"TenantId", 
			"SubscriptionId", 
			"AppInsightsInstrumentationKey");
    }
    ...
   
}
```

**Infer Instance Name**

You can get key vault config values during the ConfigureAppConfiguration method and add the values automatically into the configuration builder for future use.

The key value instance name can be inferred automatically via the "KeyVaultInstanceName" config value as follows:

```csharp
public class Startup
{
   ...
   
    public void ConfigureAppConfiguration(IConfigurationBuilder builder)
    {
        builder.UseDefaultConfigs();

        // Instance name is pulled from config value "KeyVaultInstanceName"
        builder.AddKeyVaultSecrets(new [] { "TenantId", "SubscriptionId", "AppInsightsInstrumentationKey" });
    }
    
    ...
   
}
```

## Adding an instance to service collection (dependency injection)

There may be cases where the Key Vault service (ISecureVault) needs to be injected into the service collection for use in a down stream class.  

There are a number of extension methods to support this for the IServiceCollection, such as:

```csharp
public class Startup
{
    ...
   
    public void ConfigureServices(IConfiguration config, ILogger logger, IServiceCollection services)
    {
        // Instance name is pulled from config value "KeyVaultInstanceName"
        services.AddKeyVaultSingleton(new MsiConfig { KeyVaultInstanceName = "instanceName" });
	
	// This could be simplified down to...
        services.AddKeyVaultSingleton("instanceName");
	
	// A named instance can be added as follows
	services.AddKeyVaultSingletonNamed("key", "instanceName");
    }
    
    ...
}
```

If you've already used KeyVault in the IConfigurationBuilder section, you don't want to have to instantiate a new instance.  Instead you can do the following, to add the already setup client to the service collection:

```csharp
public class Startup
{
    ...
    
    public void ConfigureAppConfiguration(IConfigurationBuilder builder)
    {
        builder.UseDefaultConfigs();

        // Instance name is pulled from config value "KeyVaultInstanceName"
        builder.AddKeyVaultSecrets(builder.GetValue<string>("KeyVaultInstanceName"), 
              "TenantId", 
              "SubscriptionId", 
              "AppInsightsInstrumentationKey");
    }
    
    ...
    
    public void ConfigureServices(IConfiguration config, ILogger logger, IServiceCollection services)
    {
        // Instance used in IConfigurationBuilder will be reused and added to IServiceCollection.
        services.AddKeyVaultFromConfiguration(config);
    }
    
    ...
}
```

## Test Coverage
A threshold will be added to this package to ensure the test coverage is above 80% for branches, functions and lines.  If it's not above the required threshold 
(threshold that will be implemented on ALL of the core repositories to gurantee a satisfactory level of testing), then the build will fail.

## Compatibility
This package has has been written in .net Standard and can be therefore be referenced from a .net Core or .net Framework application. The advantage of utilising from a .net Core application, 
is that it can be deployed and run on a number of host operating systems, such as Windows, Linux or OSX.  Unlike referencing from the a .net Framework application, which can only run on 
Windows (or Linux using Mono).
 
## Setup
This package is built using .net Standard 2.1 and requires the .net Core 3.1 SDK, it can be downloaded here: 
https://www.microsoft.com/net/download/dotnet-core/

IDE of Visual Studio or Visual Studio Code, can be downloaded here:
https://visualstudio.microsoft.com/downloads/

## How to access this package
All of the Cloud.Core.* packages are published to a internal NuGet feed.  To consume this on your local development machine, please add the following feed to your feed sources in Visual Studio:
https://pkgs.dev.azure.com/cloudcoreproject/CloudCore/_packaging/Cloud.Core/nuget/v3/index.json
 
For help setting up, follow this article: https://docs.microsoft.com/en-us/vsts/package/nuget/consume?view=vsts


<img src="https://cloud1core.blob.core.windows.net/icons/cloud_core_small.PNG" />
