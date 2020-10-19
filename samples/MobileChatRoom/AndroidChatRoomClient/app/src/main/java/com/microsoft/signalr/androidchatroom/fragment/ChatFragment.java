package com.microsoft.signalr.androidchatroom.fragment;

import android.content.Context;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.google.firebase.iid.FirebaseInstanceId;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;
import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.message.ChatMessage;
import com.microsoft.signalr.androidchatroom.message.MessageType;
import com.microsoft.signalr.androidchatroom.message.SystemMessage;
import com.microsoft.signalr.androidchatroom.message.Message;

import org.jetbrains.annotations.NotNull;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Date;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.atomic.AtomicBoolean;

import io.reactivex.CompletableObserver;
import io.reactivex.disposables.Disposable;

public class ChatFragment extends Fragment {
    // SignalR HubConnection
    private HubConnection hubConnection = null;

    // Messages
    private final List<Message> messages = new ArrayList<>();

    // User info
    private String username;
    private String deviceToken;

    // View elements and adapters
    private EditText chatBoxReceiverEditText;
    private EditText chatBoxMessageEditText;
    private Button chatBoxSendButton;
    private RecyclerView chatContentRecyclerView;
    private ChatContentAdapter chatContentAdapter;

    // Reconnect timer
    private AtomicBoolean firstConnectionStarted = new AtomicBoolean(false);
    private int reconnectDelay = 0; // immediate connect to server when enter the chat room
    private int reconnectInterval = 5000;
    private Timer reconnectTimer;

    // Resend timer
    private int resendChatMessageDelay = 2500;
    private int resendChatMessageInterval = 2500;
    private Timer resendChatMessageTimer;

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_chat, container, false);

        // Get passed parameters
        if (getArguments() != null) {
            this.username = getArguments().getString("username");
        } else {
            this.username = "Nobody";
        }

        // Get view element references
        this.chatBoxReceiverEditText = view.findViewById(R.id.edit_chat_receiver);
        this.chatBoxMessageEditText = view.findViewById(R.id.edit_chat_message);
        this.chatBoxSendButton = view.findViewById(R.id.button_chatbox_send);
        this.chatContentRecyclerView = view.findViewById(R.id.recyclerview_chatcontent);

        // Create objects
        this.chatContentAdapter = new ChatContentAdapter(messages, getContext());
        LinearLayoutManager layoutManager = new LinearLayoutManager(this.getActivity());

        // Configure RecyclerView
        layoutManager.setStackFromEnd(true);
        chatContentRecyclerView.setLayoutManager(layoutManager);
        chatContentRecyclerView.setAdapter(chatContentAdapter);
        chatContentAdapter.notifyDataSetChanged();
        chatContentRecyclerView.scrollToPosition(messages.size() - 1);

        return view;
    }

    @Override
    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        // Fetch device token for notification
        FirebaseInstanceId.getInstance()
                .getInstanceId()
                .addOnSuccessListener(
                        instanceIdResult -> this.deviceToken = instanceIdResult.getToken());

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

    private void onHubConnectionStart() {
        // Register the method handlers
        hubConnection.on("broadcastSystemMessage", this::broadcastSystemMessage,
                String.class, String.class);
        hubConnection.on("displayBroadcastMessage", this::displayBroadcastMessage,
                String.class, String.class, String.class, String.class, Long.class, String.class);
        hubConnection.on("displayPrivateMessage", this::displayPrivateMessage,
                String.class, String.class, String.class, String.class, Long.class, String.class);
        hubConnection.on("serverAck", this::serverAck, String.class);

        // Set onClickListener for SEND button
        chatBoxSendButton.setOnClickListener(this::chatBoxSendButtonClickListener);
    }

    public void chatBoxSendButtonClickListener(View view) {

        if (chatBoxMessageEditText.getText().length() > 0) { // Empty message not allowed
            // Create and add message into list
            ChatMessage chatMessage = createMessage();
            messages.add(chatMessage);

            refreshUiThread();

            // If hubConnection is active then send message
            if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
                sendMessage(chatMessage);
            }
        }
    }

    public void broadcastSystemMessage(String messageId, String text) {
        // Check for duplicated message
        boolean isDuplicateMessage = checkForDuplicatedMessage(messageId);

        // If not duplicated, create ChatMessage according to parameters
        if (!isDuplicateMessage) {
            SystemMessage systemMessage = new SystemMessage(messageId, text);
            messages.add(systemMessage);
        }

        refreshUiThread();
    }

    public void displayBroadcastMessage(String messageId, String sender, String receiver, String text, long sendTime, String ackId) {
        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId);

        // Check for duplicated message
        boolean isDuplicateMessage = checkForDuplicatedMessage(messageId);

        // If not duplicated, create ChatMessage according to parameters
        if (!isDuplicateMessage) {
            ChatMessage chatMessage = new ChatMessage(messageId, sender, receiver, text, sendTime, MessageType.RECEIVED_BROADCAST_MESSAGE);
            messages.add(chatMessage);
        }

        refreshUiThread();
    }

    public void displayPrivateMessage(String messageId, String sender, String receiver, String text, long sendTime, String ackId) {
        Log.d("displayPrivateMessage", sender);

        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId);

        // Check for duplicated message
        boolean isDuplicateMessage = checkForDuplicatedMessage(messageId);

        // If not duplicated, create ChatMessage according to parameters
        if (!isDuplicateMessage) {
            ChatMessage chatMessage = new ChatMessage(messageId, sender, receiver, text, sendTime, MessageType.RECEIVED_PRIVATE_MESSAGE);
            messages.add(chatMessage);
        }

        refreshUiThread();
    }

    public void serverAck(String messageId) {
        for (Message message : messages) {
            synchronized (message) {
                if (message.getMessageId().equals(messageId)) {
                    message.ack();
                    Log.d("ACK", messageId);
                    break;
                }
            }
        }

        refreshUiThread();
    }

    private ChatMessage createMessage() {
        // Receiver field length == 0 -> broadcast message
        boolean isBroadcastMessage = chatBoxReceiverEditText.getText().length() == 0;
        String messageContent = chatBoxMessageEditText.getText().toString();
        chatBoxMessageEditText.getText().clear();
        ChatMessage chatMessage;
        if (isBroadcastMessage) {
            chatMessage = new ChatMessage(username, "", messageContent, System.currentTimeMillis(), MessageType.SENDING_BROADCAST_MESSAGE);
        } else {
            String receiver = chatBoxReceiverEditText.getText().toString();
            chatMessage = new ChatMessage(username, receiver, messageContent, System.currentTimeMillis(), MessageType.SENDING_PRIVATE_MESSAGE);
        }
        return chatMessage;
    }

    private boolean checkForDuplicatedMessage(String messageId) {
        boolean isDuplicateMessage = false;
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                isDuplicateMessage = true;
                break;
            }
        }
        return isDuplicateMessage;
    }

    private void refreshUiThread() {
        // Sort by send time first
        Collections.sort(messages, (m1, m2) -> (int) (m1.getTime() - m2.getTime()));

        // Then refresh the UiThread
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            chatContentRecyclerView.scrollToPosition(messages.size() - 1);
        });
    }

    private void sendMessage(ChatMessage chatMessage) {
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

    private void connectToServer() {
        if (hubConnection.getConnectionState() != HubConnectionState.CONNECTED) {
            Log.d("reconnectHandler", "called");
            hubConnection.start().subscribe(new CompletableObserver() {
                @Override
                public void onSubscribe(@NotNull Disposable d) {

                }

                @Override
                public void onComplete() {
                    if (!firstConnectionStarted.get()) { // very first start of connection
                        onHubConnectionStart();
                        hubConnection.send("EnterChatRoom", deviceToken, username);
                        firstConnectionStarted.set(true);
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

    private void resendChatMessageHandler() {
        // Calculate chat messages to resend
        Set<ChatMessage> sendingMessages = new HashSet<>();
        for (Message message : messages) {
            if (message.getMessageType() == MessageType.SENDING_BROADCAST_MESSAGE
                    || message.getMessageType() == MessageType.SENDING_PRIVATE_MESSAGE) {
                sendingMessages.add((ChatMessage) message);
            }
        }

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

    static class ChatContentViewHolder extends RecyclerView.ViewHolder {
        // For ChatMessage
        private TextView messageSender;
        private TextView messageContent;
        private TextView messageTime;

        // For EnterLeaveMessage
        private TextView enterLeaveContent;

        ChatContentViewHolder(View view) {
            super(view);
            this.messageSender = (TextView) view.findViewById(R.id.textview_message_sender);
            this.messageContent = (TextView) view.findViewById(R.id.textview_message_content);
            this.messageTime = (TextView) view.findViewById(R.id.textview_message_time);
            this.enterLeaveContent = (TextView) view.findViewById(R.id.textview_enter_leave_content);
        }
    }

    static class ChatContentAdapter extends RecyclerView.Adapter<ChatContentViewHolder> {
        private static final int SYSTEM_MESSAGE_VIEW = 0;
        private static final int SENDING_BROADCAST_MESSAGE_VIEW = 1;
        private static final int SENDING_PRIVATE_MESSAGE_VIEW = 2;
        private static final int SENT_BROADCAST_MESSAGE_VIEW = 3;
        private static final int SENT_PRIVATE_MESSAGE_VIEW = 4;
        private static final int RECEIVED_BROADCAST_MESSAGE_VIEW = 5;
        private static final int RECEIVED_PRIVATE_MESSAGE_VIEW = 6;

        private Context context;
        private List<Message> messages;

        // Used for datetime formatting
        private SimpleDateFormat sdf = new SimpleDateFormat("MM/dd hh:mm:ss", Locale.US);

        public ChatContentAdapter(List<Message> messages, Context context) {
            this.messages = messages;
            this.context = context;
        }

        @NonNull
        @Override
        public ChatContentViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
            View view;

            switch (viewType) {
                case SENDING_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_broadcast_message, parent, false);
                    break;
                case SENDING_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_private_message, parent, false);
                    break;
                case SENT_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_broadcast_message, parent, false);
                    break;
                case SENT_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_private_message, parent, false);
                    break;
                case RECEIVED_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_broadcast_message, parent, false);
                    break;
                case RECEIVED_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_private_message, parent, false);
                    break;
                case SYSTEM_MESSAGE_VIEW:
                default:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_enter_leave_message, parent, false);
                    break;
            }

            return new ChatContentViewHolder(view);
        }

        @Override
        public void onBindViewHolder(@NonNull ChatContentViewHolder viewHolder, int position) {
            Message message = messages.get(position);
            switch (message.getMessageType()) {
                case SYSTEM_MESSAGE:
                    SystemMessage systemMessage = (SystemMessage) message;
                    viewHolder.enterLeaveContent.setText(systemMessage.getText());
                    break;
                case SENDING_BROADCAST_MESSAGE:
                case SENDING_PRIVATE_MESSAGE:
                case SENT_BROADCAST_MESSAGE:
                case SENT_PRIVATE_MESSAGE:
                case RECEIVED_BROADCAST_MESSAGE:
                case RECEIVED_PRIVATE_MESSAGE:
                default:
                    ChatMessage chatMessage = (ChatMessage) message;
                    viewHolder.messageSender.setText(chatMessage.getSender());
                    viewHolder.messageTime.setText(sdf.format(new Date(chatMessage.getTime())));
                    viewHolder.messageContent.setText(chatMessage.getText());
            }
        }

        @Override
        public int getItemCount() {
            return this.messages.size();
        }

        @Override
        public int getItemViewType(int position) {
            int viewType;
            switch(messages.get(position).getMessageType()) {
                case SYSTEM_MESSAGE:
                    viewType = SYSTEM_MESSAGE_VIEW;
                    break;
                case SENDING_BROADCAST_MESSAGE:
                    viewType = SENDING_BROADCAST_MESSAGE_VIEW;
                    break;
                case SENDING_PRIVATE_MESSAGE:
                    viewType = SENDING_PRIVATE_MESSAGE_VIEW;
                    break;
                case SENT_BROADCAST_MESSAGE:
                    viewType = SENT_BROADCAST_MESSAGE_VIEW;
                    break;
                case SENT_PRIVATE_MESSAGE:
                    viewType = SENT_PRIVATE_MESSAGE_VIEW;
                    break;
                case RECEIVED_PRIVATE_MESSAGE:
                    viewType = RECEIVED_PRIVATE_MESSAGE_VIEW;
                    break;
                case RECEIVED_BROADCAST_MESSAGE:
                default:
                    viewType = RECEIVED_BROADCAST_MESSAGE_VIEW;
            }
            return viewType;
        }
    }
}
