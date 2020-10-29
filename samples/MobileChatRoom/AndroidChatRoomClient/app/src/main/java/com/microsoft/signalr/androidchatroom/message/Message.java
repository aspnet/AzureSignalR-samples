package com.microsoft.signalr.androidchatroom.message;

import android.graphics.Bitmap;

import java.util.Timer;
import java.util.TimerTask;
import java.util.UUID;

public class Message {
    public final static String BROADCAST_RECEIVER = "BCAST";
    public final static String SYSTEM_SENDER = "SYS";

    private String messageId;
    private MessageTypeEnum messageType;
    private String sender;
    private String receiver;
    private String payload;
    private long time;

    private Bitmap bmp;

    private Timer sendMessageTimer;
    private boolean sendMessageTimeOut;

    private Timer pullImageTimer;
    private boolean pullImageTimeOut;

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

    public boolean isPullImageTimeOut() {
        return this.isPullImageTimeOut();
    }

    public void startPullImageTimer() {
        this.pullImageTimer = new Timer();
        this.pullImageTimeOut = false;
        long localPullTime = System.currentTimeMillis();
        pullImageTimer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                if (!pullImageTimeOut && System.currentTimeMillis() - localPullTime > 5000) {
                    pullImageTimeOut = true;
                    cancel();
                }
            }
        }, 0, 200);
    }

    public void ackPullImage(long receivedTimeInLong) {
        if (!pullImageTimeOut) {
            if (pullImageTimer != null) {
                pullImageTimer.cancel();
            }
        }
    }

    public boolean isSendMessageTimeOut() {
        return this.sendMessageTimeOut;
    }

    public void startSendMessageTimer() {
        this.sendMessageTimer = new Timer();
        this.sendMessageTimeOut = false;
        long localSendTime = this.time;
        sendMessageTimer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                if (!sendMessageTimeOut && System.currentTimeMillis() - localSendTime > 5000) {
                    sendMessageTimeOut = true;
                    cancel();
                }
            }
        }, 0, 200);
    }

    public void ack(long receivedTimeInLong) {
        if (!sendMessageTimeOut) {
            if (sendMessageTimer != null) {
                sendMessageTimer.cancel();
            }
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
}
