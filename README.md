# **Cloud.Core.SecureVault.AzureKeyVault**

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

## Test Coverage
A threshold will be added to this package to ensure the test coverage is above 80% for branches, functions and lines.  If it's not above the required threshold 
(threshold that will be implemented on ALL of the new core repositories going forward), then the build will fail.

## Compatibility
This package has has been written in .net Standard and can be therefore be referenced from a .net Core or .net Framework application. The advantage of utilising from a .net Core application, 
is that it can be deployed and run on a number of host operating systems, such as Windows, Linux or OSX.  Unlike referencing from the a .net Framework application, which can only run on 
Windows (or Linux using Mono).
 
## Setup
This package requires the .net Core 2.1 SDK, it can be downloaded here: 
https://www.microsoft.com/net/download/dotnet-core/2.1

IDE of Visual Studio or Visual Studio Code, can be downloaded here:
https://visualstudio.microsoft.com/downloads/

## How to access this package
All of the Cloud.Core.* packages are published to our internal NuGet feed.  To consume this on your local development machine, please add the following feed to your feed sources in Visual Studio:
TBC

For help setting up, follow this article: https://docs.microsoft.com/en-us/vsts/package/nuget/consume?view=vsts
