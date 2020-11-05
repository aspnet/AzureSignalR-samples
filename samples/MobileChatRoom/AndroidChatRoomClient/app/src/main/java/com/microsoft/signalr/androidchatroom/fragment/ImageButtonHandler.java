package com.microsoft.signalr.androidchatroom.fragment;

import android.content.Intent;
import android.graphics.Bitmap;
import android.view.View;

import com.microsoft.signalr.androidchatroom.message.Message;
import com.microsoft.signalr.androidchatroom.message.MessageFactory;
import com.microsoft.signalr.androidchatroom.service.ChatService;

public class ImageButtonHandler implements View.OnClickListener {
    private final ChatFragment chatFragment;
    private final ChatService chatService;

    public ImageButtonHandler(ChatFragment chatFragment, ChatService chatService) {
        this.chatFragment = chatFragment;
        this.chatService = chatService;
    }

    @Override
    public void onClick(View v) {
        Intent photoPickerIntent = new Intent(Intent.ACTION_PICK);
        photoPickerIntent.setType("image/*");
        chatFragment.startActivityForResult(photoPickerIntent, ChatFragment.RESULT_LOAD_IMAGE);
    }


    public Message createAndSendImageMessage(Bitmap bmp) {
        // Receiver
        String receiver = chatFragment.getChatBoxReceiverEditText().getText().toString();

        // Payload
        String messageContent = chatFragment.getChatBoxMessageEditText().getText().toString();
        chatFragment.getChatBoxMessageEditText().getText().clear();

        // Receiver field length == 0 -> broadcast message
        boolean isBroadcastMessage = receiver.length() == 0;

        Message chatMessage;
        if (isBroadcastMessage) {
            chatMessage = MessageFactory.createSendingImageBroadcastMessage(chatFragment.getUsername(), bmp, System.currentTimeMillis(), m -> {
                m.startSendMessageTimer(chatFragment, r -> r.refreshUiThread(false, 0));
                chatService.sendMessage(m);
            });
        } else {
            chatMessage = MessageFactory.createSendingImagePrivateMessage(chatFragment.getUsername(), receiver, bmp, System.currentTimeMillis(), m -> {
                m.startSendMessageTimer(chatFragment, r -> r.refreshUiThread(false,0));
                chatService.sendMessage(m);
            });
        }

        return chatMessage;
    }
}
