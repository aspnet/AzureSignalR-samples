package com.microsoft.signalr.androidchatroom.fragment;

import android.app.AlertDialog;
import android.content.ClipData;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.navigation.Navigation;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.activity.MainActivity;
import com.microsoft.signalr.androidchatroom.message.MessageFactory;
import com.microsoft.signalr.androidchatroom.message.MessageTypeEnum;
import com.microsoft.signalr.androidchatroom.message.Message;
import com.microsoft.signalr.androidchatroom.service.ChatService;
import com.microsoft.signalr.androidchatroom.service.NotificationService;

import java.io.ByteArrayOutputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Base64;
import java.util.Date;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.stream.Collectors;

import static android.app.Activity.RESULT_OK;

public class ChatFragment extends Fragment implements MessageReceiver {
    private static final String TAG = "ChatFragment";
    private static final int RESULT_LOAD_IMAGE = 1;

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
    private Button chatBoxImageButton;
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
        this.chatBoxImageButton = view.findViewById(R.id.button_chatbox_image);
        this.chatContentRecyclerView = view.findViewById(R.id.recyclerview_chatcontent);

        // Create objects
        this.chatContentAdapter = new ChatContentAdapter(messages, getContext(), this);
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
        new Thread(() -> {
            chatService.register(username, deviceUuid, this);
            chatService.startSession();
        }).start();
    }

    @Override
    public void activateClickEvent() {
        chatBoxSendButton.setOnClickListener(this::chatBoxSendButtonClickListener);
        chatBoxImageButton.setOnClickListener(this::chatBoxImageButtonClickListener);
        chatContentRecyclerView.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                super.onScrolled(recyclerView, dx, dy);
                if (!recyclerView.canScrollVertically(-1)) {
                    Log.d(TAG, "OnScroll cannot scroll vertical -1");
                    long untilTime = System.currentTimeMillis();
                    for (Message message : messages) {
                        if (message.getMessageType().name().contains("RECEIVED") || message.getMessageType().name().contains("SENT")) {
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

        refreshUiThread(true, direction);
    }

    @Override
    public void tryAddAllMessages(List<Message> messages, int direction) {
        Set<String> existedMessageIds = this.messages.stream().map(Message::getMessageId).collect(Collectors.toSet());
        for (Message message : messages) {
            if (!existedMessageIds.contains(message.getMessageId())) {
                this.messages.add(message);
                Log.d("Image message", String.format("id=%s\ttime=%s", message.getMessageId(), message.getTime()));
                existedMessageIds.add(message.getMessageId());
            }
        }

        refreshUiThread(true, direction);
    }

    @Override
    public void setMessageAck(String messageId, long receivedTimeInLong) {
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                message.ack(receivedTimeInLong);
                Log.d("setMessageAck", messageId);
                break;
            }
        }
        refreshUiThread(true, 0);
    }

    @Override
    public void showSessionExpiredDialog() {
        requireActivity().runOnUiThread(() -> {
            AlertDialog.Builder builder = new AlertDialog.Builder(getContext());
            builder.setMessage(R.string.alert_message)
                    .setTitle(R.string.alert_title)
                    .setCancelable(false);
            builder.setPositiveButton(R.string.alert_ok, (dialog, id) -> {
                Navigation.findNavController(requireView()).navigate(R.id.action_ChatFragment_to_LoginFragment);
                requireActivity().recreate();
            });
            AlertDialog dialog = builder.create();
            dialog.show();
        });
    }

    private void chatBoxSendButtonClickListener(View view) {
        if (chatBoxMessageEditText.getText().length() > 0) { // Empty message not allowed
            // Create and add message into list
            Message chatMessage = createTextMessage();
            messages.add(chatMessage);

            // If hubConnection is active then send message
            chatMessage.startSendMessageTimer(this, r -> r.refreshUiThread(false,0));
            chatService.sendMessage(chatMessage);

            // Refresh ui
            refreshUiThread(false,1);
        }
    }

    private void chatBoxImageButtonClickListener(View view) {
        Intent photoPickerIntent = new Intent(Intent.ACTION_PICK);
        photoPickerIntent.setType("image/*");
        startActivityForResult(photoPickerIntent, RESULT_LOAD_IMAGE);
    }

    @Override
    public void onActivityResult(int reqCode, int resultCode, Intent data) {
        super.onActivityResult(reqCode, resultCode, data);
        if (resultCode == RESULT_OK) {
            try {
                final Uri imageUri = data.getData();
                final InputStream imageStream = requireActivity().getContentResolver().openInputStream(imageUri);
                final Bitmap selectedImage = BitmapFactory.decodeStream(imageStream);
                Message imageMessage = createAndSendImageMessage(selectedImage);
                messages.add(imageMessage);
                refreshUiThread(false,1);
            } catch (FileNotFoundException e) {
                e.printStackTrace();
                Toast.makeText(getContext(), "Image picking failed.", Toast.LENGTH_LONG).show();
            }
        } else {
            Toast.makeText(getContext(), "You haven't picked Image.", Toast.LENGTH_LONG).show();
        }
    }

    @Override
    public void loadImageContent(String messageId, String payload) {
        Message message = getMessageWithId(messageId);
        if (message != null) {
            message.ackPullImage();
            message.setPayload(payload);
            message.setBmp(MessageFactory.decodeToBitmap(payload));
            refreshUiThread(false,0);
        }
    }

    private Message createTextMessage() {
        // Receiver field length == 0 -> broadcast message
        boolean isBroadcastMessage = chatBoxReceiverEditText.getText().length() == 0;
        String messageContent = chatBoxMessageEditText.getText().toString();
        chatBoxMessageEditText.getText().clear();
        Message chatMessage;
        if (isBroadcastMessage) {
            chatMessage = MessageFactory.createSendingTextBroadcastMessage(username, messageContent, System.currentTimeMillis());
        } else {
            String receiver = chatBoxReceiverEditText.getText().toString();
            chatMessage = MessageFactory.createSendingTextPrivateMessage(username, receiver, messageContent, System.currentTimeMillis());
        }
        return chatMessage;
    }

    private Message createAndSendImageMessage(Bitmap bmp) {
        // Receiver field length == 0 -> broadcast message
        boolean isBroadcastMessage = chatBoxReceiverEditText.getText().length() == 0;
        Message chatMessage;

        if (isBroadcastMessage) {
            chatMessage = MessageFactory.createSendingImageBroadcastMessage(username, bmp, System.currentTimeMillis(), m -> {
                m.startSendMessageTimer(this, r -> r.refreshUiThread(false, 0));
                chatService.sendMessage(m);
            });
        } else {
            String receiver = chatBoxReceiverEditText.getText().toString();
            chatMessage = MessageFactory.createSendingImagePrivateMessage(username, receiver, bmp, System.currentTimeMillis(), m -> {
                m.startSendMessageTimer(this, r -> r.refreshUiThread(false,0));
                chatService.sendMessage(m);
            });
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

    private Message getMessageWithId(String messageId) {
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                return message;
            }
        }

        return null;
    }

    @Override
    public void refreshUiThread(boolean sort, int direction) {
        // Sort by send time first
        if (sort) {
            messages.sort((m1, m2) -> (int) (m1.getTime() - m2.getTime()));
        }


        // Then refresh the UiThread
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            switch (direction) {
                case 1:
                    Log.d(TAG, "Scrolling to position " + (messages.size() - 1));
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

    interface ItemClickListener {
        void onClickItem(Message message);
    }

    static class ChatContentViewHolder extends RecyclerView.ViewHolder {
        // For all non-system message
        private final TextView messageSender;
        private final TextView messageTime;
        private final TextView statusTextView;

        // For Text Message
        private final TextView messageTextContent;

        // For Image Message
        private final ImageView messageImageContent;

        // For System Message
        private final TextView systemMessageContent;

        ChatContentViewHolder(View view) {
            super(view);
            this.messageSender = view.findViewById(R.id.textview_message_sender);
            this.messageTime = view.findViewById(R.id.textview_message_time);
            this.statusTextView = view.findViewById(R.id.textview_message_status);
            this.messageTextContent = view.findViewById(R.id.textview_message_content);
            this.messageImageContent = view.findViewById(R.id.imageview_message_content);
            this.systemMessageContent = view.findViewById(R.id.textview_enter_leave_content);
        }

        public void bindImageClick(final Message message, final ItemClickListener listener) {
            messageImageContent.setOnClickListener(v -> listener.onClickItem(message));
        }

        public void bindStatusClick(final Message message, final ItemClickListener listener) {
            statusTextView.setOnClickListener(v -> listener.onClickItem(message));
        }
    }

    class ChatContentAdapter extends RecyclerView.Adapter<ChatContentViewHolder> {
        private static final int SYSTEM_MESSAGE_VIEW = 0;
        private static final int SENDING_TEXT_BROADCAST_MESSAGE_VIEW = 1;
        private static final int SENDING_TEXT_PRIVATE_MESSAGE_VIEW = 2;
        private static final int SENT_TEXT_BROADCAST_MESSAGE_VIEW = 3;
        private static final int SENT_TEXT_PRIVATE_MESSAGE_VIEW = 4;
        private static final int RECEIVED_TEXT_BROADCAST_MESSAGE_VIEW = 5;
        private static final int RECEIVED_TEXT_PRIVATE_MESSAGE_VIEW = 6;
        private static final int SENDING_IMAGE_BROADCAST_MESSAGE_VIEW = 7;
        private static final int SENDING_IMAGE_PRIVATE_MESSAGE_VIEW = 8;
        private static final int SENT_IMAGE_BROADCAST_MESSAGE_VIEW = 9;
        private static final int SENT_IMAGE_PRIVATE_MESSAGE_VIEW = 10;
        private static final int RECEIVED_IMAGE_BROADCAST_MESSAGE_VIEW = 11;
        private static final int RECEIVED_IMAGE_PRIVATE_MESSAGE_VIEW = 12;

        private final Context context;
        private final List<Message> messages;
        private final MessageReceiver messageReceiver;

        // Used for datetime formatting
        private final SimpleDateFormat sdf = new SimpleDateFormat("MM/dd hh:mm:ss", Locale.US);

        public ChatContentAdapter(List<Message> messages, Context context, MessageReceiver messageReceiver) {
            this.messages = messages;
            this.context = context;
            this.messageReceiver = messageReceiver;
        }

        @NonNull
        @Override
        public ChatContentViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
            View view;

            switch (viewType) {
                case SENDING_TEXT_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_text_broadcast_message, parent, false);
                    break;
                case SENDING_TEXT_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_text_private_message, parent, false);
                    break;
                case SENT_TEXT_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_text_broadcast_message, parent, false);
                    break;
                case SENT_TEXT_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_text_private_message, parent, false);
                    break;
                case RECEIVED_TEXT_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_text_broadcast_message, parent, false);
                    break;
                case RECEIVED_TEXT_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_text_private_message, parent, false);
                    break;
                case SENDING_IMAGE_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_image_broadcast_message, parent, false);
                    break;
                case SENDING_IMAGE_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sending_image_private_message, parent, false);
                    break;
                case SENT_IMAGE_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_image_broadcast_message, parent, false);
                    break;
                case SENT_IMAGE_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sent_image_private_message, parent, false);
                    break;
                case RECEIVED_IMAGE_BROADCAST_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_image_broadcast_message, parent, false);
                    break;
                case RECEIVED_IMAGE_PRIVATE_MESSAGE_VIEW:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_image_private_message, parent, false);
                    break;
                case SYSTEM_MESSAGE_VIEW:
                default:
                    view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_system_message, parent, false);
                    break;
            }

            return new ChatContentViewHolder(view);
        }

        @Override
        public void onBindViewHolder(@NonNull ChatContentViewHolder viewHolder, int position) {
            Message message = messages.get(position);

            // System message directly set content and return
            if (message.getMessageType() == MessageTypeEnum.SYSTEM_MESSAGE) {
                viewHolder.systemMessageContent.setText(message.getPayload());
                return;
            }

            // General chat message components
            viewHolder.messageSender.setText(message.getSender());
            viewHolder.messageTime.setText(sdf.format(new Date(message.getTime())));

            // Special cases
            if (message.getMessageType().name().contains("TEXT")) {
                // Normal text content
                viewHolder.messageTextContent.setText(message.getPayload());
            } else if (message.getMessageType().name().contains("IMAGE") && message.getBmp() != null) {
                // Loaded image content
                Bitmap bmp = message.getBmp();
                int[] resized = resizeImage(bmp.getWidth(), bmp.getHeight(), 400);
                viewHolder.messageImageContent.setImageBitmap(Bitmap.createScaledBitmap(bmp, resized[0], resized[1], false));
            } else if (message.getMessageType().name().contains("IMAGE") && message.getBmp() == null) {
                viewHolder.messageImageContent.setImageResource(R.drawable.ic_ready_to_pull);
                // Image content need to load
                if (message.isPullImageTimeOut() && message.getBmp() == null) {
                    viewHolder.bindImageClick(message, v -> {
                        if (message.isPullImageTimeOut() && message.getBmp() == null) {
                            Log.d("Clicking image time=", new Date(message.getTime()).toString());
                            message.startPullImageTimer(messageReceiver, r -> r.refreshUiThread(false, 0));
                            viewHolder.messageImageContent.setImageResource(R.drawable.ic_pulling);
                            chatService.pullImageContent(message.getMessageId());
                        }
                    });
                }
            }

            if (message.getMessageType().name().contains("SENDING")) {
                if (message.isSendMessageTimeOut()) {
                    viewHolder.statusTextView.setText(R.string.message_resend);
                    viewHolder.bindStatusClick(message, v -> {
                        if (message.isSendMessageTimeOut()) {
                            viewHolder.statusTextView.setText(R.string.message_sending);
                            message.setTime(System.currentTimeMillis());
                            message.startSendMessageTimer(messageReceiver, r -> r.refreshUiThread(false, 0));
                            chatService.sendMessage(message);
                        }
                    });
                } else {
                    viewHolder.statusTextView.setText(R.string.message_sending);
                    viewHolder.statusTextView.setOnClickListener(null);
                }
            }
        }


        @Override
        public int getItemCount() {
            return this.messages.size();
        }

        @Override
        public int getItemViewType(int position) {
            return messages.get(position).getMessageType().getValue();
        }

        private int[] resizeImage(int width, int height, int maxValue) {
            if (width > height && width > maxValue) {
                height = height * maxValue / width;
                width = maxValue;
            } else if (width < height && height > maxValue) {
                width = width * maxValue / height;
                height = maxValue;
            }
            return new int[]{width, height};
        }
    }
}
