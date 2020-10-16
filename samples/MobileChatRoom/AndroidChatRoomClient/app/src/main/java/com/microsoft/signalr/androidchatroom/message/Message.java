package com.microsoft.signalr.androidchatroom.message;

import java.util.UUID;

public abstract class Message {
    public static final int SENDING_BROADCAST_MESSAGE = 0x1;
    public static final int SENDING_PRIVATE_MESSAGE = 0x2;
    public static final int SENT_BROADCAST_MESSAGE = 0x3;
    public static final int SENT_PRIVATE_MESSAGE = 0x4;
    public static final int RECEIVED_BROADCAST_MESSAGE = 0x5;
    public static final int RECEIVED_PRIVATE_MESSAGE = 0x6;
    public static final int SYSTEM_MESSAGE = 0x7;

    private int messageEnum;
    private String text;
    private String messageId;

    public Message() {
        this.messageId = UUID.randomUUID().toString();
    }

    public int getMessageEnum() {
        return messageEnum;
    }

    public void setMessageEnum(int messageEnum) {
        this.messageEnum = messageEnum;
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

    public void ack() {
        switch (messageEnum) {
            case SENDING_BROADCAST_MESSAGE:
                messageEnum = SENT_BROADCAST_MESSAGE;
                break;
            case SENDING_PRIVATE_MESSAGE:
                messageEnum = SENT_PRIVATE_MESSAGE;
                break;
            default:
        }
    }
}
