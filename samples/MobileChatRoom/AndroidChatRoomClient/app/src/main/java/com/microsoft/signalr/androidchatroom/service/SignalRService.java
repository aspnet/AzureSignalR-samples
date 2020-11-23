package com.microsoft.signalr.androidchatroom.service;

import android.util.Log;

import com.microsoft.signalr.Action1;
import com.microsoft.signalr.Action2;
import com.microsoft.signalr.Action3;
import com.microsoft.signalr.Action7;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;
import com.microsoft.signalr.androidchatroom.model.param.LoginParam;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;

import org.jetbrains.annotations.NotNull;

import java.util.Timer;
import java.util.TimerTask;

import io.reactivex.CompletableObserver;
import io.reactivex.Single;
import io.reactivex.annotations.NonNull;
import io.reactivex.disposables.Disposable;
import io.reactivex.schedulers.Schedulers;


public class SignalRService {
    private static final String TAG = "SignalRService";

    private static final String serverUrl = "http://10.0.2.2:5000/chat";

    private static String username;
    private static String deviceUuid;

    private static HubConnection hubConnection = null;
    private static Timer reconnectTimer = null;

    public static void startHubConnection(SimpleCallback<String> callback) {
        if (hubConnection == null) {
            synchronized (SignalRService.class) {
                if (hubConnection == null) {
                    hubConnection = HubConnectionBuilder.create(serverUrl).build();
                }
            }
        }
        if (hubConnection.getConnectionState() != HubConnectionState.CONNECTED) {
            hubConnection.start().subscribeOn(Schedulers.io())
                    .subscribe(new CompletableObserver() {
                        @Override
                        public void onSubscribe(@NonNull Disposable d) {

                        }

                        @Override
                        public void onComplete() {
                            callback.onSuccess("");
                        }

                        @Override
                        public void onError(@NonNull Throwable e) {
                            callback.onError(e.getMessage());
                        }
                    });
        } else {
            callback.onSuccess("");
        }
    }

    private static void reconnect() {
        if (hubConnection.getConnectionState() != HubConnectionState.CONNECTED) {
            hubConnection.start().subscribe(new CompletableObserver() {
                @Override
                public void onSubscribe(@NotNull Disposable d) {

                }

                @Override
                public void onComplete() {

                }

                @Override
                public void onError(@NotNull Throwable e) {
                    Log.e(TAG, e.toString());
                }
            });
        } else {
            // If connected, must be in an active session. Directly call TouchServer
            hubConnection.send("TouchServer", deviceUuid, username);
        }
    }

    public static void stopHubConnection() {
        if (hubConnection != null && hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            synchronized (SignalRService.class) {
                hubConnection.stop();
            }
        }
    }

    public static <T1> void registerServerCallback(String target, Action1<T1> action, Class<T1> clazz) {
        hubConnection.on(target, action, clazz);
    }

    public static <T1, T2> void registerServerCallback(String target, Action2<T1, T2> action, Class<T1> clazz1, Class<T2> clazz2) {
        hubConnection.on(target, action, clazz1, clazz2);
    }

    public static <T1, T2, T3> void registerServerCallback(String target, Action3<T1, T2, T3> action, Class<T1> clazz1, Class<T2> clazz2, Class<T3> clazz3) {
        hubConnection.on(target, action, clazz1, clazz2, clazz3);
    }

    public static <T1, T2, T3, T4, T5, T6, T7> void registerServerCallback(String target, Action7<T1, T2, T3, T4, T5, T6, T7> action, Class<T1> clazz1, Class<T2> clazz2, Class<T3> clazz3, Class<T4> clazz4, Class<T5> clazz5, Class<T6> clazz6, Class<T7> clazz7) {
        hubConnection.on(target, action, clazz1, clazz2, clazz3, clazz4, clazz5, clazz6, clazz7);
    }

    public static Single<String> login(LoginParam loginParam) {
        deviceUuid = loginParam.getDeviceUuid();
        username = loginParam.getUsername();
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            return hubConnection.invoke(String.class, "EnterChatRoom", deviceUuid, username);
        }
        return null;
    }

    public static void startReconnectTimer() {
        reconnectTimer = new Timer();
        reconnectTimer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                reconnect();
            }
        }, 0, 5000);
    }

    public static void stopReconnectTimer() {
        if (reconnectTimer != null) {
            reconnectTimer.cancel();
            reconnectTimer = null;
        }
    }

    public static Single<String> logout() {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            return hubConnection.invoke(String.class, "LeaveChatRoom", deviceUuid, username);
        }
        return null;
    }

    public static void sendBroadcastMessage(String messageId, String sender, String payload, boolean isImage) {
        Log.d(TAG, "sendBroadcastMessage");
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            hubConnection.send("OnBroadcastMessageReceived",
                    messageId, sender, payload, isImage);
        }
    }

    public static void sendPrivateMessage(String messageId, String sender, String receiver, String payload, boolean isImage) {
        Log.d(TAG, "sendPrivateMessage");
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            hubConnection.send("OnPrivateMessageReceived",
                    messageId, sender, receiver, payload, isImage);
        }
    }

    public static void sendMessageRead(String messageId) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            hubConnection.send("OnReadResponseReceived", messageId, username);
        }
    }

    public static void sendAck(String ackId) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            hubConnection.send("OnAckResponseReceived", ackId, username);
        }
    }

    public static void pullHistoryMessages(long untilTimeInLong) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            hubConnection.send("OnPullHistoryMessagesReceived", username, untilTimeInLong);
        }
    }

    public static void pullImageContent(String messageId) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            hubConnection.send("OnPullImageContentReceived", username, messageId);
        }
    }

    public static String getUsername() {
        return username;
    }
}
