# SigningServer

A simple server for code-signing binaries for internal infrastructure usage.
If you work in a company with many teams it's sometimes hard to maintain code signing. Every team needs to sign their
binaries
as part of their build process and therefore need the certificate installed on their build servers. This software solves
this issue
by providing the code-signing as a service. You setup on a central signing server this software as a windows service and
using the shipped client any other client can ask the central server to sign the files.

# Features

See the [Features](Docs/features.md) document for a detailed list of features.

## Versioning
Unless stated differently in the release notes, we use [Semantic Versioning](https://semver.org/).

## Builds

### Server 
**Stable Releases:** For main stable releases we use GitHub Releases. Binaries for the application can be consumed from there 
and installed by copying them into any folder and registering it as a Windows service via `SC CREATE`.

https://github.com/Danielku15/SigningServer/releases

**Prereleases:** Pre-releases are typically created when the software has already some significant changes in, which are ready
to be tested in staging environments. They are considered fairly stable while new features might not yet fully work as expected. 
They can be obtained from GitHub Actions as artifacts under the "Prerelease" workflow. 

https://github.com/Danielku15/SigningServer/actions/workflows/prerelease.yml


**Intermediate Builds:** Debug and release builds of every change are also created. While we try to avoid breaking old functionality
it can happen on ongoing refactorings or changes that there are breaking changes and instabilities. 
They can be obtained from GitHub Actions as artifacts under the "Build" workflow.

https://github.com/Danielku15/SigningServer/actions/workflows/build.yml

### Clients

Clients are generally available like the server binaries. With v2.1 and higher we plan to ship clients also as
[.NET tools](learn.microsoft.com/en-us/dotnet/core/tools/global-tools) via NuGet.org. Stable and prereleases are published
to NuGet.org while normal binaries can only be obtained from GitHub Actions. 

https://www.nuget.org/packages/SigningServer.Client/
https://www.nuget.org/packages/SigningServer.StandaloneClient/

## Configuration 

### Server Configuration

The server is configured using a `appsettings.json` beside the executable. The application
specific settings are nested in the `"SigningServer` key.

```json
{
    // The Kestrel HTTP Server configuration (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-6.0)
    "Kestrel": {
        "Endpoints": {
            "Http": {
                "Url": "http://localhost:5000"
            }
        }
    },
    // some general system information presented on the landing page (available on the hostname+port as configured)
    "SystemInfo": {
        // The name of the service, displayed as headline
        "ServiceName": "MyCorp Signing Server",
        // A short description of the service displayed on the landing page for users to understand where they are 
        "ServiceDescription": "This is the signing server for MyCrop. Check the knowledge base link below for more info.",
        // A link to the support page where users can get help
        "SupportLink": "https://mycorp.com/support",
        // A link to a knowledge base article describing this service and how to gain access.
        "KnowledgeBaseLink": "https://mycorp.com/signing-server-kb"
    },
            
    // the service configuration
    "SigningServer": {
        // whether to do internal certificate handle pooling or reuse the certificate instance (can improve USB token resiliance)
        "UseCertificatePooling": true,
        // The directory where the server will put temporarily the files during signing
        "WorkingDirectory": "C:\\SigningServer\\WorkingDirectory",
        // A RFC-3161 compliant timestamping server which should be used. 
        "TimestampServer": "http://timestamp.digicert.com",
        // A fallback Authenticode timestamping server for SHA1 based signing
        "Sha1TimestampServer": "https://timestamp.sectigo.com",
        // The interval in which the HSM certificates should be reloaded
        // to avoid unexpected  
        "HardwareCertificateUnlockIntervalInSeconds": 3600,
        // The maximum degree of parallelism allowed per individual client.
        "MaxDegreeOfParallelismPerClient": 4,
        // An array of certificates which should be loaded and made available
        "Certificates": [
            // Example for a certificate from the local windows certificate store
            {
                // Very basic security and certificate selection mechanism
                // Can be removed or left empty for the default certificate
                // which should be used in case the client does not supply credentials.
                // There can only be one certificate without username and password 
                "CertificateName": "MyCodeSigningCert", // The name used in logging and reporting areas
                // A list of credentials which can be used to select this certificate,
                // Leave this empty or add a item with username and password set to empty 
                // To use it as default certificate 
                "Credentials": [
                  { "Username": "", "Password": "" }, // default when no credentials are supplied
                  { "Username": "team01", "Password": "teampass01" } // designated credentials for a team                  
                ],
                "Local": {
                    "Thumbprint": "", // The thumbprint of the certificate to load
                    "StoreName": "", // The name of the certificate store to access (AddressBook, AuthRoot, CertificateAuthority, Disallowed, My, Root, TrustedPeople, TrustedPublisher)
                    "StoreLocation": "", // The location of the store (CurrentUser, LocalMachine)
                    "TokenPin": ""  // The pin to unlock the hardware token (holding EV certificates)
                                    // The pin is encrypted, obtain the value to put here with 
                                    // SigningServer.exe -encode TokenPinHere
                                    // is is protected with the Windows DPAPI
                                    // If the pin is prefixed with the following marker it is not considered encrypted
                                    // and treated as it is: 
                                    //     plain-text:
                }
            },
            // Example for a certificate from an Azure KeyVault
            {
                // Same as for local certificates
                "CertificateName": "KeyVaultCert",
                "Credentials": [
                  { "Username": "azure-keyvault", "Password": "azure-keyvault" }                  
                ],

                // Azure specific configuration
                "Azure": {
                    "KeyVaultUrl": "", // The url to the azure keyvault like https://weu-000-keyvaultname.vault.azure.net/
                    "TenantId": "6333e2e8-47e9-46c1-ab8a-c9882a843742", // The ID of the tenant for accessing the keyvault
                    "CertificateName": "MyCodeSigningCert", // The name of the certificate in the key vault
                    "ClientId": "", // The client id for accessing the Key Vault (OAuth Client Credentias Grant flow)
                    "ClientSecret": "", // The client secret for accessing the Key Vault (OAuth Client Credentias Grant flow)
                    "ManagedIdentity": false // Whether to attempt using a managed identity for authentication
                }
            }
        ]
    }
}
```

> **Security Note** As of today the security is primarily given through the purpose of hosting server
> a local corporate network which is considered a trusted environment. The username/password mechanism
> is currently primarily intended for a simple certificate selection mechanism. Optional support for
> transport level security and bearer token authentication are under consideration.

### Client Usage Configuration

The client can be launched with
`SigningServer.Client.exe [--config config.json] <Files or Folders>`
You can specify multiple files or folders with multiple arguments.

The client is configured using a `config.json` which can also be changed through command line parameter. Call `SigningServer.Client.exe --help` for the full client documentation.
Under Linux use `dotnet SigningServer.Client.dll` instead of the executable. A simplified example config (not containing all options):

```json
{
    // The url where the signing server is hosted.
    "SigningServer": "http://hostname:port/",
    // The username and password to use for connecting to the server
    // and select the certificate
    "Username": "",
    "Password": "",

    // A flag indicating whether existing signatures should be overwritten
    "OverwriteSignatures": false,
    // Whether to accept existing signatures on the files or whether
    // the client should fail on files with existing signatures.
    "IgnoreExistingSignatures": false,
    // Whether to accept files which are not supported or whether
    // the client should fail on files which are not supported.
    "IgnoreUnsupportedFiles": false,
    // The maximum timeout before giving up on signing the file (in seconds)
    "Timeout": 300,
    // The hash algorithm to use when signing the file.
    // Supported algorithms might depend on the certificates used and 
    // the file formats (typical values: SHA1, SHA256, SHA386, SHA512)
    "HashAlgorithm": "SHA256",
    // How often to retry the signing operation until giving up. 
    "Retry": 1,
    // Allows to fully disable the signing performed
    // This is useful in CI/CD scenarios to keep the signing step as-is 
    // But control the signing via this flag
    "IsSigningDisabled": false
}
```

### Environment Variable Configuration

The client can also be configured using environment variables which can be useful in CI/CD environments.
The software uses the [ASP.net core configuration system](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-9.0#evcp) to allow specifying any configuration key through environment variables.

The environment variables are prefixed with `SIGNINGSERVER_CLIENT_`:

* `SIGNINGSERVER_CLIENT_Username=team01`
* `SIGNINGSERVER_CLIENT_IsSigningDisabled=true`

Be sure to use the latest signing client for this feature to be available.

#### Client Exit Codes

The client provides exit codes for each error scenario:

* `1` - Unexpected Error, Details should be in the log file.
* `2` - File not found.
* `3` - File is already signed (can be silenced through `IgnoreExistingSignatures`)
* `4` - Unsupported file format (can be silenced through `IgnoreUnsupportedFiles`)
* `5` - Invalid username or password provided
* `6` - Invalid configuration, check the json file for errors
* `7` - An error when communicating with the server occured
* `8` - Security Negotiation Failed

### Use Client as .net Tool
The signing client is published as [.net Tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) to nuget.org. 
You can install the signing client globally or locally (through a manifest). 

```
dotnet new tool-manifest
dotnet tool install SigningServer.Client
dotnet tool run signingserver-client <Arguments>
```

## License / Credits

Unless stated otherwise the code of this project is licensed under:

> Copyright (c) 2023 Daniel Kuschny and Contributors
> Licensed under the MIT license.
>
> [MIT license](LICENSE)

### SigningServer.Android

The code for Android signing is based on the apksig tool of the Android Open Source Project.

> Copyright (c) 2016, The Android Open Source Project
> Licensed under the Apache License, Version 2.0 (the "License");
> you may not use this file except in compliance with the License.
>
> [Apache License 2.0](SigningServer.Android/LICENSE).

It is ported for the Signing Server for proper integration with the certificates and signing mechanisms.

### SigningServer.ClickOnce

The code for ClickOnce signing is based on the MsBuild Source Code. Due to some limitations
on the original libraries, we had to adopt the source code. 

> The MIT License (MIT)
> 
> Copyright (c) .NET Foundation and contributors
> 
> All rights reserved.
> 
> Permission is hereby granted, free of charge, to any person obtaining a copy
> of this software and associated documentation files (the "Software"), to deal
> in the Software without restriction, including without limitation the rights
> to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
> copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be included in all
> copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
> FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
> AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
> LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
> SOFTWARE.

It is ported for the Signing Server for proper integration with the certificates and signing mechanisms.

### SigningServer.MsSign

While the code in this area is owned by this project, credit goes over to @vcsjones
and his efforts behind https://github.com/vcsjones/AzureSignTool
He did an amazing job reverse engineering the new undocumented `SignerSignEx3` function which is the heart behind the
signing mechanisms
provided by this library.

Read more about his analysis here: https://vcsjones.dev/azure-signtool/