package com.microsoft.signalr.androidchatroom.model.entity;

import android.graphics.Bitmap;

import com.microsoft.signalr.androidchatroom.util.MessageTypeUtils;

import java.util.UUID;

/**
 * Entity class for a chat message
 */
public class Message {
    public final static String BROADCAST_RECEIVER = "BCAST";
    public final static String SYSTEM_SENDER = "SYS";

    private String messageId;
    private int messageType;
    private String sender;
    private String receiver;
    private String payload;
    private long time;

    private Bitmap bmp;

    public Message(String messageId, int messageType) {
        this.messageId = messageId;
        this.messageType = messageType;
    }

    public Message(int messageType) {
        this.messageId = UUID.randomUUID().toString();
        this.messageType = messageType;
    }

    public String getMessageId() {
        return messageId;
    }

    public void setMessageId(String messageId) {
        this.messageId = messageId;
    }

    public int getMessageType() {
        return messageType;
    }

    public void setMessageType(int messageType) {
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
        return (messageType & MessageTypeConstant.MESSAGE_CONTENT_MASK) == MessageTypeConstant.IMAGE;
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

    public boolean isRead() {
        return (messageType & MessageTypeConstant.MESSAGE_STATUS_MASK) == MessageTypeConstant.READ;
    }


    public void ack(long receivedTimeInLong) {
        if ((messageType & MessageTypeConstant.MESSAGE_STATUS_MASK) == MessageTypeConstant.SENDING) {
            // Set SENDING -> SENT
            messageType = MessageTypeUtils.setStatus(messageType, MessageTypeConstant.SENT);
            setTime(receivedTimeInLong);
        }
    }

    public void read() {
        if ((messageType & MessageTypeConstant.MESSAGE_STATUS_MASK) == MessageTypeConstant.SENT) {
            // Set SENT -> READ
            messageType = MessageTypeUtils.setStatus(messageType, MessageTypeConstant.READ);
        }
    }

    public void timeout() {
        if ((messageType & MessageTypeConstant.MESSAGE_STATUS_MASK) == MessageTypeConstant.SENDING) {
            // Set SENDING -> SENT
            messageType = MessageTypeUtils.setStatus(messageType, MessageTypeConstant.TIMEOUT);
        }
    }
}
