package com.microsoft.signalr.androidchatroom.message;

import java.util.UUID;

public abstract class Message {
    public static final int SELF_SENDING_MESSAGE = 0x1;
    public static final int SELF_SENT_MESSAGE = 0x2;
    public static final int INCOMING_MESSAGE = 0x3;
    public static final int ENTER_MESSAGE = 0x4;
    public static final int LEAVE_MESSAGE = 0x5;

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
}
