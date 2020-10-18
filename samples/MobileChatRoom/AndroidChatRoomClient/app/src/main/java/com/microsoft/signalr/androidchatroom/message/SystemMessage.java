package com.microsoft.signalr.androidchatroom.message;

public class SystemMessage extends Message {

    public SystemMessage(String messageId, String text) {
        setMessageType(MessageType.SYSTEM_MESSAGE);
        setMessageId(messageId);
        setText(text);
    }
}
