# Data IO Extension App Reference Implementation
## Introduction
This reference implementation demonstrates data input and output functionality for an [extension app](https://developers.docusign.com/extension-apps/).

To test a read-only data IO extension app, modify one of the `ReadOnlyManifest.json` files.

To test a data IO extension app with both read and write capabilities, modify one of the `ReadWriteManifest.json` files.

## Authentication
This reference implementation supports two [authentication](https://developers.docusign.com/extension-apps/build-an-extension-app/it-infrastructure/authorization/) flows:
* [Authorization Code Grant](https://developers.docusign.com/extension-apps/build-an-extension-app/it-infrastructure/authorization/#authorization-code-grant) – required for public extension apps
* [Client Credentials Grant](https://developers.docusign.com/extension-apps/build-an-extension-app/it-infrastructure/authorization/#client-credentials-grant) – available to private extension apps. See [Choosing private distribution instead of public](https://developers.docusign.com/extension-apps/extension-apps-101/choosing-private-distribution/)

*Private extension apps can use either authentication method, but public extension apps must use Authorization Code Grant.*

```text
manifests/
    authorizationCode/
        ReadOnlyManifest.json
        ReadWriteManifest.json
    clientCredentials/
        ReadOnlyManifest.json
        ReadWriteManifest.json
    hosted/
        authorizationCode.ReadOnlyManifest.json
        authorizationCode.ReadWriteManifest.json
        clientCredentials.ReadOnlyManifest.json
        clientCredentials.ReadWriteManifest.json
```

## Local setup instructions

### Video Walkthrough
[![Reference implementation videos](https://img.youtube.com/vi/_4p7GWK5aoA/0.jpg)](https://youtube.com/playlist?list=PLXpRTgmbu4orBQrYWPAXa4EBXv0IGGzID&feature=shared)

### 1. Clone the repository
Run the following command to clone the repository: 
```bash
git clone https://github.com/docusign/extension-app-data-io-reference-implementation-csharp.git
```

### 2. Generate secret values
- If you already have values for `JwtSecretKey`, `OAuthClientId`, `OAuthClientSecret`, and `AuthorizationCode`, you may skip this step.

The easiest way to generate a secret value is to run the following command:

#### PowerShell
```bash
[Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
```

#### OpenSSL (macOS/Linux)
```bash
openssl rand -hex 64
```

Each command generates a 128-character hexadecimal string (64 random bytes) that can be used as a secret value. Generate separate values for `JwtSecretKey`, `OAuthClientId`, `OAuthClientSecret`, and `AuthorizationCode`.

### 3. Configure appsettings for the cloned repository
- If you're running this in a development environment, create a copy of `appsettings.Development.json` and save it as `appsettings.json`.
- Replace `JwtSecretKey`, `OAuthClientId`, `OAuthClientSecret`, and `AuthorizationCode` in `appsettings.json` with your generated values. These values will be used to configure the sample proxy's mock authentication server.
- Set the `clientId` value in the manifest file to the same value as `OAuthClientId`.
- Set the `clientSecret` value in the manifest file to the same value as `OAuthClientSecret`.
### 4. [Install and configure .NET 10 on your machine.](https://dotnet.microsoft.com/en-us/download)
### 5. Running the proxy server
#### Development mode:
Start the proxy server in development mode by running the command:
```bash
dotnet run
```

This will create a local server on the port 5245 by default.

## Setting up ngrok
### 1. [Install and configure ngrok for your machine.](https://ngrok.com/docs/getting-started/)
### 2. Start ngrok
Run the following command to create a publicly accessible tunnel to your localhost:

```bash
ngrok http 5245
```

### 3. Save the forwarding address
Copy the `Forwarding` address from the response. You’ll need this address in your manifest file.

```bash
ngrok                                                    

Send your ngrok traffic logs to Datadog: https://ngrok.com/blog-post/datadog-log

Session Status                online
Account                       email@domain.com (Plan: Free)
Update                        update available (version 3.3.1, Ctrl-U to update)
Version                       3.3.0
Region                        United States (us)
Latency                       60ms
Web Interface                 http://127.0.0.1:4040
Forwarding                    https://bbd7-12-202-171-35.ngrok-free.app -> http:

Connections                   ttl     opn     rt1     rt5     p50     p90
                              0       0       0.00    0.00    0.00    0.00
```

In this example, the `Forwarding` address to copy is `https://bbd7-12-202-171-35.ngrok-free.app`.
## Create an extension app
### 1. Prepare your app manifest
Replace `<PROXY_BASE_URL>` in your manifest file with the ngrok forwarding address in the following sections:
- `connections.params.customConfig.tokenUrl` in all manifests
- `connections.params.customConfig.authorizationUrl` in the authorization code manifests
- `actions.params.uri` for all actions

### 2. Navigate to the [Developer Console](https://devconsole.docusign.com/apps)
Log in with your Docusign developer credentials and create a new app.

### 3. Upload your manifest and create the data IO app
To [create your extension app](https://developers.docusign.com/extension-apps/build-an-extension-app/create/), open the [Developer Console](https://devconsole.docusign.com/apps) and select **+New App.** In the app manifest editor that opens, upload your manifest file or paste into the editor itself; then select Validate. Once the editor validates your manifest, select **Create App.** 

### 4. Test the extension app
This reference implementation uses mock data to simulate how data can be verified against a database. [Test your extension](https://developers.docusign.com/extension-apps/build-an-extension-app/test/) using the sample data in [ExtensionAppDataIO/Data/MockDb](ExtensionAppDataIO/Data/MockDb). Extension app tests include [integration tests](https://developers.docusign.com/extension-apps/build-an-extension-app/test/integration-tests/) (connection tests and extension tests), [functional tests](https://developers.docusign.com/extension-apps/build-an-extension-app/test/functional-tests/), and [App Center preview](https://developers.docusign.com/extension-apps/build-an-extension-app/test/app-center-preview/).


### Extension tests
The Developer Console offers five extension tests to verify that a data IO extension app can connect to and exchange data with third-party APIs (or an API proxy that in turn connects with those APIs). 

**Note:** These instructions only apply if you use the mock data in [ExtensionAppDataIO/Data/MockDb](ExtensionAppDataIO/Data/MockDb). If you use your own database, you’ll need to construct your requests based on your own schema. Queries for extension tests in the Developer Console are built using the [IQuery](https://developers.docusign.com/extension-apps/extension-app-reference/extension-contracts/custom-query-language/) structure.


#### CreateRecord extension test
To begin the extension test process, run the CreateRecord test using the sample query below. The test should return a response containing the record ID.

```json
{
  "typeName": "Account",
  "idempotencyKey": "NOT_USED_CURRENTLY",
  "data": {
    "Name": "Test Account",
    "ShippingLatitude": 10,
    "PushCount": 6      
  }
}
```

![CreateRecord Test](https://github.com/user-attachments/assets/f962d007-2cab-49ce-a032-472cd214478d)


All record types are located in the `ExtensionAppDataIO/Data/MockDb/` folder of this repository.

![db folder](https://github.com/user-attachments/assets/06449adc-057e-44c8-a76e-8406b29ae13e)


Open `ExtensionAppDataIO/Data/MockDb/Account.json` and check that the records were created.

![Account.json](https://github.com/user-attachments/assets/37cca0e3-9113-4e01-a710-514b16763dbe)


#### SearchRecords extension test
This query searches the records that have been created. You do not have to use the same sample values used here; the search should work with a valid attribute in `Account.json`.

Open the SearchRecords test and create a new query based on `Account.json`:

- The `from` attribute maps to the value of `typeName` in the CreateRecord query; in this case, `Account`.
- The `data` object from the CreateRecord query maps to the `attributesToSelect` array; in this case, `Name`.
- The `name` property of the `leftOperand` object should be the value of `Name`; in this case, `Test Account`.
- The `operator` value should be `EQUALS`.
- The `name` property of the `rightOperand` object should be the same as what's in `attributesToSelect` array; in this case, `Name`.

The query below has been updated based on the directions above. You can copy and paste this into the SearchRecords test input box.

```json
{
    "query": {
        "$class": "com.docusign.connected.data.queries@1.0.0.Query",
        "attributesToSelect": [
            "Name"
        ],
        "from": "Account",
        "queryFilter": {
            "$class": "com.docusign.connected.data.queries@1.0.0.QueryFilter",
            "operation": {
                "$class": "com.docusign.connected.data.queries@1.0.0.ComparisonOperation",
                "leftOperand": {
                    "$class": "com.docusign.connected.data.queries@1.0.0.Operand",
                    "name": "Test Account",
                    "type": "STRING",
                    "isLiteral": true
                },
                "operator": "EQUALS",
                "rightOperand": {
                    "$class": "com.docusign.connected.data.queries@1.0.0.Operand",
                    "name": "Name",
                    "type": "INTEGER",
                    "isLiteral": false
                }
            }
        }
    },
    "pagination": {
        "limit": 10,
        "skip": 10
    }
}
```

Running the test will return the record you queried.

![new search records](https://github.com/user-attachments/assets/4e6e26e2-040b-43a1-b20c-f69c84bcf765)




#### PatchRecord extension test
The `recordId` property in the sample input maps to an `Id` in `Account.json`. Any valid record ID can be used in this field.

In the `data` object, include any attributes and values to be added to the record. In this query, a new property will be added, and the original data in the record will be updated.

```json
{
  "recordId": "2",
  "typeName": "Account",
  "idempotencyKey": "NOT_USED_CURRENTLY",
  "data": {
    "Name": "updatedTestAccount",
    "ShippingLatitude": 11,
    "PushCount": 7,
    "MasterRecordId": "ABCD"
  }
}
```

Running the test should return the response `"success": true`.

![PatchRecord test](https://github.com/user-attachments/assets/e445b06b-6790-475a-8434-c0eea1e003b3)


Rerun the SearchRecords extension test to search for the new patched values. 

**Input query:**

```json
{
    "query": {
        "$class": "com.docusign.connected.data.queries@1.0.0.Query",
        "attributesToSelect": [
            "Name"
        ],
        "from": "Account",
        "queryFilter": {
            "$class": "com.docusign.connected.data.queries@1.0.0.QueryFilter",
            "operation": {
                "$class": "com.docusign.connected.data.queries@1.0.0.ComparisonOperation",
                "leftOperand": {
                    "$class": "com.docusign.connected.data.queries@1.0.0.Operand",
                    "name": "updatedTestAccount",
                    "type": "STRING",
                    "isLiteral": true
                },
                "operator": "EQUALS",
                "rightOperand": {
                    "$class": "com.docusign.connected.data.queries@1.0.0.Operand",
                    "name": "Name",
                    "type": "INTEGER",
                    "isLiteral": false
                }
            }
        }
    },
    "pagination": {
        "limit": 10,
        "skip": 10
    }
}
```

**Results:**

![Results of SearchRecords after PatchRecord](https://github.com/user-attachments/assets/70dbce23-0c9d-4150-ab25-c853e92d695f)


Alternatively, `Account.json` will contain the updated records.

![Account.json after PatchRecord test](https://github.com/user-attachments/assets/ace8276b-2d36-4171-a598-2450e6d9b5fe)
