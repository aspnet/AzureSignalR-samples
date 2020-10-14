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
import com.microsoft.signalr.androidchatroom.message.EnterLeaveMessage;
import com.microsoft.signalr.androidchatroom.message.Message;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.Timer;
import java.util.TimerTask;
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
    private EditText chatBoxEditText;
    private Button chatBoxSendButton;
    private RecyclerView chatContentRecyclerView;
    private ChatContentAdapter chatContentAdapter;

    // Used for datetime formatting
    private SimpleDateFormat sdf = new SimpleDateFormat("MM/dd hh:mm:ss", Locale.UK);

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
        this.chatBoxEditText = view.findViewById(R.id.edit_chat);
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
        hubConnection.on("broadcastChatMessage", this::broadcastChatMessage,
                String.class, String.class, String.class, String.class, String.class);
        hubConnection.on("broadcastEnterMessage", this::broadcastEnterMessage,
                String.class);
        hubConnection.on("broadcastLeaveMessage", this::broadcastLeaveMessage,
                String.class);

        // Broadcast user has entered chat room
        Log.d("EnterChatRoom", "called");
        hubConnection.send("EnterChatRoom", deviceToken, username);

        // Set onClickListener for SEND button
        chatBoxSendButton.setOnClickListener(this::chatBoxSendButtonClickListener);

        // Set periodic chat message sender method
        new Timer().scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                messageSendingHandler();
            }
        }, 500, 500);
    }

    public void chatBoxSendButtonClickListener(View view) {
        if (chatBoxEditText.getText().length() > 0) { // Empty message not allowed
            // Create and add message into list
            String messageContent = chatBoxEditText.getText().toString();
            ChatMessage chatMessage = new ChatMessage(username, sdf.format(new Date()), messageContent, Message.SELF_SENDING_MESSAGE);
            messages.add(chatMessage);
            sendingMessageCount.incrementAndGet();
            chatBoxEditText.getText().clear();

            // Update RecyclerView
            requireActivity().runOnUiThread(() -> {
                chatContentAdapter.notifyDataSetChanged();
                chatContentRecyclerView.scrollToPosition(messages.size() - 1);
            });

            if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
                synchronized (chatMessage) {
                    hubConnection.send("BroadcastChatMessage",
                            deviceToken,
                            chatMessage.getSender(),
                            chatMessage.getTime(),
                            chatMessage.getContent(),
                            chatMessage.getUuid());
                }
            }
        }
    }

    public void messageSendingHandler() {
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

    public void sendMessageQueue() {
        for (Message message : messages) {
            synchronized (message) {
                if (message.getMessageEnum() == Message.SELF_SENDING_MESSAGE && hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
                    ChatMessage chatMessage = (ChatMessage) message;

                    hubConnection.send("BroadcastChatMessage",
                            deviceToken,
                            chatMessage.getSender(),
                            chatMessage.getTime(),
                            chatMessage.getContent(),
                            chatMessage.getUuid());
                }
            }
        }
    }

    public void broadcastChatMessage(String senderDeviceToken, String sender, String time, String content, String uuid) {
        if (senderDeviceToken.equals(deviceToken)) { // A senderDeviceToken equals to current deviceToken serves as an ACK
            for (Message message : messages) {
                synchronized (message) {
                    if (message.getUuid().equals(uuid) && message.getMessageEnum() == Message.SELF_SENDING_MESSAGE) {
                        // Change message status
                        message.setMessageEnum(Message.SELF_SENT_MESSAGE);
                        sendingMessageCount.decrementAndGet();
                        break;
                    }
                }
            }
        } else { // Create ChatMessage according to parameters
            boolean isDuplicateMessage = false;
            for (Message message : messages) {
                if (message.getUuid().equals(uuid)) {
                    isDuplicateMessage = true;
                    break;
                }
            }
            if (!isDuplicateMessage) {
                ChatMessage chatMessage = new ChatMessage(sender, time, content, uuid, Message.INCOMING_MESSAGE);
                messages.add(chatMessage);
            }
        }
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            chatContentRecyclerView.scrollToPosition(messages.size() - 1);
        });
    }

    public void broadcastEnterMessage(String username) {
        EnterLeaveMessage enterMessage = new EnterLeaveMessage(username, Message.ENTER_MESSAGE);
        messages.add(enterMessage);
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            chatContentRecyclerView.scrollToPosition(messages.size() - 1);
        });
    }

    public void broadcastLeaveMessage(String username) {
        EnterLeaveMessage leaveMessage = new EnterLeaveMessage(username, Message.LEAVE_MESSAGE);
        messages.add(leaveMessage);
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
                case Message.SELF_SENDING_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_self_message, parent, false);
                    break;
                case Message.SELF_SENT_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_self_message, parent, false);
                    break;
                case Message.ENTER_MESSAGE:
                case Message.LEAVE_MESSAGE:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_enter_leave_message, parent, false);
                    break;
                case Message.INCOMING_MESSAGE:
                default:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_incoming_message, parent, false);
                    break;
            }

            return new ChatContentViewHolder(view);
        }

        @Override
        public void onBindViewHolder(@NonNull ChatContentViewHolder viewHolder, int position) {
            Message message = messages.get(position);
            if (Message.ENTER_MESSAGE == message.getMessageEnum() || Message.LEAVE_MESSAGE == message.getMessageEnum()) {
                EnterLeaveMessage enterLeaveMessage = (EnterLeaveMessage) message;
                viewHolder.enterLeaveContent.setText(enterLeaveMessage.getContent());
            } else {
                ChatMessage chatMessage = (ChatMessage) message;
                viewHolder.messageSender.setText(chatMessage.getSender());
                viewHolder.messageTime.setText(chatMessage.getTime());
                viewHolder.messageContent.setText(chatMessage.getContent());
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
