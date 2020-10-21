package com.microsoft.signalr.androidchatroom.message;

import com.google.gson.JsonObject;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class ChatMessage extends Message {

    private String sender;
    private String receiver;
    private final static SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'", Locale.US);
    private final static long utcOffset = 1000 * 3600 * 8;

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

    public static ChatMessage fromJsonObject(JsonObject jsonObject, String sessionUser) {
        String messageId = jsonObject.get("MessageId").getAsString();
        String sender = jsonObject.get("Sender").getAsString();
        String receiver = jsonObject.get("Receiver").getAsString();
        String text = jsonObject.get("Text").getAsString();
        long time;
        try {
            time = sdf.parse(jsonObject.get("SendTime").getAsString()).getTime() + utcOffset;
        } catch (Exception e) {
            time = 0;
        }
        MessageType type;
        int rawType = jsonObject.get("Type").getAsInt();
        if (rawType == 0) {
            if (sender.equals(sessionUser)) {
                type = MessageType.SENT_PRIVATE_MESSAGE;
            } else {
                type = MessageType.RECEIVED_PRIVATE_MESSAGE;
            }
        } else {
            if (sender.equals(sessionUser)) {
                type = MessageType.SENT_BROADCAST_MESSAGE;
            } else {
                type = MessageType.RECEIVED_BROADCAST_MESSAGE;
            }
        }
        return new ChatMessage(messageId, sender, receiver, text, time, type);
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
