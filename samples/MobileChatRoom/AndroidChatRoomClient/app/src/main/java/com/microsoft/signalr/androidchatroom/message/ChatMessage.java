package com.microsoft.signalr.androidchatroom.message;

public class ChatMessage extends Message {

    private String sender;
    private String receiver;
    private String time;

    public ChatMessage(String sender, String receiver, String time, String content, int messageEnum) {
        this.sender = sender;
        this.receiver = receiver;
        this.time = time;
        setContent(content);
        setMessageEnum(messageEnum);
    }

    public ChatMessage(String uuid, String sender, String receiver, String time, String content, int messageEnum) {
        this.sender = sender;
        this.receiver = receiver;
        this.time = time;
        setContent(content);
        setUuid(uuid);
        setMessageEnum(messageEnum);
    }

    public String getReceiver() {
        return receiver;
    }

    public void setReceiver(String receiver) {
        this.receiver = receiver;
    }

    public String getSender() {
        return sender;
    }

    public void setSender(String sender) {
        this.sender = sender;
    }

    public String getTime() {
        return time;
    }

    public void setTime(String time) {
        this.time = time;
    }

}
