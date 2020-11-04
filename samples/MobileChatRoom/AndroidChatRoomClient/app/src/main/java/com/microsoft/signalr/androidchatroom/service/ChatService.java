package com.microsoft.signalr.androidchatroom.service;

import com.microsoft.signalr.androidchatroom.fragment.MessageReceiver;
import com.microsoft.signalr.androidchatroom.message.Message;

public interface ChatService {
    //// Message sending methods
    void sendMessage(Message chatMessage);
    void sendMessageRead(String messageId);

    //// Session management methods
    void startSession();
    void expireSession(boolean showAlert);

    //// register methods
    void register(String username, String deviceUuid, MessageReceiver messageReceiver);

    //// Pulling methods
    void pullHistoryMessages(long untilTimeInLong);
    void pullImageContent(String messageId);
}
