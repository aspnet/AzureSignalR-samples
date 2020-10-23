package com.microsoft.signalr.androidchatroom.fragment;

import android.app.AlertDialog;
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
import androidx.navigation.Navigation;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.activity.MainActivity;
import com.microsoft.signalr.androidchatroom.message.ChatMessage;
import com.microsoft.signalr.androidchatroom.message.MessageType;
import com.microsoft.signalr.androidchatroom.message.SystemMessage;
import com.microsoft.signalr.androidchatroom.message.Message;
import com.microsoft.signalr.androidchatroom.service.ChatService;
import com.microsoft.signalr.androidchatroom.service.NotificationService;

import java.text.SimpleDateFormat;
import java.time.Instant;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.stream.Collectors;

public class ChatFragment extends Fragment implements MessageReceiver {
    private static final String TAG = "ChatFragment";

    // Services
    private ChatService chatService;
    private NotificationService notificationService;

    // Messages
    private final List<Message> messages = new ArrayList<>();
    private String username;
    private String deviceUuid;

    // View elements and adapters
    private EditText chatBoxReceiverEditText;
    private EditText chatBoxMessageEditText;
    private Button chatBoxSendButton;
    private RecyclerView chatContentRecyclerView;
    private ChatContentAdapter chatContentAdapter;

    @Override
    public void onAttach(Context context) {
        super.onAttach(context);
        try {
            chatService = ((MainActivity) context).getChatService();
            notificationService = ((MainActivity) context).getNotificationService();
        } catch (ClassCastException e) {
            Log.e(TAG, e.getMessage());
        }
    }

    @Override
    public void onDetach() {
        super.onDetach();
        // Remove activity reference
        chatService = null;
        notificationService = null;
    }

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_chat, container, false);

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
        // Set chat fragment for chat service

        // Get passed username
        if ((username = getArguments().getString("username")) == null) {
            username = "Nobody";
        }

        // Get deviceUuid
        deviceUuid = notificationService.getDeviceUuid();

        // Register user info into chat service
        chatService.register(username, deviceUuid,this);
        chatService.startSession();
    }

    @Override
    public void activate() {
        chatBoxSendButton.setOnClickListener(this::chatBoxSendButtonClickListener);
        chatContentRecyclerView.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                super.onScrolled(recyclerView, dx, dy);
                if (!recyclerView.canScrollVertically(-1)) {
                    Log.d(TAG, "OnScroll cannot scroll vertical -1");
                    long untilTime = System.currentTimeMillis();
                    for (Message message : messages) {
                        if (message.getMessageType() == MessageType.RECEIVED_BROADCAST_MESSAGE ||
                                message.getMessageType() == MessageType.RECEIVED_PRIVATE_MESSAGE ||
                                message.getMessageType() == MessageType.SENT_BROADCAST_MESSAGE ||
                                message.getMessageType() == MessageType.SENT_PRIVATE_MESSAGE) {
                            untilTime = message.getTime();
                            break;
                        }
                    }
                    chatService.pullHistoryMessages(untilTime);
                }
            }
        });
    }

    @Override
    public void tryAddMessage(Message message, int direction) {
        // Check for duplicated message
        boolean isDuplicateMessage = checkForDuplicatedMessage(message.getMessageId());

        // If not duplicated, create ChatMessage according to parameters
        if (!isDuplicateMessage) {
            messages.add(message);
        }

        refreshUiThread(direction);
    }

    @Override
    public void tryAddAllMessages(List<Message> messages, int direction) {
        Set<String> existedMessageIds = this.messages.stream().map(Message::getMessageId).collect(Collectors.toSet());
        for (Message message : messages) {
            if (!existedMessageIds.contains(message.getMessageId())) {
                this.messages.add(message);
                existedMessageIds.add(message.getMessageId());
            }
        }

        refreshUiThread(direction);
    }

    @Override
    public void setMessageAck(String messageId, long receivedTimeInLong) {
        for (Message message : messages) {
            synchronized (message) {
                if (message.getMessageId().equals(messageId)) {
                    message.ack(receivedTimeInLong);
                    Log.d("setMessageAck", messageId);
                    break;
                }
            }
        }

        refreshUiThread(0);
    }

    @Override
    public Set<ChatMessage> getSendingMessages() {
        Set<ChatMessage> sendingMessages = new HashSet<>();
        for (Message message : messages) {
            if (message.getMessageType() == MessageType.SENDING_BROADCAST_MESSAGE
                    || message.getMessageType() == MessageType.SENDING_PRIVATE_MESSAGE) {
                sendingMessages.add((ChatMessage) message);
            }
        }
        return sendingMessages;
    }

    @Override
    public void showSessionExpiredDialog() {
        requireActivity().runOnUiThread(() -> {
            AlertDialog.Builder builder = new AlertDialog.Builder(getContext());
            builder.setMessage(R.string.alert_message)
                    .setTitle(R.string.alert_title)
                    .setCancelable(false);
            builder.setPositiveButton(R.string.alert_ok, (dialog, id) -> {
                Navigation.findNavController(getView()).navigate(R.id.action_ChatFragment_to_LoginFragment);
                getActivity().recreate();
            });
            AlertDialog dialog = builder.create();
            dialog.show();
        });
    }

    private void chatBoxSendButtonClickListener(View view) {
        if (chatBoxMessageEditText.getText().length() > 0) { // Empty message not allowed
            // Create and add message into list
            ChatMessage chatMessage = createMessage();
            messages.add(chatMessage);

            // If hubConnection is active then send message
            synchronized (chatMessage) {
                chatService.sendMessage(chatMessage);
            }

            // Refresh ui
            refreshUiThread(1);
        }
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

    private void refreshUiThread(int direction) {
        // Sort by send time first
        messages.sort((m1, m2) -> (int) (m1.getTime() - m2.getTime()));

        // Then refresh the UiThread
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            switch (direction) {
                case 1:
                    Log.d(TAG, "Scrolling to position "+ (messages.size() - 1) );
                    chatContentRecyclerView.scrollToPosition(messages.size() - 1);
                    break;
                case -1:
                    Log.d(TAG, "Scrolling to position 0");
                    chatContentRecyclerView.scrollToPosition(0);
                    break;
                default:

            }
        });
    }

    static class ChatContentViewHolder extends RecyclerView.ViewHolder {
        // For ChatMessage
        private final TextView messageSender;
        private final TextView messageContent;
        private final TextView messageTime;

        // For EnterLeaveMessage
        private final TextView enterLeaveContent;

        ChatContentViewHolder(View view) {
            super(view);
            this.messageSender = view.findViewById(R.id.textview_message_sender);
            this.messageContent = view.findViewById(R.id.textview_message_content);
            this.messageTime = view.findViewById(R.id.textview_message_time);
            this.enterLeaveContent = view.findViewById(R.id.textview_enter_leave_content);
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

        private final Context context;
        private final List<Message> messages;

        // Used for datetime formatting
        private final SimpleDateFormat sdf = new SimpleDateFormat("MM/dd hh:mm:ss", Locale.US);

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
            switch (messages.get(position).getMessageType()) {
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
