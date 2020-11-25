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

/*
 * Main entrance of the application.
 */
public class MainActivity extends AppCompatActivity {
    private static final String TAG = MainActivity.class.getSimpleName();

    /*
     * Used for notification display
     * Display notification when MainActivity is not visible
     */
    private static MainActivity sMainActivity;
    private boolean mIsVisible = false;

    /* View components */
    private LoginFragment mLoginFragment;
    private ChatFragment mChatFragment;

    /* Instance of NotificationService */
    private NotificationService notificationService;
    /* Service connection that get the ref of NotificationService object */
    private final ServiceConnection notificationServiceConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName name, IBinder service) {
            NotificationService.NotificationServiceBinder notificationServiceBinder =
                    (NotificationService.NotificationServiceBinder) service;
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
        sMainActivity = this;
        setContentView(R.layout.activity_main);
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        bindNotificationService();
        FirebaseService.createNotificationChannel(getApplicationContext());
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
        mIsVisible = true;
    }

    @Override
    protected void onPause() {
        super.onPause();
        mIsVisible = false;
    }

    @Override
    protected void onResume() {
        super.onResume();
        mIsVisible = true;
    }

    @Override
    protected void onStop() {
        super.onStop();
        mIsVisible = false;
    }

    public void setLoginFragment(LoginFragment loginFragment) {
        this.mLoginFragment = loginFragment;
    }

    public void setChatFragment(ChatFragment chatFragment) {
        this.mChatFragment = chatFragment;
    }

    public static MainActivity getActiveInstance() {
        return sMainActivity;
    }

    public NotificationService getNotificationService() {
        return notificationService;
    }

    public boolean isVisible() {
        return mIsVisible;
    }
}