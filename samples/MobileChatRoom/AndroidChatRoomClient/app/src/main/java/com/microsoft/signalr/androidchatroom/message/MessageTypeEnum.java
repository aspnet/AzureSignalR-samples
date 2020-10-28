package com.microsoft.signalr.androidchatroom.message;

public enum MessageTypeEnum {
    SYSTEM_MESSAGE(0),
    SENDING_TEXT_BROADCAST_MESSAGE(1),
    SENDING_TEXT_PRIVATE_MESSAGE(2),
    SENT_TEXT_BROADCAST_MESSAGE(3),
    SENT_TEXT_PRIVATE_MESSAGE(4),
    RECEIVED_TEXT_BROADCAST_MESSAGE(5),
    RECEIVED_TEXT_PRIVATE_MESSAGE(6),
    SENDING_IMAGE_BROADCAST_MESSAGE(7),
    SENDING_IMAGE_PRIVATE_MESSAGE(8),
    SENT_IMAGE_BROADCAST_MESSAGE(9),
    SENT_IMAGE_PRIVATE_MESSAGE(10),
    RECEIVED_IMAGE_BROADCAST_MESSAGE(11),
    RECEIVED_IMAGE_PRIVATE_MESSAGE(12);

    private final int value;

    MessageTypeEnum(int value) {
        this.value = value;
    }

    public int getValue() {
        return this.value;
    }
}
