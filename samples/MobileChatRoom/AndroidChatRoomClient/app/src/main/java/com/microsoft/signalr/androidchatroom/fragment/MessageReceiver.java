package com.microsoft.signalr.androidchatroom.fragment;

import com.microsoft.signalr.androidchatroom.message.ChatMessage;
import com.microsoft.signalr.androidchatroom.message.Message;

import java.util.Set;

public interface MessageReceiver {
    void tryAddMessage(Message message);

    void setMessageAck(String messageId);

    Set<ChatMessage> getSendingMessages();

    void showSessionExpiredDialog();
}
