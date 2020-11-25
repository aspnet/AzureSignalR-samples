package com.microsoft.signalr.androidchatroom.util;

import com.microsoft.signalr.androidchatroom.model.entity.MessageTypeConstant;

/**
 * Utility class for MessageTypeConstant manipulation.
 */
public class MessageTypeUtils {

    /**
     * Sets the MESSAGE_TYPE bits to type.
     *
     * 0000 0000
     *        ^^
     *
     * @param messageType The complete int returned after calling Message.getMessageType()
     * @param type Choose among {MessageTypeConstant.SYSTEM, MessageTypeConstant.BROADCAST,
     *             MessageTypeConstant.PRIVATE}
     * @return The complete bit of modified messageType.
     */
    public static int setType(int messageType, int type) {
        /* Clear type field */
        messageType = messageType & (~MessageTypeConstant.MESSAGE_TYPE_MASK);

        /* Set type field */
        messageType = messageType | type;

        return messageType;
    }

    /**
     * Sets the MESSAGE_CONTENT bits to content.
     *
     * 0000 0000
     *       ^
     *
     * @param messageType The complete int returned after calling Message.getMessageType()
     * @param content Choose between {MessageTypeConstant.TEXT, MessageTypeConstant.IMAGE}
     * @return The complete bit of modified messageType.
     */
    public static int setContent(int messageType, int content) {
        /* Clear content field */
        messageType = messageType & (~MessageTypeConstant.MESSAGE_CONTENT_MASK);

        /* Set content field */
        messageType = messageType | content;

        return messageType;
    }

    /**
     * Sets the MESSAGE_STATUS bits to status.
     *
     * 0000 0000
     *   ^^ ^
     *
     * @param messageType The complete int returned after calling Message.getMessageType()
     * @param status Choose among {MessageTypeConstant.RECEIVED, MessageTypeConstant.SENDING,
     *               MessageTypeConstant.SENT, MessageTypeConstant.TIMEOUT, MessageTypeConstant.READ}
     * @return The complete bit of modified messageType.
     */
    public static int setStatus(int messageType, int status) {
        /* Clear status field */
        messageType = messageType & (~MessageTypeConstant.MESSAGE_STATUS_MASK);

        /* Set status field */
        messageType = messageType | status;

        return messageType;
    }

    /**
     * Calculate the complete messageType bits given different MessageTypeConstant values.
     *
     * @param type Choose among {MessageTypeConstant.SYSTEM, MessageTypeConstant.BROADCAST,
     *             MessageTypeConstant.PRIVATE}
     * @param content Choose between {MessageTypeConstant.TEXT, MessageTypeConstant.IMAGE}
     * @param status Choose among {MessageTypeConstant.RECEIVED, MessageTypeConstant.SENDING,
     *               MessageTypeConstant.SENT, MessageTypeConstant.TIMEOUT, MessageTypeConstant.READ}
     * @return The complete bit of messageType.
     */
    public static int calculateMessageType(int type, int content, int status) {
        return type | content | status;
    }

    /**
     * Calculates the complete messageType bits given different flags.
     *
     * @param isImage If is an image.
     * @param isSelf If is send by user him(her)self.
     * @param isPrivate If is a private message.
     * @param isRead If is read.
     * @return The complete bit of messageType.
     */
    public static int calculateMessageType(boolean isImage, boolean isSelf, boolean isPrivate, boolean isRead) {
        int messageType;
        int type = isPrivate ? MessageTypeConstant.PRIVATE : MessageTypeConstant.BROADCAST;
        int content = isImage ? MessageTypeConstant.IMAGE : MessageTypeConstant.TEXT;
        int status = isSelf ? (isRead && isPrivate ? MessageTypeConstant.READ : MessageTypeConstant.SENT) : MessageTypeConstant.RECEIVED;
        messageType = MessageTypeUtils.calculateMessageType(type, content, status);
        return messageType;
    }

    /**
     * Converts the C# raw type to MessageTypeConstant.
     *
     * @param rawType A int of C# rawType passed by JSON string.
     * @return The corresponding MessageTypeConstant.
     */
    public static int convertCSharpRawTypeToMessageTypeConstant(int rawType) {
        /* In C#'s MessageTypeEnum 0 is private message;
         * 1 is broadcast message;
         * 2 is system message.
         */
        switch (rawType) {
            case 0:
                return MessageTypeConstant.PRIVATE;
            case 1:
                return MessageTypeConstant.BROADCAST;
            case 2:
            default:
                return MessageTypeConstant.SYSTEM;
        }
    }
}
