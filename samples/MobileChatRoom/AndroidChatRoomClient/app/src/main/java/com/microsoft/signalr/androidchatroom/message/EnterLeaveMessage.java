package com.microsoft.signalr.androidchatroom.message;

public class EnterLeaveMessage extends Message {

    public EnterLeaveMessage(String username, int messageEnum) {
        setMessageEnum(messageEnum);
        switch (messageEnum) {
            case ENTER_MESSAGE:
                setContent(String.format("%s has joined the chat", username));
                break;
            case LEAVE_MESSAGE:
            default:
                setContent(String.format("%s has left the chat", username));
        }
    }
}
