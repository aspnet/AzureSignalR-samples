package com.microsoft.signalr.androidchatroom.service;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;

import androidx.annotation.Nullable;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;
import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.message.ChatMessage;
import com.microsoft.signalr.androidchatroom.fragment.MessageReceiver;
import com.microsoft.signalr.androidchatroom.message.Message;
import com.microsoft.signalr.androidchatroom.message.MessageType;
import com.microsoft.signalr.androidchatroom.message.SystemMessage;

import org.jetbrains.annotations.NotNull;

import java.util.ArrayList;
import java.util.List;
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
    private String deviceUuid;

    // Reconnect timer
    private Boolean firstPull = true;
    private AtomicBoolean sessionStarted = new AtomicBoolean(false);
    private int reconnectDelay = 0; // immediate connect to server when enter the chat room
    private int reconnectInterval = 5000;
    private Timer reconnectTimer;

    // Resend timer
    private int resendChatMessageDelay = 5000;
    private int resendChatMessageInterval = 5000;
    private Timer resendChatMessageTimer;

    // Gson object for deserialization
    private final Gson gson = new Gson();

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
    public void register(String username, String deviceUuid, MessageReceiver messageReceiver) {
        this.username = username;
        this.deviceUuid = deviceUuid;
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
        // Register the method handlers called by the server
        hubConnection.on("broadcastSystemMessage", this::broadcastSystemMessage,
                String.class, String.class, Long.class);
        hubConnection.on("displayBroadcastMessage", this::displayBroadcastMessage,
                String.class, String.class, String.class, String.class, Long.class, String.class);
        hubConnection.on("displayPrivateMessage", this::displayPrivateMessage,
                String.class, String.class, String.class, String.class, Long.class, String.class);
        hubConnection.on("serverAck", this::serverAck, String.class, Long.class);
        hubConnection.on("expireSession", this::expireSession, Boolean.class);
        hubConnection.on("addHistoryMessages", this::addHistoryMessages, String.class);
    }

    @Override
    public void expireSession(boolean showAlert) {
        Log.d("expireSession", "Server expired the session");
        hubConnection.stop();
        reconnectTimer.cancel();
        resendChatMessageTimer.cancel();
        sessionStarted.set(false);
        if (showAlert) {
            messageReceiver.showSessionExpiredDialog();
        }
    }


    //// Message methods called by server
    public void broadcastSystemMessage(String messageId, String text, long sendTime) {
        Log.d("broadcastSystemMessage", text);

        // Create message
        SystemMessage systemMessage = new SystemMessage(messageId, text, sendTime);

        // Try to add message to fragment
        messageReceiver.tryAddMessage(systemMessage, 1);
    }

    public void displayBroadcastMessage(String messageId, String sender, String receiver, String text, long sendTime, String ackId) {
        Log.d("displayBroadcastMessage", sender);

        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId, username);

        // Create message
        ChatMessage chatMessage = new ChatMessage(messageId, sender, receiver, text, sendTime, MessageType.RECEIVED_BROADCAST_MESSAGE);

        // Try to add message to fragment
        messageReceiver.tryAddMessage(chatMessage, 1);
    }

    public void displayPrivateMessage(String messageId, String sender, String receiver, String text, long sendTime, String ackId) {
        Log.d("displayPrivateMessage", sender);

        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId, username);

        // Create message
        ChatMessage chatMessage = new ChatMessage(messageId, sender, receiver, text, sendTime, MessageType.RECEIVED_PRIVATE_MESSAGE);

        // Try to add message to fragment
        messageReceiver.tryAddMessage(chatMessage, 1);
    }

    public void serverAck(String messageId, long receivedTimeInLong) {
        Log.d("serverAck", messageId);
        messageReceiver.setMessageAck(messageId, receivedTimeInLong);
    }

    public void addHistoryMessages(String serializedString) {
        Log.d("addHistoryMessages", serializedString);
        List<Message> historyMessages = new ArrayList<>();
        JsonArray jsonArray = gson.fromJson(serializedString, JsonArray.class);
        for (JsonElement jsonElement : jsonArray) {
            ChatMessage chatMessage = ChatMessage.fromJsonObject(jsonElement.getAsJsonObject(), username);
            historyMessages.add(chatMessage);
        }
        Log.d("addHistoryMessages", historyMessages.toString());
        if (firstPull) {
            synchronized (firstPull) {
                if (firstPull) {
                    messageReceiver.tryAddAllMessages(historyMessages, 1);
                    firstPull = false;
                }
            }
        } else {
            messageReceiver.tryAddAllMessages(historyMessages, 0);
        }

    }

    //// Message sending methods
    @Override
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

    //// Pulling message methods
    @Override
    public void pullHistoryMessages(long untilTimeInLong) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            Log.d("pullHistoryMessages", "Called with long time: " + untilTimeInLong);
            hubConnection.send("OnPullHistoryMessagesReceived", username, untilTimeInLong);
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
                if (message.getMessageType() == MessageType.SENDING_BROADCAST_MESSAGE ||
                    message.getMessageType() == MessageType.SENDING_PRIVATE_MESSAGE) {
                    sendMessage(message);
                }
            }
        }
    }

    private void connectToServer() {
        if (hubConnection.getConnectionState() != HubConnectionState.CONNECTED) {
            hubConnection.start().subscribe(new CompletableObserver() {
                @Override
                public void onSubscribe(@NotNull Disposable d) {

                }

                @Override
                public void onComplete() {
                    if (!sessionStarted.get()) { // very first start of connection
                        onSessionStart();
                        hubConnection.send("EnterChatRoom", deviceUuid, username);
                        messageReceiver.activate();
                        sessionStarted.set(true);
                    }
                    Log.d("Reconnection", "touch server after reconnection");
                    hubConnection.send("TouchServer", deviceUuid, username);
                }

                @Override
                public void onError(@NotNull Throwable e) {
                    Log.e("HubConnection", e.toString());
                }
            });
        } else {
            hubConnection.send("TouchServer", deviceUuid, username);
        }
    }
}
