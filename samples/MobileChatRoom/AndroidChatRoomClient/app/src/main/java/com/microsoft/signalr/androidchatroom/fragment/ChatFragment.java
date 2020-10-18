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
import com.microsoft.signalr.androidchatroom.message.SystemMessage;
import com.microsoft.signalr.androidchatroom.message.Message;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.concurrent.atomic.AtomicInteger;

import io.reactivex.disposables.Disposable;

public class ChatFragment extends Fragment {
    // SignalR HubConnection
    private HubConnection hubConnection = null;

    // Messages
    private final List<Message> messages = new ArrayList<>();

    // Message count to send
    private AtomicInteger sendingMessageCount = new AtomicInteger(0);

    // User info
    private String username;
    private String deviceToken;

    // View elements and adapters
    private EditText chatBoxReceiverEditText;
    private EditText chatBoxMessageEditText;
    private Button chatBoxSendButton;
    private RecyclerView chatContentRecyclerView;
    private ChatContentAdapter chatContentAdapter;

    // Used for datetime formatting
    private SimpleDateFormat sdf = new SimpleDateFormat("MM/dd hh:mm:ss", Locale.US);

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
        Disposable toDispose = hubConnection.start().subscribe(this::onHubConnectionStart);
    }

    public void onHubConnectionStart() {
        // Register the method handlers
        hubConnection.on("broadcastSystemMessage", this::broadcastSystemMessage,
                String.class, String.class);
        hubConnection.on("displayBroadcastMessage", this::displayBroadcastMessage,
                String.class, String.class, String.class, String.class, String.class, String.class);
        hubConnection.on("displayPrivateMessage", this::displayPrivateMessage,
                String.class, String.class, String.class, String.class, String.class, String.class);
        hubConnection.on("serverAck", this::serverAck, String.class);

        // Broadcast user has entered chat room
        Log.d("EnterChatRoom", "called");
        hubConnection.send("EnterChatRoom", deviceToken, username);

        // Set onClickListener for SEND button
        chatBoxSendButton.setOnClickListener(this::chatBoxSendButtonClickListener);

        // Set periodic chat message sender method
//        new Timer().scheduleAtFixedRate(new TimerTask() {
//            @Override
//            public void run() {
//                messageSendingHandler();
//            }
//        }, 500, 500);
    }

    public void chatBoxSendButtonClickListener(View view) {
        if (chatBoxMessageEditText.getText().length() > 0) { // Empty message not allowed
            // Create and add message into list
            boolean isBroadcastMessage = chatBoxReceiverEditText.getText().length() == 0;
            String messageContent = chatBoxMessageEditText.getText().toString();
            ChatMessage chatMessage;
            if (isBroadcastMessage) {
                chatMessage = new ChatMessage(username, "", messageContent, sdf.format(new Date()), Message.SENDING_BROADCAST_MESSAGE);
            } else {
                String receiver = chatBoxReceiverEditText.getText().toString();
                chatMessage = new ChatMessage(username, receiver, messageContent, sdf.format(new Date()), Message.SENDING_PRIVATE_MESSAGE);
            }
            messages.add(chatMessage);
            sendingMessageCount.incrementAndGet();
            chatBoxMessageEditText.getText().clear();

            // Update RecyclerView
            requireActivity().runOnUiThread(() -> {
                chatContentAdapter.notifyDataSetChanged();
                chatContentRecyclerView.scrollToPosition(messages.size() - 1);
            });

            if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
                synchronized (chatMessage) {
                    if (isBroadcastMessage) {
                        Log.d("SEND BCAST MESSAGE", chatMessage.toString());
                        hubConnection.send("OnBroadcastMessageReceived",
                                chatMessage.getMessageId(),
                                chatMessage.getSender(),
                                chatMessage.getText());
                    } else {
                        Log.d("SEND PRIVATE MESSAGE", chatMessage.toString());
                        hubConnection.send("OnPrivateMessageReceived",
                                chatMessage.getMessageId(),
                                chatMessage.getSender(),
                                chatMessage.getReceiver(),
                                chatMessage.getText());
                    }
                }
            }
        }
    }



    private void messageSendingHandler() {
        if (sendingMessageCount.get() > 0) {
            try {
                Disposable toDispose = hubConnection.start().subscribe(
                        this::sendMessageQueue,
                        onError -> Log.e("HubConnection", onError.toString())
                );
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

    private void sendMessageQueue() {
        for (Message message : messages) {
            synchronized (message) {
                if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
                    if (message.getMessageEnum() == Message.SENDING_BROADCAST_MESSAGE) {
                        ChatMessage chatMessage = (ChatMessage) message;
                        hubConnection.send("OnBroadcastMessageReceived",
                                chatMessage.getMessageId(),
                                chatMessage.getSender(),
                                chatMessage.getTime(),
                                chatMessage.getText());
                    } else if (message.getMessageEnum() == Message.SENDING_PRIVATE_MESSAGE) {
                        ChatMessage chatMessage = (ChatMessage) message;
                        hubConnection.send("OnPrivateMessageReceived",
                                chatMessage.getMessageId(),
                                chatMessage.getSender(),
                                chatMessage.getReceiver(),
                                chatMessage.getTime(),
                                chatMessage.getText());
                    }
                }
            }
        }
    }

    public void broadcastSystemMessage(String messageId, String text) {
        boolean isDuplicateMessage = false;
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                isDuplicateMessage = true;
                break;
            }
        }
        if (!isDuplicateMessage) {
            SystemMessage systemMessage = new SystemMessage(messageId, text);
            messages.add(systemMessage);
        }
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            chatContentRecyclerView.scrollToPosition(messages.size() - 1);
        });
    }

    public void displayBroadcastMessage(String messageId, String sender, String receiver, String text, String sendTime, String ackId) {
        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId);

        // Create ChatMessage according to parameters
        boolean isDuplicateMessage = false;
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                isDuplicateMessage = true;
                break;
            }
        }
        if (!isDuplicateMessage) {
            ChatMessage chatMessage = new ChatMessage(messageId, sender, receiver, text, sendTime, Message.RECEIVED_BROADCAST_MESSAGE);
            messages.add(chatMessage);
        }
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            chatContentRecyclerView.scrollToPosition(messages.size() - 1);
        });
    }

    public void displayPrivateMessage(String messageId, String sender, String receiver, String text, String sendTime, String ackId) {
        // Send back ack
        hubConnection.send("OnAckResponseReceived", ackId);

        // Create ChatMessage according to parameters
        boolean isDuplicateMessage = false;
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                isDuplicateMessage = true;
                break;
            }
        }
        if (!isDuplicateMessage) {
            ChatMessage chatMessage = new ChatMessage(messageId, sender, receiver, text, sendTime, Message.RECEIVED_PRIVATE_MESSAGE);
            messages.add(chatMessage);
        }
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            chatContentRecyclerView.scrollToPosition(messages.size() - 1);
        });
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
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            chatContentRecyclerView.scrollToPosition(messages.size() - 1);
        });
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
        private Context context;
        private List<Message> messages;

        public ChatContentAdapter(List<Message> messages, Context context) {
            this.messages = messages;
            this.context = context;
        }

        @NonNull
        @Override
        public ChatContentViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
            View view;

            switch (viewType) {
                case Message.SENDING_BROADCAST_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_broadcast_message, parent, false);
                    break;
                case Message.SENDING_PRIVATE_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_private_message, parent, false);
                    break;
                case Message.SENT_BROADCAST_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_broadcast_message, parent, false);
                    break;
                case Message.SENT_PRIVATE_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_private_message, parent, false);
                    break;
                case Message.RECEIVED_BROADCAST_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_broadcast_message, parent, false);
                    break;
                case Message.RECEIVED_PRIVATE_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_private_message, parent, false);
                    break;
                case Message.SYSTEM_MESSAGE:
                default:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_enter_leave_message, parent, false);
                    break;
            }

            return new ChatContentViewHolder(view);
        }

        @Override
        public void onBindViewHolder(@NonNull ChatContentViewHolder viewHolder, int position) {
            Message message = messages.get(position);
            if (Message.SYSTEM_MESSAGE == message.getMessageEnum()) {
                SystemMessage systemMessage = (SystemMessage) message;
                viewHolder.enterLeaveContent.setText(systemMessage.getText());
            } else {
                ChatMessage chatMessage = (ChatMessage) message;
                viewHolder.messageSender.setText(chatMessage.getSender());
                viewHolder.messageTime.setText(chatMessage.getTime());
                viewHolder.messageContent.setText(chatMessage.getText());
            }
        }

        @Override
        public int getItemCount() {
            return this.messages.size();
        }

        @Override
        public int getItemViewType(int position) {
            return messages.get(position).getMessageEnum();
        }
    }
}
