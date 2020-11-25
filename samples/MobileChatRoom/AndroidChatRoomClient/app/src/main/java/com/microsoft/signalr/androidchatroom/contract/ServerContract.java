package com.microsoft.signalr.androidchatroom.contract;

/**
 * Defines server callbacks
 */
public interface ServerContract {
    /**
     * Receives a system message from SignalR server.
     *
     * @param messageId A server-generated unique string of message id.
     * @param payload Body content of the system message (can only be text).
     * @param sendTime A long int representing the time when the system was sent.
     */
    void receiveSystemMessage(String messageId, String payload, long sendTime);

    /**
     * Received a broadcast message from SignalR server.
     *
     * @param messageId A server-generated unique string of message id.
     * @param sender A string of sender.
     * @param receiver A string of receiver (can only be "BCAST").
     * @param payload Body content of the broadcast message (can be either text
     *                or binary image contents encoded in base64).
     * @param isImage A boolean indicating whether the broadcast is an image message.
     * @param sendTime A long int representing the time when the system was sent.
     * @param ackId A string of ack id that client will later send back to server as Ack response.
     */
    void receiveBroadcastMessage(String messageId, String sender, String receiver, String payload, boolean isImage, long sendTime, String ackId);

    /**
     * Received a private message from SignalR server.
     *
     * @param messageId A server-generated unique string of message id.
     * @param sender A string of sender.
     * @param receiver A string of receiver.
     * @param payload Body content of the broadcast message (can be either text
     *                or binary image contents encoded in base64).
     * @param isImage A boolean indicating whether the broadcast is an image message.
     * @param sendTime A long int representing the time when the system was sent.
     * @param ackId A string of ack id that client will later send back to server as Ack response.
     */
    void receivePrivateMessage(String messageId, String sender, String receiver, String payload, boolean isImage, long sendTime, String ackId);

    /**
     * Receives history messages from SignalR server.
     * Usually called by server right after a client pullHistoryMessage request.
     *
     * @param serializedString A json string of list of history messages.
     */
    void receiveHistoryMessages(String serializedString);

    /**
     * Receives image content from SignalR server.
     *
     * @param messageId A string representing a message id.
     * @param payload Binary image contents encoded in base64.
     */
    void receiveImageContent(String messageId, String payload);

    /**
     * Receives a client read response from SignalR server.
     * The client read response is sent from another client to server.
     *
     * @param messageId A string representing a message id.
     * @param username A string representing the username of sender of the read response.
     */
    void clientRead(String messageId, String username);

    /**
     * Receives a server ack from SignalR server.
     * The server ack is a server response of successfully receiving a client message.
     *
     * @param messageId A string representing a message id.
     * @param receivedTimeInLong A long int representing the received time in milliseconds.
     */
    void serverAck(String messageId, long receivedTimeInLong);

    /**
     * Expires a client session from SignalR server.
     *
     * @param isForced A boolean indicating whether the expire is forced.
     */
    void expireSession(boolean isForced);

}
