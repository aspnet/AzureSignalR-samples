package com.microsoft.signalr.androidchatroom.activity;

import android.content.ComponentName;
import android.content.Context;
import android.content.ServiceConnection;
import android.os.Bundle;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.service.ChatService;
import com.microsoft.signalr.androidchatroom.service.NotificationService;
import com.microsoft.signalr.androidchatroom.service.SignalRChatService;
import com.microsoft.signalr.androidchatroom.service.FirebaseService;

import android.content.Intent;
import android.os.IBinder;
import android.widget.Toast;

public class MainActivity extends AppCompatActivity {
    private static final String TAG = "MainActivity";

    public static MainActivity mainActivity;
    public static Boolean isVisible = false;


    private ChatService chatService;
    private final ServiceConnection chatServiceConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName name, IBinder service) {
            SignalRChatService.ChatServiceBinder chatServiceBinder = (SignalRChatService.ChatServiceBinder) service;
            chatService = chatServiceBinder.getService();
        }

        @Override
        public void onServiceDisconnected(ComponentName name) {
            chatService = null;
        }
    };

    private NotificationService notificationService;
    private final ServiceConnection notificationServiceConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName name, IBinder service) {
            NotificationService.NotificationServiceBinder notificationServiceBinder = (NotificationService.NotificationServiceBinder) service;
            notificationService = notificationServiceBinder.getService();
        }

        @Override
        public void onServiceDisconnected(ComponentName name) {
            notificationService = null;
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        mainActivity = this;
        setContentView(R.layout.activity_main);
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        bindChatService();
        bindNotificationService();
        FirebaseService.createChannelAndHandleNotifications(getApplicationContext());
    }

    public ChatService getChatService() {
        return chatService;
    }

    public NotificationService getNotificationService() {
        return notificationService;
    }

    public void bindChatService() {
        Intent intent = new Intent(this, SignalRChatService.class);
        bindService(intent, chatServiceConnection, Context.BIND_AUTO_CREATE);
    }

    public void bindNotificationService() {
        Intent intent = new Intent(this, NotificationService.class);
        bindService(intent, notificationServiceConnection, Context.BIND_AUTO_CREATE);
    }

    @Override
    public void onBackPressed() {
        chatService.expireSession(false);
        super.onBackPressed();
    }

    @Override
    protected void onStart() {
        super.onStart();
        isVisible = true;
    }

    @Override
    protected void onPause() {
        super.onPause();
        isVisible = false;
    }

    @Override
    protected void onResume() {
        super.onResume();
        isVisible = true;
    }

    @Override
    protected void onStop() {
        super.onStop();
        isVisible = false;
    }

    public void ToastNotify(final String notificationMessage) {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                Toast.makeText(MainActivity.this, notificationMessage, Toast.LENGTH_LONG).show();
            }
        });
    }
}