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
import com.microsoft.signalr.androidchatroom.message.ChatMessage;
import com.microsoft.signalr.androidchatroom.fragment.MessageReceiver;
import com.microsoft.signalr.androidchatroom.message.MessageType;
import com.microsoft.signalr.androidchatroom.message.SystemMessage;

import org.jetbrains.annotations.NotNull;

import java.util.Set;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.atomic.AtomicBoolean;

import io.reactivex.CompletableObserver;
import io.reactivex.disposables.Disposable;

public class SignalRChatService extends Service implements ChatService {
    // SignalR HubConnection
    private HubConnection hubConnection;
    private MessageReceiver messageReceiver;

    // User info
    private String username;
    private String deviceToken;

    // Reconnect timer
    private AtomicBoolean sessionStarted = new AtomicBoolean(false);
    private int reconnectDelay = 0; // immediate connect to server when enter the chat room
    private int reconnectInterval = 5000;
    private Timer reconnectTimer;

    // Resend timer
    private int resendChatMessageDelay = 2500;
    private int resendChatMessageInterval = 2500;
    private Timer resendChatMessageTimer;

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

    //// Register methods
    @Override
    public void register(String username, String deviceToken, MessageReceiver messageReceiver) {
        this.username = username;
        this.deviceToken = deviceToken;
        this.messageReceiver = messageReceiver;
    }

    //// Session management methods
    @Override
    public void startSession() {
        // Create, register, and start hub connection
        this.hubConnection = HubConnectionBuilder.create(getString(R.string.app_server_url)).build();
        // Set reconnect timer
        reconnectTimer = new Timer();
        reconnectTimer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                connectToServer();
            }
        }, reconnectDelay, reconnectInterval);

        // Set resend chat message timer
        resendChatMessageTimer = new Timer();
        resendChatMessageTimer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                resendChatMessageHandler();
            }
        }, resendChatMessageDelay, resendChatMessageInterval);
    }

    private void onSessionStart() {
        // Register the method handlers
        hubConnection.on("broadcastSystemMessage", this::broadcastSystemMessage,
                String.class, String.class);
        hubConnection.on("displayBroadcastMessage", this::displayBroadcastMessage,
                String.class, String.class, String.class, String.class, Long.class, String.class);
        hubConnection.on("displayPrivateMessage", this::displayPrivateMessage,
                String.class, String.class, String.class, String.class, Long.class, String.class);
        hubConnection.on("serverAck", this::serverAck, String.class);
        hubConnection.on("expireSession", this::expireSession);
    }

    @Override
    public void expireSession() {
        Log.d("expireSession", "Server expired the session");
        hubConnection.stop();
        reconnectTimer.cancel();
        resendChatMessageTimer.cancel();
        messageReceiver.showSessionExpiredDialog();
    }


    //// Message methods called by server
    @Override
    public void broadcastSystemMessage(String messageId, String text) {
        Log.d("broadcastSystemMessage", text);

        // Create message
        SystemMessage systemMessage = new SystemMessage(messageId, text);

        // Try to add message to fragment
        messageReceiver.tryAddMessage(systemMessage);
    }

    @Override
    public void displayBroadcastMessage(String messageId, String sender, String receiver, String text, long sendTime, String ackId) {
        Log.d("displayBroadcastMessage", sender);

        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId);

        // Create message
        ChatMessage chatMessage = new ChatMessage(messageId, sender, receiver, text, sendTime, MessageType.RECEIVED_BROADCAST_MESSAGE);

        // Try to add message to fragment
        messageReceiver.tryAddMessage(chatMessage);
    }

    @Override
    public void displayPrivateMessage(String messageId, String sender, String receiver, String text, long sendTime, String ackId) {
        Log.d("displayPrivateMessage", sender);

        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId);

        // Create message
        ChatMessage chatMessage = new ChatMessage(messageId, sender, receiver, text, sendTime, MessageType.RECEIVED_PRIVATE_MESSAGE);

        // Try to add message to fragment
        messageReceiver.tryAddMessage(chatMessage);
    }

    @Override
    public void serverAck(String messageId) {
        messageReceiver.setMessageAck(messageId);
    }

    //// Message sender methods
    public void sendMessage(ChatMessage chatMessage) {
        synchronized (chatMessage) {
            switch (chatMessage.getMessageType()) {
                case SENDING_BROADCAST_MESSAGE:
                    sendBroadcastMessage(chatMessage);
                    break;
                case SENDING_PRIVATE_MESSAGE:
                    sendPrivateMessage(chatMessage);
                    break;
                default:
            }
        }
    }

    private void sendBroadcastMessage(ChatMessage broadcastMessage) {
        Log.d("SEND BCAST MESSAGE", broadcastMessage.toString());
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            hubConnection.send("OnBroadcastMessageReceived",
                    broadcastMessage.getMessageId(),
                    broadcastMessage.getSender(),
                    broadcastMessage.getText());
        }
    }

    private void sendPrivateMessage(ChatMessage privateMessage) {
        Log.d("SEND PRIVATE MESSAGE", privateMessage.toString());
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            hubConnection.send("OnPrivateMessageReceived",
                    privateMessage.getMessageId(),
                    privateMessage.getSender(),
                    privateMessage.getReceiver(),
                    privateMessage.getText());
        }
    }

    //// Resend and reconnect methods
    private void resendChatMessageHandler() {
        // Calculate chat messages to resend
        Set<ChatMessage> sendingMessages = messageReceiver.getSendingMessages();

        if (sendingMessages.size() > 0 && hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            resendChatMessages(sendingMessages);
        }
    }

    private void resendChatMessages(Set<ChatMessage> messagesToSend) {
        for (ChatMessage message : messagesToSend) {
            synchronized (message) {
                sendMessage(message);
            }
        }
    }

    private void connectToServer() {
        if (hubConnection.getConnectionState() != HubConnectionState.CONNECTED) {
            Log.d("reconnectHandler", "called");
            hubConnection.start().subscribe(new CompletableObserver() {
                @Override
                public void onSubscribe(@NotNull Disposable d) {

                }

                @Override
                public void onComplete() {
                    if (!sessionStarted.get()) { // very first start of connection
                        onSessionStart();
                        hubConnection.send("EnterChatRoom", deviceToken, username);
                        sessionStarted.set(true);
                    }
                    Log.d("Reconnection", "touch server after reconnection");
                    hubConnection.send("TouchServer", deviceToken, username);
                }

                @Override
                public void onError(@NotNull Throwable e) {
                    Log.e("HubConnection", e.toString());
                }
            });
        } else {
            hubConnection.send("TouchServer", deviceToken, username);
        }
    }
}
