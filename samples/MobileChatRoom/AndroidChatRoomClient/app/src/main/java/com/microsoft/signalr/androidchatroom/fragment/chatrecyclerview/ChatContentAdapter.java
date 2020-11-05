package com.microsoft.signalr.androidchatroom.fragment.chatrecyclerview;

import android.content.Context;
import android.graphics.Bitmap;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.fragment.ChatUserInterface;
import com.microsoft.signalr.androidchatroom.message.Message;
import com.microsoft.signalr.androidchatroom.message.MessageTypeEnum;
import com.microsoft.signalr.androidchatroom.service.ChatService;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Locale;

public class ChatContentAdapter extends RecyclerView.Adapter<ChatContentViewHolder> {
    private static final int READ_IMAGE_PRIVATE_MESSAGE_VIEW = 0;
    private static final int READ_TEXT_PRIVATE_MESSAGE_VIEW = 1;
    private static final int RECEIVED_IMAGE_BROADCAST_MESSAGE_VIEW = 2;
    private static final int RECEIVED_IMAGE_PRIVATE_MESSAGE_VIEW = 3;
    private static final int RECEIVED_TEXT_BROADCAST_MESSAGE_VIEW = 4;
    private static final int RECEIVED_TEXT_PRIVATE_MESSAGE_VIEW = 5;
    private static final int SYSTEM_MESSAGE_VIEW = 6;
    private static final int SENDING_IMAGE_BROADCAST_MESSAGE_VIEW = 7;
    private static final int SENDING_IMAGE_PRIVATE_MESSAGE_VIEW = 8;
    private static final int SENDING_TEXT_BROADCAST_MESSAGE_VIEW = 9;
    private static final int SENDING_TEXT_PRIVATE_MESSAGE_VIEW = 10;
    private static final int SENT_IMAGE_BROADCAST_MESSAGE_VIEW = 11;
    private static final int SENT_IMAGE_PRIVATE_MESSAGE_VIEW = 12;
    private static final int SENT_TEXT_BROADCAST_MESSAGE_VIEW = 13;
    private static final int SENT_TEXT_PRIVATE_MESSAGE_VIEW = 14;

    private final ChatUserInterface chatUserInterface;
    private final ChatService chatService;


    private final Context context;
    private final List<Message> messages;


    // Used for datetime formatting
    private final SimpleDateFormat sdf = new SimpleDateFormat("MM/dd hh:mm:ss", Locale.US);

    public ChatContentAdapter(List<Message> messages, Context context, ChatUserInterface chatUserInterface, ChatService chatService) {
        this.messages = messages;
        this.context = context;
        this.chatUserInterface = chatUserInterface;
        this.chatService = chatService;
    }

    @NonNull
    @Override
    public ChatContentViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view;

        switch (viewType) {
            case SENDING_TEXT_BROADCAST_MESSAGE_VIEW:
            case SENT_TEXT_BROADCAST_MESSAGE_VIEW:
                view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_self_text_broadcast_message, parent, false);
                break;
            case SENDING_TEXT_PRIVATE_MESSAGE_VIEW:
            case SENT_TEXT_PRIVATE_MESSAGE_VIEW:
            case READ_TEXT_PRIVATE_MESSAGE_VIEW:
                view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_self_text_private_message, parent, false);
                break;
            case SENDING_IMAGE_BROADCAST_MESSAGE_VIEW:
            case SENT_IMAGE_BROADCAST_MESSAGE_VIEW:
                view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_self_image_broadcast_message, parent, false);
                break;
            case SENDING_IMAGE_PRIVATE_MESSAGE_VIEW:
            case SENT_IMAGE_PRIVATE_MESSAGE_VIEW:
            case READ_IMAGE_PRIVATE_MESSAGE_VIEW:
                view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_self_image_private_message, parent, false);
                break;
            case RECEIVED_TEXT_BROADCAST_MESSAGE_VIEW:
                view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_text_broadcast_message, parent, false);
                break;
            case RECEIVED_TEXT_PRIVATE_MESSAGE_VIEW:
                view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_received_text_private_message, parent, false);
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

        // Different types of message content
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
                        message.startPullImageTimer(chatUserInterface, r -> r.refreshUiThread(false, 0));
                        viewHolder.messageImageContent.setImageResource(R.drawable.ic_pulling);
                        chatService.pullImageContent(message.getMessageId());
                    }
                });
            }
        }

        // Different types of message status
        if (message.getMessageType().name().contains("SENDING")) {
            if (message.isSendMessageTimeOut()) {
                viewHolder.statusTextView.setText(R.string.message_resend);
                viewHolder.bindStatusClick(message, v -> {
                    if (message.isSendMessageTimeOut()) {
                        viewHolder.statusTextView.setText(R.string.message_sending);
                        message.setTime(System.currentTimeMillis());
                        message.startSendMessageTimer(chatUserInterface, r -> r.refreshUiThread(false, 0));
                        chatService.sendMessage(message);
                    }
                });
            } else {
                viewHolder.statusTextView.setText(R.string.message_sending);
                viewHolder.statusTextView.setOnClickListener(null);
            }
        } else if (message.getMessageType().name().contains("SENT")) {
            if (message.getMessageType().name().contains("PRIVATE") && !message.isRead()) {
                Log.d("unread sent private message:", message.getPayload());
                viewHolder.statusTextView.setText(R.string.message_sent);
            } else {
                viewHolder.statusTextView.setText("");
            }
        } else if ( message.getMessageType().name().contains("READ") &&
                message.isRead()) {
            if (isLastSelfMessage(message)) {
                viewHolder.statusTextView.setText(R.string.message_read);
            } else {
                viewHolder.statusTextView.setText(R.string.message_empty);
            }
        }
    }

    private boolean isLastSelfMessage(Message message) {
        for (int index=messages.size() - 1; index>=0; index --) {
            if (messages.get(index).getMessageType().name().contains("READ")) {
                return messages.get(index).getMessageId().equals(message.getMessageId());
            }
        }
        return false;
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
