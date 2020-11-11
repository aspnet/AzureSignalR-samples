# Build SignalR-based Android Chatting App

This tutorial shows you how to build and modify a BSignalR-based Android Chatting App. You'll learn how to:

> **&#x2713;** Build a mobile chat room client with SignalR client and Android Studio.
>
> **&#x2713;** Integrate the chat room app with [ReliableChatRoom Server](https://github.com/UncooleBen/AzureSignalR-samples/tree/master/samples/ReliableChatRoom)
>
> **&#x2713;** Chat with the mobile app.

## Prerequisites
* Install [.NET Core 3.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0) (Version >= 3.0.100)
* Install [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) (Version >= 16.3)
* Install [Android Studio](https://developer.android.com/studio) (We use ver. 4.1)


## Build A Mobile Chat Room Client App With SignalR Library and Android Studio

0. Download or clone the Android Studio project
   
   ```cmd
   git clone https://github.com/uncooleben/AzureSignalR-samples.git
   ```

1. Open the directory as project in Android Studio

    Open Android Studio -> Open an Existing Project

    ![1-open-project](./assets/1-open-project.png)

    Select the project directory -> OK

    ![2-open-project](./assets/2-open-project.png)

2. Build the Android application

    Hit the hammer button

    ![3-build](./assets/3-build.png)

3. Download and place `google-services.json`

    1. In [Firebase Console](https://console.firebase.google.com/) -> Click your project

    2. In `Settings` -> `Project Settings` -> Download `google-services.json` -> Copy it to `AzureSignalR-samples\samples\MobileChatRoom\AndroidChatRoomClient\app\google-services.json`

## Chat With Mobile Chat Room App

1. Build and run your app server
    
    In your app server project directory
    ```cmd 
    cd AzureSignalR-samples\samples\ReliableChatRoom\ReliableChatRoom
    dotnet run
    ```


2. Launch Android Chat Room Clients

    1. Create two AVD in the Android Emulator

    2. Run the app on multiple clients

    3. Start chatting

