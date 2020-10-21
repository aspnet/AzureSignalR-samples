package com.microsoft.signalr.androidchatroom.service;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;

import androidx.annotation.NonNull;

import com.google.android.gms.tasks.OnCanceledListener;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;
import com.google.firebase.iid.FirebaseInstanceId;
import com.google.firebase.iid.InstanceIdResult;
import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.windowsazure.messaging.NotificationHub;

import java.util.Arrays;
import java.util.UUID;
import java.util.concurrent.TimeUnit;


//  See https://docs.microsoft.com/en-us/azure/notification-hubs/notification-hubs-android-push-notification-google-fcm-get-started#test-send-notification-from-the-notification-hub
public class NotificationService extends Service {

    private static final String TAG = "NotificationService";
    private final String deviceUuid = UUID.randomUUID().toString();
    private String deviceToken;
    private String registrationId;
    private NotificationHub notificationHub;


    // Service binder
    private final IBinder notificationServiceBinder = new NotificationService.NotificationServiceBinder();
    public class NotificationServiceBinder extends Binder {
        public NotificationService getService() {
            return  NotificationService.this;
        }
    }

    @Override
    public IBinder onBind(Intent intent) {
        FirebaseInstanceId.getInstance().getInstanceId().addOnSuccessListener(new OnSuccessListener<InstanceIdResult>() {
            @Override
            public void onSuccess(InstanceIdResult instanceIdResult) {
                deviceToken = instanceIdResult.getToken();
                notificationHub = new NotificationHub(getString(R.string.azure_notification_hub_name),
                        getString(R.string.azure_notification_hub_connection_string), NotificationService.this);
                new Thread() {
                    @Override
                    public void run() {
                        try {
                            Log.d(TAG, String.format("Register with deviceToken: %s; tag: %s", deviceToken, deviceUuid));
                            registrationId = notificationHub.register(deviceToken, deviceUuid).getRegistrationId();
                        } catch (Exception e) {
                            Log.d(TAG, Arrays.toString(e.getStackTrace()));
                        }
                    }
                }.start();
            }
        });
        return notificationServiceBinder;
    }

    public String getDeviceToken() {
        return deviceToken;
    }

    public String getRegistrationId() {
        return registrationId;
    }

    public String getDeviceUuid() {
        return deviceUuid;
    }
}