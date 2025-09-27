package com.signalr.androidchatroom.view.chatrecyclerview;

import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.recyclerview.widget.RecyclerView;

import com.signalr.androidchatroom.R;
import com.signalr.androidchatroom.model.entity.Message;

public class ChatContentViewHolder extends RecyclerView.ViewHolder {
    /* For all non-system message */
    public final TextView messageSender;
    public final TextView messageTime;
    public final TextView statusTextView;

    /* For Text Message */
    public final TextView messageTextContent;

    /* For Image Message */
    public final ImageView messageImageContent;

    /* For System Message */
    public final TextView systemMessageContent;

    ChatContentViewHolder(View view) {
        super(view);
        this.messageSender = view.findViewById(R.id.textview_message_sender);
        this.messageTime = view.findViewById(R.id.textview_message_time);
        this.statusTextView = view.findViewById(R.id.textview_message_status);
        this.messageTextContent = view.findViewById(R.id.textview_message_content);
        this.messageImageContent = view.findViewById(R.id.imageview_message_content);
        this.systemMessageContent = view.findViewById(R.id.textview_enter_leave_content);
    }

    public void bindImageClick(final Message message, final RecyclerViewItemClickListener listener) {
        messageImageContent.setOnClickListener(v -> listener.onClickItem(message));
    }

    public void bindStatusClick(final Message message, final RecyclerViewItemClickListener listener) {
        statusTextView.setOnClickListener(v -> listener.onClickItem(message));
    }
}
