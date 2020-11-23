package com.microsoft.signalr.androidchatroom.activity;

import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.os.Bundle;
import android.os.IBinder;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;

import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.service.FirebaseService;
import com.microsoft.signalr.androidchatroom.service.NotificationService;
import com.microsoft.signalr.androidchatroom.view.ChatFragment;
import com.microsoft.signalr.androidchatroom.view.LoginFragment;

public class MainActivity extends AppCompatActivity {
    private static final String TAG = "MainActivity";

    public static MainActivity mainActivity;
    
    // Used for notification display
    // Display notification when MainActivity is not visible
    public static Boolean isVisible = false;

    // View components
    private LoginFragment mLoginFragment;
    private ChatFragment mChatFragment;

    // Notification service and service connection
    private NotificationService notificationService;
    private final ServiceConnection notificationServiceConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName name, IBinder service) {
            NotificationService.NotificationServiceBinder notificationServiceBinder = (NotificationService.NotificationServiceBinder) service;
            notificationService = notificationServiceBinder.getService();
            mLoginFragment.setDeviceUuid(notificationService.getDeviceUuid());
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

        bindNotificationService();
        FirebaseService.createNotificationChannel(getApplicationContext());
    }

    public NotificationService getNotificationService() {
        return notificationService;
    }

    public void bindNotificationService() {
        Intent intent = new Intent(this, NotificationService.class);
        bindService(intent, notificationServiceConnection, Context.BIND_AUTO_CREATE);
    }

    @Override
    public void onBackPressed() {
        // If back pressed, manually logout user
        if (mChatFragment != null) {
            mChatFragment.onBackPressed();
            mChatFragment = null;
        }
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

    public void setLoginFragment(LoginFragment loginFragment) {
        this.mLoginFragment = loginFragment;
    }

    public void setChatFragment(ChatFragment chatFragment) {
        this.mChatFragment = chatFragment;
    }
}