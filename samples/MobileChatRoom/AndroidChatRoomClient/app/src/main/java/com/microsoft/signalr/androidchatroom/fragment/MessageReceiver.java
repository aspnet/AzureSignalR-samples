package com.microsoft.signalr.androidchatroom.fragment;

import com.microsoft.signalr.androidchatroom.message.ChatMessage;
import com.microsoft.signalr.androidchatroom.message.Message;

import java.util.List;
import java.util.Set;

public interface MessageReceiver {

    void activate();

    void tryAddMessage(Message message, int direction);

    void tryAddAllMessages(List<Message> messages, int direction);

    void setMessageAck(String messageId, long receivedTimeInLong);

    Set<ChatMessage> getSendingMessages();

    void showSessionExpiredDialog();
}
