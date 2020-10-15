package com.microsoft.signalr.androidchatroom.message;

import java.util.UUID;

public abstract class Message {
    public static final int SENDING_BROADCAST_MESSAGE = 0x1;
    public static final int SENDING_PRIVATE_MESSAGE = 0x2;
    public static final int SENT_BROADCAST_MESSAGE = 0x3;
    public static final int SENT_PRIVATE_MESSAGE = 0x4;
    public static final int RECEIVED_BROADCAST_MESSAGE = 0x5;
    public static final int RECEIVED_PRIVATE_MESSAGE = 0x6;
    public static final int ENTER_MESSAGE = 0x7;
    public static final int LEAVE_MESSAGE = 0x8;

    private int messageEnum;
    private String content;
    private String uuid;

    public Message() {
        this.uuid = UUID.randomUUID().toString();
    }

    public int getMessageEnum() {
        return messageEnum;
    }

    public void setMessageEnum(int messageEnum) {
        this.messageEnum = messageEnum;
    }

    protected void setUuid(String uuid) {
        this.uuid = uuid;
    }

    public String getUuid() {
        return uuid;
    }

    public String getContent() {
        return content;
    }

    public void setContent(String content) {
        this.content = content;
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
