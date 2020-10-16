package com.microsoft.signalr.androidchatroom.message;

public class SystemMessage extends Message {

    public SystemMessage(String messageId, String text) {
        setMessageEnum(SYSTEM_MESSAGE);
        setMessageId(messageId);
        setText(text);
    }
}
