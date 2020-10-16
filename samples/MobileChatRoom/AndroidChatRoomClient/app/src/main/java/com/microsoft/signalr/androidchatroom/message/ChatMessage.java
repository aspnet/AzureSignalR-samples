package com.microsoft.signalr.androidchatroom.message;

public class ChatMessage extends Message {

    private String sender;
    private String receiver;
    private String time;

    public ChatMessage(String sender, String receiver, String text, String time, int messageEnum) {
        this.sender = sender;
        this.receiver = receiver;
        this.time = time;
        setText(text);
        setMessageEnum(messageEnum);
    }

    public ChatMessage(String messageId, String sender, String receiver, String text, String time, int messageEnum) {
        this.sender = sender;
        this.receiver = receiver;
        this.time = time;
        setText(text);
        setMessageId(messageId);
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
