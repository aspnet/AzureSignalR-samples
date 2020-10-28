package com.microsoft.signalr.androidchatroom.message;

import android.graphics.Bitmap;

import java.util.UUID;

public class Message {
    public final static String BROADCAST_RECEIVER = "BCAST";
    public final static String SYSTEM_SENDER = "SYS";

    private String messageId;
    private MessageTypeEnum messageType;
    private String sender;
    private String receiver;
    private boolean isImageLoaded;
    private String payload;
    private long time;

    private Bitmap bmp;

    public Message(String messageId, MessageTypeEnum messageType) {
        this.messageId = messageId;
        this.messageType = messageType;
    }

    public Message(MessageTypeEnum messageType) {
        this.messageId = UUID.randomUUID().toString();
        this.messageType = messageType;
    }

    public String getMessageId() {
        return messageId;
    }

    public void setMessageId(String messageId) {
        this.messageId = messageId;
    }

    public MessageTypeEnum getMessageType() {
        return messageType;
    }

    public void setMessageType(MessageTypeEnum messageType) {
        this.messageType = messageType;
    }

    public String getSender() {
        return sender;
    }

    public void setSender(String sender) {
        this.sender = sender;
    }

    public String getReceiver() {
        return receiver;
    }

    public void setReceiver(String receiver) {
        this.receiver = receiver;
    }

    public boolean isImage() {
        return this.messageType.name().contains("IMAGE");
    }

    public boolean isImageLoaded() {
        return isImageLoaded;
    }

    public void setImageLoaded(boolean imageLoaded) {
        isImageLoaded = imageLoaded;
    }

    public String getPayload() {
        return payload;
    }

    public void setPayload(String payload) {
        this.payload = payload;
    }

    public long getTime() {
        return time;
    }

    public void setTime(long time) {
        this.time = time;
    }

    public Bitmap getBmp() {
        return bmp;
    }

    public void setBmp(Bitmap bmp) {
        this.bmp = bmp;
    }

    public void ack(long receivedTimeInLong) {
        switch (messageType) {
            case SENDING_TEXT_BROADCAST_MESSAGE:
                messageType = MessageTypeEnum.SENT_TEXT_BROADCAST_MESSAGE;
                break;
            case SENDING_TEXT_PRIVATE_MESSAGE:
                messageType = MessageTypeEnum.SENT_TEXT_PRIVATE_MESSAGE;
                break;
            case SENDING_IMAGE_BROADCAST_MESSAGE:
                messageType = MessageTypeEnum.SENT_IMAGE_BROADCAST_MESSAGE;
                break;
            case SENDING_IMAGE_PRIVATE_MESSAGE:
                messageType = MessageTypeEnum.SENT_IMAGE_PRIVATE_MESSAGE;
                break;
            default:
        }
        setTime(receivedTimeInLong);
    }
}
