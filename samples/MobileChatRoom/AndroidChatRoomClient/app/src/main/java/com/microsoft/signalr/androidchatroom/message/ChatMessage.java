package com.microsoft.signalr.androidchatroom.message;

public class ChatMessage extends Message {

    private String sender;
    private String time;


    public ChatMessage(String sender, String time, String content, int messageEnum) {
        this.sender = sender;
        this.time = time;
        setContent(content);
        setMessageEnum(messageEnum);
    }

    public ChatMessage(String sender, String time, String content, String uuid, int messageEnum) {
        this.sender = sender;
        this.time = time;
        setContent(content);
        setUuid(uuid);
        setMessageEnum(messageEnum);
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
