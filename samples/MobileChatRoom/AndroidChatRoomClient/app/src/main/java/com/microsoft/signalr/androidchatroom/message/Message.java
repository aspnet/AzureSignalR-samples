package com.microsoft.signalr.androidchatroom.message;

import java.util.UUID;

public abstract class Message {
    private MessageType messageType;
    private String text;
    private String messageId;
    private long time;

    public Message() {
        this.messageId = UUID.randomUUID().toString();
    }

    public MessageType getMessageType() {
        return messageType;
    }

    public void setMessageType(MessageType messageType) {
        this.messageType = messageType;
    }

    protected void setMessageId(String messageId) {
        this.messageId = messageId;
    }

    public String getMessageId() {
        return messageId;
    }

    public String getText() {
        return text;
    }

    public void setText(String text) {
        this.text = text;
    }

    public long getTime() {
        return time;
    }

    public void setTime(long time) {
        this.time = time;
    }

    public void ack() {
        switch (messageType) {
            case SENDING_BROADCAST_MESSAGE:
                messageType = MessageType.SENT_BROADCAST_MESSAGE;
                break;
            case SENDING_PRIVATE_MESSAGE:
                messageType = MessageType.SENT_PRIVATE_MESSAGE;
                break;
            default:
        }
    }
}
