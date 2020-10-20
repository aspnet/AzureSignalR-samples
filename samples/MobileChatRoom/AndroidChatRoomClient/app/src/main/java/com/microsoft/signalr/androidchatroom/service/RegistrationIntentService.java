package com.microsoft.signalr.androidchatroom.service;

import android.app.IntentService;
import android.content.Intent;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.util.Log;

import com.google.firebase.iid.FirebaseInstanceId;
import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.windowsazure.messaging.NotificationHub;

import java.util.concurrent.TimeUnit;


//  See https://docs.microsoft.com/en-us/azure/notification-hubs/notification-hubs-android-push-notification-google-fcm-get-started#test-send-notification-from-the-notification-hub
public class RegistrationIntentService extends IntentService {

    private static final String TAG = "RegIntentService";
    String FCM_token = null;

    private NotificationHub hub;

    public RegistrationIntentService() {
        super(TAG);
    }

    @Override
    protected void onHandleIntent(Intent intent) {

        SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(this);
        String resultString = null;
        String regID = null;
        String storedToken = null;

        try {
            FirebaseInstanceId.getInstance().getInstanceId().addOnSuccessListener(instanceIdResult -> {
                FCM_token = instanceIdResult.getToken();
                Log.d(TAG, "FCM Registration Token: " + FCM_token);
            });
            TimeUnit.SECONDS.sleep(1);

            // Storing the registration ID that indicates whether the generated token has been
            // sent to your server. If it is not stored, send the token to your server.
            // Otherwise, your server should have already received the token.
            if (((regID = sharedPreferences.getString("registrationID", null)) == null)) {

                NotificationHub hub = new NotificationHub(getString(R.string.azure_notification_hub_name),
                        getString(R.string.azure_notification_hub_connection_string), this);
                Log.d(TAG, "Attempting a new registration with NH using FCM token : " + FCM_token);
                regID = hub.register(FCM_token).getRegistrationId();

                // If you want to use tags...
                // Refer to : https://azure.microsoft.com/documentation/articles/notification-hubs-routing-tag-expressions/
                // regID = hub.register(token, "tag1,tag2").getRegistrationId();

                resultString = "New NH Registration Successfully - RegId : " + regID;
                Log.d(TAG, resultString);

                sharedPreferences.edit().putString("registrationID", regID).apply();
                sharedPreferences.edit().putString("FCMtoken", FCM_token).apply();
            }

            // Check to see if the token has been compromised and needs refreshing.
            else if (!(storedToken = sharedPreferences.getString("FCMtoken", "")).equals(FCM_token)) {

                NotificationHub hub = new NotificationHub(getString(R.string.azure_notification_hub_name),
                        getString(R.string.azure_notification_hub_connection_string), this);
                Log.d(TAG, "NH Registration refreshing with token : " + FCM_token);
                regID = hub.register(FCM_token).getRegistrationId();

                // If you want to use tags...
                // Refer to : https://azure.microsoft.com/documentation/articles/notification-hubs-routing-tag-expressions/
                // regID = hub.register(token, "tag1,tag2").getRegistrationId();

                resultString = "New NH Registration Successfully - RegId : " + regID;
                Log.d(TAG, resultString);

                sharedPreferences.edit().putString("registrationID", regID).apply();
                sharedPreferences.edit().putString("FCMtoken", FCM_token).apply();
            }
        } catch (Exception e) {
            Log.e(TAG, "Failed to complete registration", e);
            // If an exception happens while fetching the new token or updating registration data
            // on a third-party server, this ensures that we'll attempt the update at a later time.
        }
    }
}