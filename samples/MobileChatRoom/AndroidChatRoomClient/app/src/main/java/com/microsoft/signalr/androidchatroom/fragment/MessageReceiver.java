package com.microsoft.signalr.androidchatroom.fragment;

import com.microsoft.signalr.androidchatroom.message.ChatMessage;
import com.microsoft.signalr.androidchatroom.message.Message;

import java.util.List;
import java.util.Set;

public interface MessageReceiver {
    void tryAddMessage(Message message);

    void tryAddAllMessages(List<Message> messages);

    void setMessageAck(String messageId);

    Set<ChatMessage> getSendingMessages();

    void showSessionExpiredDialog();
}
