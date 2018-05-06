# Flight Map: Realtime Monitoring Dashboard using Azure SignalR Service

This sample shows how to use Azure SignalR Service to build a realtime monitoring dashboard. Open the application, you'll see flight locations in realtime.

A live demo can be found [here](http://flightmap-demo1.azurewebsites.net/).

## How Does It Work

In this sample, the data is generated on a web app, by reading from a JSON file on Azure blob. The web app then connects to the Azure SignalR Service and use it broadcast the data to all clients.

Here is a diagram that illustrates the application structure:

![flightmap](../../docs/images/flightmap.png)

In real world scenarios you can replace the web server and the blob storage with a component that generates the real data.

## Deploy to Azure

1.  Create a SignalR Service in Azure portal.

2.  Create a web app:
    ```
    az group create --name <resource_group_name> --location CentralUS
    az appservice plan create --name <plan_name> --resource-group <resource_group_name> --sku S1 --is-linux
    az webapp create \
       --resource-group <resource_group_name> --plan <plan_name> --name <app_name> \
       --runtime "DOTNETCORE|2.0"
    ```

3.  Config deployment source and credential:
    ```
    az webapp deployment source config-local-git --resource-group <resource_group_name> --name <app_name>
    az webapp deployment user set --user-name <user_name> --password <password>
    ```

4.  Add your bing map key to `index.html`:

    ```
    <script src='https://www.bing.com/api/maps/mapcontrol?callback=getMap&key=<bing_map_key>'
    ```

    Then deploy using git:
    ```
    git init
    git remote add origin <deploy_git_url>
    git add -A
    git commit -m "init commit"
    git push origin master
    ```

5.  Update config
    ```
    az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
       --setting DataFileUrl=<data_file_url>
    az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
       --setting Azure__SignalR__ConnectionString=<connection_string>
    az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
       --setting AdminKey=<admin_key>
    ```

## How to Play

Open the homepage you'll see the planes moving on the map in realtime. The data is from the `<data_file_url>` you set in the app settings and played repeatedly.

You can also use the following APIs to control the data:

1. `https://<web_app_url>/animation/<action>?key=<key>` to start/stop/restart the animation. Here `<action>` can be `start`, `stop`, and `restart`. `<key>` is the `<admin_key>` you set in the app settings.

2. `https://<web_app_url/animiation/setSpeed?speed=<speed>&key=<key>` to control the speed of the animation. Speed should be between 1 and 10.
