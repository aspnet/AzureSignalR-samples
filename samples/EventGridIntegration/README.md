# Sample: Azure SignalR Service integration with Event Grid and Azure Function

A step by step tutorial to build a chat room with real-time online counting using Azure Functions, Event Grid, App Service Authentication, and SignalR Service.

## Introduction

### Prerequisites

The following software is required to build this tutorial.

* [Git](https://git-scm.com/downloads)
* [Node.js](https://nodejs.org/en/download/) (Version 10.x)
* [.NET SDK](https://www.microsoft.com/net/download) (Version 2.x, required for Functions extensions)
* [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools) (Version 2)
* [Visual Studio Code](https://code.visualstudio.com/) (VS Code) with the following extensions
* [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions) - work with Azure Functions in VS Code
* [Live Server](https://marketplace.visualstudio.com/items?itemName=ritwickdey.LiveServer) - serve web pages locally for testing

## Create an Azure SignalR Service instance

You will build and test the Azure Functions app locally. The app will access a SignalR Service instance in Azure that needs to be created ahead of time.

1. Please take the [tutorial](https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-quickstart-azure-functions-javascript#create-an-azure-signalr-service-instance) in document to create a
Azure SignalR Service instance.

1. After the instance is deployed, open it in the portal and locate its Settings page. Change the Service Mode setting to **Serverless**.

## Create a Storage account

An Azure Storage account is required by a function app using Event Grid trigger. You will also host the web page for the chat UI using the static websites feature of Azure Storage if you try to deploy the application to Azure.

1. In the Azure portal, click on the **Create a resource** (**+**) button for creating a new Azure resource.

1. Select the **Storage** category, then select **Storage account**.

1. Enter the following information.

    | Name | Value |
    |---|---|
    | Subscription | Select the subscription containing the SignalR Service instance |
    | Resource group | Select the same resource group |
    | Resource name | A unique name for the Storage account |
    | Location | Select the same location as your other resources |
    | Performance | Standard |
    | Account kind | StorageV2 (general purpose V2) |
    | Replication | Locally-redundant storage (LRS) |
    | Access Tier | Hot |

1. Click **Review + create**, then **Create**.

## Initialize the function app

### Configure application settings

When running and debugging the Azure Functions runtime locally, application settings are read from **local.settings.json**. Update this file with the connection string of the SignalR Service instance that you created earlier.

1. In the root folder of project, create a file named **local.settings.json** and open it.

1. Replace the file's contents with the following.

    ```json
    {
      "IsEncrypted": false,
      "Values": {
        "AzureSignalRConnectionString": "<signalr-connection-string>",
        "WEBSITE_NODE_DEFAULT_VERSION": "10.14.1",
        "FUNCTIONS_WORKER_RUNTIME": "node",
        "AzureWebJobsStorage": "<Azure-storage-connection-string>",
        "AZURE_STORAGE_CONNECTION_STRING": "<Azure-storage-connection-string>"
      },
      "Host": {
        "LocalHttpPort": 7071,
        "CORS": "http://127.0.0.1:5500",
        "CORSCredentials": true
      }
    }
    ```

   * Enter the Azure SignalR Service connection string into a setting named `AzureSignalRConnectionString`. Obtain the value from the **Keys** page in the Azure SignalR Service resource in the Azure portal; either the primary or secondary connection string can be used.
   * The `WEBSITE_NODE_DEFAULT_VERSION` setting is not used locally, but is required when deployed to Azure.
   * The `Host` section configures the port and CORS settings for the local Functions host (this setting has no effect when running in Azure).
   * The `AzureWebJobsStorage` is used by Event Grid trigger and the `AZURE_STORAGE_CONNECTION_STRING` is used by storage client in codes. Either using the same or using separate one is fine.

       > [!NOTE]
       > Live Server is typically configured to serve content from `http://127.0.0.1:5500`. If you find that it is using a different URL or you are using a different HTTP server, change the `CORS` setting to reflect the correct origin.

     ![Get SignalR Service key](media/signalr-get-key.png)

1. Save the file.

## Build and run the sample locally

### Create an ngrok endpoint

When running Event Grid trigger locally, you need a tool to proxy events to your local endpoint like [ngrok](https://ngrok.com/).

Download *ngrok.exe* from [ngrok](https://ngrok.com/), and run with the following command:

```
ngrok http -host-header=localhost 7071
```

The -host-header parameter is needed because the functions runtime expects requests from localhost when it runs on localhost. 7071 is the default port number when the runtime runs locally.

The command creates output like the following:

```
Session Status                online
Version                       2.2.8
Region                        United States (us)
Web Interface                 http://127.0.0.1:4040
Forwarding                    http://263db807.ngrok.io -> localhost:7071
Forwarding                    https://263db807.ngrok.io -> localhost:7071

Connections                   ttl     opn     rt1     rt5     p50     p90
                              0       0       0.00    0.00    0.00    0.00
```

You'll use the `https://{subdomain}.ngrok.io` URL for your Event Grid subscription.

### Run the Event Grid trigger function

The ngrok URL doesn't get special handling by Event Grid, so your function **must be running** locally when the subscription is created. If it isn't, the validation response doesn't get sent and the subscription creation fails.

### Create a subscription

Create an Event Grid subscription of SignalR Service, and give it your ngrok endpoint.

Use this endpoint pattern for Functions 2.x:

```
https://{SUBDOMAIN}.ngrok.io/runtime/webhooks/eventgrid?functionName={FUNCTION_NAME}
```

The `{FUNCTION_NAME}` parameter must be the name specified in the `FunctionName` attribute.

Here's an example to integrate with Azure SignalR Service using the Azure CLI:

```azurecli
az eventgrid event-subscription create --resource-id <signalr-service-resource-id> --name <event-grid-subscription-name> --endpoint https://263db807.ngrok.io/runtime/webhooks/eventgrid?functionName=OnConnection
```

## Run the client page and test

The chat application's UI is a simple single page application (SPA) created with the Vue JavaScript framework. It will be hosted separately from the function app. Locally, you will run the web interface using the Live Server VS Code extension.

1. Press **F5** to run the function app locally and attach a debugger.

1. With **index.html** open, start Live Server by opening the VS Code command palette (`Ctrl-Shift-P`, macOS: `Cmd-Shift-P`) and selecting **Live Server: Open with Live Server**. Live Server will open the application in a browser.

1. The application opens. You will get a welcome message from `Function` and real-time connected connection counting. Also, you can broadcast message in the chat sample.

## Deploy to Azure and enable authentication

You have been running the function app and chat application locally. You will now deploy them to Azure and enable authentication and private messaging in the application.

### Configure static websites

1. After the Storage account is created, open it in the Azure portal.

1. Select **Static website**.

1. Select **Enabled** to enable the static website feature.

1. In **Index document name**, enter *index.html*.

1. Click **Save**.

1. A **Primary endpoint** appears. Note this value. It will be required to configure the function app.

### Deploy function app to Azure

1. Open the VS Code command palette (`Ctrl-Shift-P`, macOS: `Cmd-Shift-P`) and select **Azure Functions: Deploy to Function App**.

1. When prompted, provide the following information.

    | Name | Value |
    |---|---|
    | Folder to deploy | Select the main project folder |
    | Subscription | Select your subscription |
    | Function app | Select **Create New Function App** |
    | Function app name | Enter a unique name |
    | Resource group | Select the same resource group as the SignalR Service instance |
    | Storage account | Select the storage account you created earlier |

    A new function app is created in Azure and the deployment begins. Wait for the deployment to complete.

### Upload function app local settings

1. Open the VS Code command palette (`Ctrl-Shift-P`, macOS: `Cmd-Shift-P`).

1. Search for and select the **Azure Functions: Upload local settings** command.

1. When prompted, provide the following information.

    | Name | Value |
    |---|---|
    | Local settings file | local.settings.json |
    | Subscription | Select your subscription |
    | Function app | Select the previously deployed function app |

Local settings are uploaded to the function app in Azure. If prompted to overwrite existing settings, select **Yes to all**.

### Subscribe Azure SignalR events

Different from running Event Grid trigger locally, it's much easier to subscribe and handle events in Azure.

1. Open the VS Code command palette (`Ctrl-Shift-P`, macOS: `Cmd-Shift-P`).

1. Search for and select the **Azure Functions: Open in portal** command.

1. Select the Function `OnConnection` in the left panel. After the function shown, click `Add Event Grid subscription` and choose the Azure SignalR Service.
    ![Subscribe Azure SignalR Service events](media/signalr-event-grid-subscribe.png)

### Enable App Service Authentication

App Service Authentication supports authentication with Azure Active Directory, Facebook, Twitter, Microsoft account, and Google.

1. In the function app that was opened in the portal, locate the **Platform features** tab, select **Authentication/Authorization**.

1. Turn **On** App Service Authentication.

1. In **Action to take when request is not authenticated**, select "Allow Anonymous requests (no action)".

1. In **Allowed External Redirect URLs**, enter the URL of your storage account primary web endpoint that you previously noted.

1. Follow the documentation for the login provider of your choice to complete the configuration.

    - [Azure Active Directory](https://docs.microsoft.com/azure/app-service/configure-authentication-provider-aad)
    - [Facebook](https://docs.microsoft.com/azure/app-service/configure-authentication-provider-facebook)
    - [Twitter](https://docs.microsoft.com/azure/app-service/configure-authentication-provider-twitter)
    - [Microsoft account](https://docs.microsoft.com/azure/app-service/configure-authentication-provider-microsoft)
    - [Google](https://docs.microsoft.com/azure/app-service/configure-authentication-provider-google)

### Update the web app

1. In the Azure portal, navigate to the function app's overview page.

1. Copy the function app's URL.

1. In VS Code, open **index.html** and replace the value of `apiBaseUrl` with the function app's URL.

1. The application can be configured with authentication using Azure Active Directory, Facebook, Twitter, Microsoft account, or Google. Select the authentication provider that you will use by setting the value of `authProvider`.

1. Save the file.

### Deploy the web application to blob storage

The web application will be hosted using Azure Blob Storage's static websites feature.

1. Open the VS Code command palette (`Ctrl-Shift-P`, macOS: `Cmd-Shift-P`).

1. Search for and select the **Azure Storage: Deploy to Static Website** command.

1. Enter the following values:

    | Name | Value |
    |---|---|
    | Subscription | Select your subscription |
    | Storage account | Select the storage account you created earlier |
    | Folder to deploy | Select **Browse** and select the *content* folder |

The files in the *content* folder should now be deployed to the static website.

### Enable function app cross origin resource sharing (CORS)

Although there is a CORS setting in **local.settings.json**, it is not propagated to the function app in Azure. You need to set it separately.

1. Open the function app in the Azure portal.

1. Under the **Platform features** tab, select **CORS**.

1. In the *Allowed origins* section, add an entry with the static website *primary endpoint* as the value (remove the trailing */*).

1. In order for the SignalR JavaScript SDK call your function app from a browser, support for credentials in CORS must be enabled. Select the "Enable Access-Control-Allow-Credentials" checkbox.

1. Click **Save** to persist the CORS settings.

### Try the application

1. In a browser, navigate to the storage account's primary web endpoint.

1. Select **Login** to authenticate with your chosen authentication provider.

1. When you're connected, the online connection count is shown and you will get a welcome message.

1. Send public messages by entering them into the main chat box.

1. Send private messages by clicking on a username in the chat history. Only the selected recipient will receive these messages.

![Overview of the application](media/overview.png)