package com.microsoft.signalr.androidchatroom.service;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;

import androidx.annotation.Nullable;

import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;
import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.fragment.ChatUserInterface;
import com.microsoft.signalr.androidchatroom.message.Message;
import com.microsoft.signalr.androidchatroom.message.MessageFactory;

import org.jetbrains.annotations.NotNull;

import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.atomic.AtomicBoolean;

import io.reactivex.CompletableObserver;
import io.reactivex.annotations.NonNull;
import io.reactivex.disposables.Disposable;

public class SignalRChatService extends Service implements ChatService {
    // SignalR HubConnection
    private HubConnection hubConnection;
    private ChatUserInterface chatUserInterface;

    // User info
    private String username;
    private String deviceUuid;

    // Reconnect timer
    private boolean firstPull = true;
    private boolean activePull = false;
    private final AtomicBoolean sessionStarted = new AtomicBoolean(false);
    private final int reconnectDelay = 0; // immediate connect to server when enter the chat room
    private final int reconnectInterval = 5000;
    private Timer reconnectTimer;

    // Service binder
    private final IBinder chatServiceBinder = new ChatServiceBinder();

    public class ChatServiceBinder extends Binder {
        public SignalRChatService getService() {
            return  SignalRChatService.this;
        }
    }

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return chatServiceBinder;
    }

    ///////// Session managers

    @Override
    public void register(String username, String deviceUuid, ChatUserInterface chatUserInterface) {
        this.username = username;
        this.deviceUuid = deviceUuid;
        this.chatUserInterface = chatUserInterface;
    }

    @Override
    public void startSession() {
        if (!sessionStarted.get()) {
            synchronized (sessionStarted) {
                if (!sessionStarted.get()) {
                    // Create hub connection
                    this.hubConnection = HubConnectionBuilder.create(getString(R.string.app_server_url)).build();

                    // Start hub connection
                    this.hubConnection.start().subscribe(new CompletableObserver() {
                        @Override
                        public void onSubscribe(@NonNull Disposable d) {

                        }

                        @Override
                        public void onComplete() {
                            // When completed start process, register methods and start guard thread
                            onSessionStart();
                        }

                        @Override
                        public void onError(@NonNull Throwable e) {

                        }
                    });
                }
            }
        }
    }

    private void onSessionStart() {
        // Set atomic boolean status
        sessionStarted.set(true);

        // Register message receivers
        hubConnection.on("receiveSystemMessage", this::receiveSystemMessage,
                String.class, String.class, Long.class);
        hubConnection.on("receiveBroadcastMessage", this::receiveBroadcastMessage,
                String.class, String.class, String.class, String.class, Boolean.class, Long.class, String.class);
        hubConnection.on("receivePrivateMessage", this::receivePrivateMessage,
                String.class, String.class, String.class, String.class, Boolean.class, Long.class, String.class);

        // Register rich content receivers
        hubConnection.on("receiveHistoryMessages", this::receiveHistoryMessages, String.class);
        hubConnection.on("receiveImageContent", this::receiveImageContent, String.class, String.class);

        // Register operation receivers
        hubConnection.on("serverAck", this::serverAck, String.class, Long.class);
        hubConnection.on("clientRead", this::clientRead, String.class, String.class);
        hubConnection.on("expireSession", this::expireSession, Boolean.class);

        // Activate UI click listeners
        chatUserInterface.activateClickEvent();

        // Broadcast enter chat room
        hubConnection.send("EnterChatRoom", deviceUuid, username);

        // start guard thread for reconnecting
        reconnectTimer = new Timer();
        reconnectTimer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                connectToServer();
            }
        }, reconnectDelay, reconnectInterval);

    }

    @Override
    public void expireSession(boolean showAlert) {
        if (sessionStarted.get()) {
            synchronized (sessionStarted) {
                if (sessionStarted.get() && hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
                    reconnectTimer.cancel();

                    hubConnection.invoke("LeaveChatRoom", deviceUuid, username).subscribe(new CompletableObserver() {
                        @Override
                        public void onSubscribe(@NonNull Disposable d) {

                        }

                        @Override
                        public void onComplete() {
                            hubConnection.stop();
                            sessionStarted.set(false);
                        }

                        @Override
                        public void onError(@NonNull Throwable e) {
                            Log.e("HubConnection", e.toString());
                            hubConnection.stop();
                            sessionStarted.set(false);
                        }
                    });
                }
            }
        }
        if (showAlert) {
            Log.d("expireSession", "Server expired the session");
            chatUserInterface.showSessionExpiredDialog();
        } else {
            Log.d("expireSession", "Manually quited the session");
        }
    }

    ///////// Receivers

    /// Message receivers
    private void receiveSystemMessage(String messageId, String payload, long sendTime) {
        Log.d("receiveSystemMessage", payload);

        // Create message
        Message systemMessage = MessageFactory.createReceivedSystemMessage(messageId, payload, sendTime);

        // Try to add message to fragment
        chatUserInterface.tryAddMessage(systemMessage, 1);
    }

    private void receiveBroadcastMessage(String messageId, String sender, String receiver, String payload, boolean isImage, long sendTime, String ackId) {
        Log.d("receiveBroadcastMessage", sender);

        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId, username);

        // Create message
        Message chatMessage;
        if (isImage) {
            chatMessage = MessageFactory.createReceivedImageBroadcastMessage(messageId, sender, payload,sendTime);
        } else {
            chatMessage = MessageFactory.createReceivedTextBroadcastMessage(messageId, sender, payload, sendTime);
        }

        // Try to add message to fragment
        chatUserInterface.tryAddMessage(chatMessage, 1);
    }

    private void receivePrivateMessage(String messageId, String sender, String receiver, String payload, boolean isImage, long sendTime, String ackId) {
        Log.d("receivePrivateMessage", sender);

        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId, username);

        // Create message
        Message chatMessage;
        if (isImage) {
            chatMessage = MessageFactory.createReceivedImagePrivateMessage(messageId, sender, receiver, payload,sendTime);
        } else {
            chatMessage = MessageFactory.createReceivedTextPrivateMessage(messageId, sender, receiver, payload, sendTime);
        }

        // Try to add message to fragment
        chatUserInterface.tryAddMessage(chatMessage, 1);
    }

    /// Rich content receivers
    private void receiveImageContent(String messageId, String payload) {
        chatUserInterface.setImageContent(messageId, payload);
    }

    private void receiveHistoryMessages(String serializedString) {
        Log.d("receiveHistoryMessages", serializedString);
        List<Message> historyMessages = MessageFactory.parseHistoryMessages(serializedString, username);

        if (firstPull) {
            chatUserInterface.tryAddAllMessages(historyMessages, 1);
            firstPull = false;
        } else {
            chatUserInterface.tryAddAllMessages(historyMessages, 0);
        }

        activePull = false;
    }

    private void serverAck(String messageId, long receivedTimeInLong) {
        chatUserInterface.setSentMessageAck(messageId, receivedTimeInLong);
    }

    private void clientRead(String messageId, String username) {
        chatUserInterface.setSentMessageRead(messageId);
    }

    ///////// Senders

    /// Message sending methods
    @Override
    public void sendMessage(Message chatMessage) {
        synchronized (chatMessage) {
            switch (chatMessage.getMessageType()) {
                case SENDING_TEXT_BROADCAST_MESSAGE:
                case SENDING_IMAGE_BROADCAST_MESSAGE:
                    sendBroadcastMessage(chatMessage);
                    break;
                case SENDING_TEXT_PRIVATE_MESSAGE:
                case SENDING_IMAGE_PRIVATE_MESSAGE:
                    sendPrivateMessage(chatMessage);
                    break;
                default:
            }
        }
    }

    @Override
    public void sendMessageRead(String messageId) {
        hubConnection.send("OnReadResponseReceived", messageId, username);
    }

    private void sendBroadcastMessage(Message broadcastMessage) {
        Log.d("SEND BCAST MESSAGE", broadcastMessage.toString());
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            if (broadcastMessage.isImage() && broadcastMessage.getBmp()!=null ||
                !broadcastMessage.isImage()) {
                hubConnection.send("OnBroadcastMessageReceived",
                        broadcastMessage.getMessageId(),
                        broadcastMessage.getSender(),
                        broadcastMessage.getPayload(),
                        broadcastMessage.isImage());
            }
        }
    }

    private void sendPrivateMessage(Message privateMessage) {
        Log.d("SEND PRIVATE MESSAGE", privateMessage.toString());
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            if (privateMessage.isImage() && privateMessage.getBmp()!=null ||
                    !privateMessage.isImage()) {
                hubConnection.send("OnPrivateMessageReceived",
                        privateMessage.getMessageId(),
                        privateMessage.getSender(),
                        privateMessage.getReceiver(),
                        privateMessage.getPayload(),
                        privateMessage.isImage());
            }
        }
    }

    //// Pulling message methods
    @Override
    public void pullHistoryMessages(long untilTimeInLong) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED && !activePull) {
            activePull = true;
            Log.d("pullHistoryMessages", "Called with long time: " + untilTimeInLong);
            hubConnection.send("OnPullHistoryMessagesReceived", username, untilTimeInLong);
        }
    }

    @Override
    public void pullImageContent(String messageId) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            Log.d("pullImageContent", "message id=" + messageId);
            hubConnection.send("OnPullImageContentReceived", username, messageId);
        }
    }

    // Guard thread method
    private void connectToServer() {
        if (hubConnection.getConnectionState() != HubConnectionState.CONNECTED) {
            hubConnection.start().subscribe(new CompletableObserver() {
                @Override
                public void onSubscribe(@NotNull Disposable d) {

                }

                @Override
                public void onComplete() {
                    hubConnection.send("TouchServer", deviceUuid, username);
                }

                @Override
                public void onError(@NotNull Throwable e) {
                    Log.e("HubConnection", e.toString());
                }
            });
        } else {
            // If connected, must be in an active session. Directly call TouchServer
            hubConnection.send("TouchServer", deviceUuid, username);
        }
    }
}
