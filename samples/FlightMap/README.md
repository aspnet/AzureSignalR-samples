# Flight Map: Realtime Monitoring Dashboard using Azure SignalR Service

This sample shows how to use Azure SignalR Service to build a realtime monitoring dashboard. Open the application, and you'll see flight locations in realtime.

A live demo can be found [here](https://signalr-flightmap-demo.azurewebsites.net/).

## How does it work?

In this sample, the data is generated in a web app by reading from a JSON file on Azure Blob storage. The web app then connects to the Azure SignalR Service and uses it to broadcast the data to all clients.

Here is a diagram that illustrates the application structure:

![flightmap](../../docs/images/flightmap.png)

In real world scenarios you can replace the web server and the blob storage with a component that generates actual live data.

## Deploy to Azure

1. Create a Bing Maps API for Enterprise on Azure and add your Bing map key to `index.html`:

    ```
    <script src='https://www.bing.com/api/maps/mapcontrol?callback=getMap&key=<bing_map_key>'
    ```

1.  Create a SignalR Service instance using [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest):

    ```
    az group create --name <resource_group_name> --location WestUS
    az signalr create --resource-group <resource_group_name> --name <signalr_name> --sku Standard_S1
    ```

1.  Create a web app using Azure CLI:

    ```
    az appservice plan create --name <plan_name> --resource-group <resource_group_name> --sku S1 --is-linux
    az webapp create \
        --resource-group <resource_group_name> --plan <plan_name> --name <app_name> \
        --runtime "dotnetcore|3.1"
    ```

1.  Deploy flight map to web app:

    a. Publish the app
       ```
       dotnet publish -c Release
       ```

    b. Package the published app into a zip file (or use whatever zip tool you like to do it)
       ```
       cd bin/Release/netcoreapp3.1/publish
       zip app.zip * -r
       ```

    c. Publish using Azure CLI
       ```
       az webapp deployment source config-zip -n <app_name> -g <resource_group_name> --src app.zip
       ```

1.  Prepare flight data

    This sample reads flight data from a JSON file, which is a two dimenstion array whose first dimension is time and second dimension is flight. Each element of the array is in the following format:

    ```json
    {
        "Icao": <unique_id>,
        "PosTime": <time>,
        "Lat": <latitude>,
        "Long": <longitude>
    }
    ```

    Here `Icao` is the unique ID of a flight and `PosTime` is the numeric value of a date time (milliseconds from midnight 1970/1/1).
    `Lat` and `Long` are latitude and longitude of the flight postion respectively.

    A simple data generator can be found in [generate.js](data/generate.js). You can use it to generate some random flight data from input time range, plane count, and coordinates. You can also write your own data generate or download real data from other websites (e.g. https://www.adsbexchange.com/). Then upload the flight data to an online storage (recommend to use Azure blob storage) so it can be referenced in the web app.

    Below commands creates an Azure storage account and upload the flight data to Azure blob:

    ```
    az storage account create --location WestUS --name <account_name> --resource-group <resource_group_name> --sku Standard_LRS
    az storage container create --account-name <account_name> --name <container_name> --public-access blob
    az storage blob upload --container-name <container_name> --account-name <account_name> -n data.json -f data.json
    ```

1.  Update settings of web app:

    ```
    az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
        --setting Azure__SignalR__ConnectionString=<connection_string>
    az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
        --setting AdminKey=<admin_key>
    az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
        --setting DataFileUrl=<data_file_url>
    ```

    Connection string and date file url can be get from the following commands:

    ```
    az signalr key list --resource-group <resource_group_name> --name <signalr_name>
    az storage blob url --container-name <container_name> --account-name <account_name> --name data.json
    ```

> There're different ways to deploy web app to Azure, please refer to [this doc](../../docs/azure-integration.md) for more details.

## How to run

Once everything is deployed, a URL will be available to you to test the application at:

    https://<app_name>.azurewebsites.net 

Open the homepage you'll see the planes moving on the map in realtime. The data originates from the `<data_file_url>` you set in the app settings and played repeatedly.

You can also use the following API endpoints to control the data:

1. `https://<web_app_url>/animation/<action>?key=<key>` to start/stop/restart the animation. Here `<action>` can be `start`, `stop`, and `restart`. `<key>` is the `<admin_key>` you set in the app settings.

2. `https://<web_app_url/animiation/setSpeed?speed=<speed>&key=<key>` to control the speed of the animation. Speed should be between 1 and 10.
