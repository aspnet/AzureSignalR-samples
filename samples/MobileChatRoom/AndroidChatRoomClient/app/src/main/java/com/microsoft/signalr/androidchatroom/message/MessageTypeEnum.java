package com.microsoft.signalr.androidchatroom.message;

public enum MessageTypeEnum {
    READ_IMAGE_PRIVATE_MESSAGE(0),
    READ_TEXT_PRIVATE_MESSAGE(1),
    RECEIVED_IMAGE_BROADCAST_MESSAGE(2),
    RECEIVED_IMAGE_PRIVATE_MESSAGE(3),
    RECEIVED_TEXT_BROADCAST_MESSAGE(4),
    RECEIVED_TEXT_PRIVATE_MESSAGE(5),
    SYSTEM_MESSAGE(6),
    SENDING_IMAGE_BROADCAST_MESSAGE(7),
    SENDING_IMAGE_PRIVATE_MESSAGE(8),
    SENDING_TEXT_BROADCAST_MESSAGE(9),
    SENDING_TEXT_PRIVATE_MESSAGE(10),
    SENT_IMAGE_BROADCAST_MESSAGE(11),
    SENT_IMAGE_PRIVATE_MESSAGE(12),
    SENT_TEXT_BROADCAST_MESSAGE(13),
    SENT_TEXT_PRIVATE_MESSAGE(14);

    private final int value;

    MessageTypeEnum(int value) {
        this.value = value;
    }

    public int getValue() {
        return this.value;
    }
}
