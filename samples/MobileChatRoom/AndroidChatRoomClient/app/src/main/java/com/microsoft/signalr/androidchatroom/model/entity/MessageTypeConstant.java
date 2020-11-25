package com.microsoft.signalr.androidchatroom.model.entity;

/**
 * Defines constants representing message types.
 * 0000 0000
 *   ^^ ^^^^
 * Use lower 6 bits of a int variable to represent complete information about message type.
 *
 * 1. Message type: {System, Broadcast, Private}
 * 0000 0000
 *        ^^
 * 2. Message content: {Text, Image}
 * 0000 0000
 *       ^
 * 3. Message status: {Received, Sending, Sent, Timeout, Read}
 * 0000 0000
 *   ^^ ^
 */
public class MessageTypeConstant {

    /* MESSAGE_TYPE_MASK 0000 0011 */
    public static final int MESSAGE_TYPE_MASK = 0x3;
    public static final int SYSTEM = 0x0;
    public static final int BROADCAST = 0x1;
    public static final int PRIVATE = 0x2;

    /* MESSAGE_CONTENT_MASK 0000 0100 */
    public static final int MESSAGE_CONTENT_MASK = 0x4;
    public static final int TEXT = 0x0;
    public static final int IMAGE = 0x4;

    /* MESSAGE_STATUS_MASK 0011 1000 */
    public static final int MESSAGE_STATUS_MASK = 0x38;
    public static final int RECEIVED = 0x0;
    public static final int SENDING = 0x8;
    public static final int SENT = 0x10;
    public static final int TIMEOUT = 0x18;
    public static final int READ = 0x20;
}
