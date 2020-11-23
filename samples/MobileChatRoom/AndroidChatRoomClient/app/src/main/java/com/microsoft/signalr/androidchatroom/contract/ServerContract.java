package com.microsoft.signalr.androidchatroom.contract;

// Define server callbacks
public interface ServerContract {

    void receiveSystemMessage(String messageId, String payload, long sendTime);

    void receiveBroadcastMessage(String messageId, String sender, String receiver, String payload, boolean isImage, long sendTime, String ackId);

    void receivePrivateMessage(String messageId, String sender, String receiver, String payload, boolean isImage, long sendTime, String ackId);

    void receiveHistoryMessages(String serializedString);

    void receiveImageContent(String messageId, String payload);

    void clientRead(String messageId, String username);

    void serverAck(String messageId, long receivedTimeInLong);

    void expireSession(boolean isForced);

}
