package com.microsoft.signalr.androidchatroom.message;

public class ChatMessage extends Message {

    private String sender;
    private String receiver;


    public ChatMessage(String sender, String receiver, String text, long time, MessageType messageType) {
        this.sender = sender;
        this.receiver = receiver;
        setTime(time);
        setText(text);
        setMessageType(messageType);
    }

    public ChatMessage(String messageId, String sender, String receiver, String text, long time, MessageType messageType) {
        this.sender = sender;
        this.receiver = receiver;
        setTime(time);
        setText(text);
        setMessageId(messageId);
        setMessageType(messageType);
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

}
