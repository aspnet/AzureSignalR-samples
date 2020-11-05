package com.microsoft.signalr.androidchatroom.fragment;

import android.view.View;
import android.widget.EditText;

import com.microsoft.signalr.androidchatroom.message.Message;
import com.microsoft.signalr.androidchatroom.message.MessageFactory;
import com.microsoft.signalr.androidchatroom.service.ChatService;

public class SendButtonHandler implements View.OnClickListener {

    private final ChatFragment chatFragment;
    private final ChatService chatService;

    public SendButtonHandler(ChatFragment chatFragment, ChatService chatService) {
        this.chatFragment = chatFragment;
        this.chatService = chatService;
    }

    @Override
    public void onClick(View v) {
        if (chatFragment.getChatBoxMessageEditText().getText().length() > 0) { // Empty message not allowed
            // Create and add message into list
            Message chatMessage = createTextMessage();
            chatFragment.tryAddMessage(chatMessage, 0);

            // If hubConnection is active then send message
            chatMessage.startSendMessageTimer(chatFragment, r -> r.refreshUiThread(false,0));
            chatService.sendMessage(chatMessage);

            // Refresh ui
            chatFragment.refreshUiThread(false,1);
        }
    }

    private Message createTextMessage() {
        // Receiver
        String receiver = chatFragment.getChatBoxReceiverEditText().getText().toString();

        // Payload
        String messageContent = chatFragment.getChatBoxMessageEditText().getText().toString();
        chatFragment.getChatBoxMessageEditText().getText().clear();

        // Receiver field length == 0 -> broadcast message
        boolean isBroadcastMessage = receiver.length() == 0;

        Message chatMessage;
        if (isBroadcastMessage) {
            chatMessage = MessageFactory.createSendingTextBroadcastMessage(chatFragment.getUsername(), messageContent, System.currentTimeMillis());
        } else {
            chatMessage = MessageFactory.createSendingTextPrivateMessage(chatFragment.getUsername(), receiver, messageContent, System.currentTimeMillis());
        }
        return chatMessage;
    }
}
