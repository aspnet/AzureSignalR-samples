package com.microsoft.signalr.androidchatroom.service;

import com.microsoft.signalr.androidchatroom.fragment.MessageReceiver;

public interface ChatService {
    //// Session management methods
    void startSession();

    void expireSession();

    //// Message methods called by server
    void broadcastSystemMessage(String messageId, String text);

    void displayBroadcastMessage(String messageId, String sender, String receiver, String text, long sendTime, String ackId);

    void displayPrivateMessage(String messageId, String sender, String receiver, String text, long sendTime, String ackId);

    void serverAck(String messageId);

    //// register methods
    void register(String username, String deviceToken, MessageReceiver messageReceiver);
}
