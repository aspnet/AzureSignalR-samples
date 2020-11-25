package com.microsoft.signalr.androidchatroom.service;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.core.app.NotificationCompat;

import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;
import com.microsoft.signalr.androidchatroom.activity.MainActivity;

import java.util.Iterator;

/**
 * FirebaseService class
 * See https://docs.microsoft.com/en-us/azure/notification-hubs/notification-hubs-android-push-notification-google-fcm-get-started#test-send-notification-from-the-notification-hub
 */
public class FirebaseService extends FirebaseMessagingService {

    public static final String NOTIFICATION_CHANNEL_ID = "id_chatroom";
    public static final String NOTIFICATION_CHANNEL_NAME = "ChatRoom Channel";
    public static final String NOTIFICATION_CHANNEL_DESCRIPTION = "ChatRoom Channel Description";
    public static final int NOTIFICATION_ID = 1;
    private static final String TAG = "FirebaseService";
    private NotificationManager notificationManager;

    public static void createNotificationChannel(Context context) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    NOTIFICATION_CHANNEL_ID,
                    NOTIFICATION_CHANNEL_NAME,
                    NotificationManager.IMPORTANCE_HIGH);
            channel.setDescription(NOTIFICATION_CHANNEL_DESCRIPTION);
            channel.setShowBadge(true);

            NotificationManager notificationManager = context.getSystemService(NotificationManager.class);
            notificationManager.createNotificationChannel(channel);
        }
    }

    @Override
    public void onNewToken(@NonNull String s) {
        super.onNewToken(s);
    }

    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {
        Log.d(TAG, "From: " + remoteMessage.getFrom());

        /* Check if message contains a notification payload */
        String notificationTitle = null, notificationBody = null;
        if (remoteMessage.getNotification() != null) {
            Log.d(TAG, "Message Notification Body: " + remoteMessage.getNotification().getBody());
            notificationBody = remoteMessage.getNotification().getBody();
        } else {
            Iterator<String> dataPayloadIterator = remoteMessage.getData().values().iterator();
            notificationTitle = dataPayloadIterator.next();
            notificationBody = dataPayloadIterator.next();
        }

        /* When MainActivity is invisible, show notification */
        if (!MainActivity.getActiveInstance().isVisible()) {
            showNotification(notificationTitle, notificationBody);
        }

    }

    private void showNotification(String title, String body) {

        Intent intent = new Intent(this, MainActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);

        notificationManager = (NotificationManager)
                this.getSystemService(Context.NOTIFICATION_SERVICE);

        PendingIntent contentIntent = PendingIntent.getActivity(this, 0,
                intent, PendingIntent.FLAG_ONE_SHOT);

        NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(
                this,
                NOTIFICATION_CHANNEL_ID)
                .setContentText(body)
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setSmallIcon(android.R.drawable.ic_popup_reminder)
                .setBadgeIconType(NotificationCompat.BADGE_ICON_SMALL)
                .setAutoCancel(true);

        if (title != null) {
            notificationBuilder.setContentTitle(title);
        }

        notificationBuilder.setContentIntent(contentIntent);
        notificationManager.notify(NOTIFICATION_ID, notificationBuilder.build());
    }
}