package com.signalr.androidchatroom.model.entity;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.util.Log;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.signalr.androidchatroom.util.MessageTypeUtils;
import com.signalr.androidchatroom.util.SimpleCallback;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Base64;
import java.util.Date;
import java.util.List;
import java.util.Locale;

/**
 * Defines factory methods for manipulating Message class
 */
public class MessageFactory {
    /* SimpleDateFormat utility object for date format decoding and encoding */
    private final static SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'", Locale.US);
    
    /* Offset in millisecond between GMT+0 and GMT+8 (8 hours) */
    private final static long utcOffset = 1000 * 3600 * 8;
    
    /* Gson utility object for JSON decoding and encoding */
    private final static Gson gson = new Gson();

    /**
     * Creates a received text broadcast message.
     * 
     * @param messageId A string of message id.
     * @param sender A string of sender client username.
     * @param payload A string of text message body.
     * @param time A long int of send time of the message in milliseconds.
     * @return A corresponding Message object created.
     */
    public static Message createReceivedTextBroadcastMessage(String messageId, String sender, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.BROADCAST, MessageTypeConstant.TEXT, MessageTypeConstant.RECEIVED));
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    /**
     * Creates a received image broadcast message.
     *
     * @param messageId A string of message id.
     * @param sender A string of sender client username.
     * @param payload A string of base64 encoded image content.
     * @param time A long int of send time of the message in milliseconds.
     * @return A corresponding Message object created.
     */
    public static Message createReceivedImageBroadcastMessage(String messageId, String sender, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.BROADCAST, MessageTypeConstant.IMAGE, MessageTypeConstant.RECEIVED));
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    /**
     * Creates a sending text broadcast message.
     * 
     * @param sender A string of sender client username.
     * @param payload A string of text message body.
     * @param time A long int of send time of the message in milliseconds.
     * @return A corresponding Message object created.
     */
    public static Message createSendingTextBroadcastMessage(String sender, String payload, long time) {
        Message message = new Message(MessageTypeUtils.calculateMessageType(MessageTypeConstant.BROADCAST, MessageTypeConstant.TEXT, MessageTypeConstant.SENDING));
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    /**
     * Creates a sending image broadcast message.
     *
     * @param sender A string of sender client username.
     * @param bmp A bitmap representation of image.
     * @param time A long int of send time of the message in milliseconds.
     * @param callback A callback function called when send completed
     * @return A corresponding Message object created.
     */
    public static Message createSendingImageBroadcastMessage(String sender, Bitmap bmp, long time, SimpleCallback<Message> callback) {
        Message message = new Message(MessageTypeUtils.calculateMessageType(MessageTypeConstant.BROADCAST, MessageTypeConstant.IMAGE, MessageTypeConstant.SENDING));
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setBmp(bmp);
        new Thread(() -> {
            String payload = encodeToBase64(bmp);
            message.setPayload(payload);
            callback.onSuccess(message);
        }).start();
        message.setTime(time);
        return message;
    }

    /**
     * Creates a received text private message.
     *
     * @param messageId A string of message id.
     * @param sender A string of sender client username.
     * @param receiver A string of receiver client username.
     * @param payload A string of text message body.
     * @param time A long int of send time of the message in milliseconds.
     * @return A corresponding Message object created.
     */
    public static Message createReceivedTextPrivateMessage(String messageId, String sender, String receiver, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.PRIVATE, MessageTypeConstant.TEXT, MessageTypeConstant.RECEIVED));
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    /**
     * Creates a received image private message.
     *
     * @param messageId A string of message id.
     * @param sender A string of sender client username.
     * @param receiver A string of receiver client username.
     * @param payload A string of base64 encoded image content.
     * @param time A long int of send time of the message in milliseconds.
     * @return A corresponding Message object created.
     */
    public static Message createReceivedImagePrivateMessage(String messageId, String sender, String receiver, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.PRIVATE, MessageTypeConstant.IMAGE, MessageTypeConstant.RECEIVED));
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    /**
     * Creates a sending text private message.
     *
     * @param sender A string of sender client username.
     * @param receiver A string of receiver client username.
     * @param payload A string of text message body.
     * @param time A long int of send time of the message in milliseconds.
     * @return A corresponding Message object created.
     */
    public static Message createSendingTextPrivateMessage(String sender, String receiver, String payload, long time) {
        Message message = new Message(MessageTypeUtils.calculateMessageType(MessageTypeConstant.PRIVATE, MessageTypeConstant.TEXT, MessageTypeConstant.SENDING));
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    /**
     * Creates a sending image private message.
     *
     * @param sender A string of sender client username.
     * @param receiver A string of receiver client username.
     * @param bmp A bitmap representation of image.
     * @param time A long int of send time of the message in milliseconds.
     * @return A corresponding Message object created.
     */
    public static Message createSendingImagePrivateMessage(String sender, String receiver, Bitmap bmp, long time, SimpleCallback<Message> callback) {
        Message message = new Message(MessageTypeUtils.calculateMessageType(MessageTypeConstant.PRIVATE, MessageTypeConstant.IMAGE, MessageTypeConstant.SENDING));
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setBmp(bmp);
        new Thread(() -> {
            String payload = encodeToBase64(bmp);
            message.setPayload(payload);
            callback.onSuccess(message);
        }).start();
        message.setTime(time);
        return message;
    }

    /**
     * Creates a received system message.
     * 
     * @param messageId A string of message id.
     * @param payload A string of text message body.
     * @param time A long int of send time of the message in milliseconds.
     * @return A corresponding Message object created.
     */
    public static Message createReceivedSystemMessage(String messageId, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.SYSTEM, MessageTypeConstant.TEXT, MessageTypeConstant.RECEIVED));
        message.setSender(Message.SYSTEM_SENDER);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    /**
     * Converts a message from a json object.
     *
     * @param jsonObject A json object of message.
     * @param sessionUser A string of current client username.
     * @return A converted Message object.
     */
    private static Message fromJsonObject(JsonObject jsonObject, String sessionUser) {
        String messageId = jsonObject.get("MessageId").getAsString();
        String sender = jsonObject.get("Sender").getAsString();
        String receiver = jsonObject.get("Receiver").getAsString();
        String payload = jsonObject.get("Payload").getAsString();
        boolean isImage = jsonObject.get("IsImage").getAsBoolean();
        boolean isRead = jsonObject.get("IsRead").getAsBoolean();
        int rawType = jsonObject.get("Type").getAsInt();
        boolean isSelf = sender.equals(sessionUser);
        boolean isPrivate = MessageTypeUtils.convertCSharpRawTypeToMessageTypeConstant(rawType)
                == MessageTypeConstant.PRIVATE;
        long time;
        try {
            time = sdf.parse(jsonObject.get("SendTime").getAsString()).getTime() + utcOffset;
        } catch (Exception e) {
            time = 0;
        }

        int messageType = MessageTypeUtils.calculateMessageType(isImage, isSelf, isPrivate, isRead);

        Message message = new Message(messageId, messageType);
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);

        return message;
    }

    /**
     * Parse a list of history messages from a serialized JSON string.
     *
     * @param serializedString A JSON string.
     * @param sessionUser A string of current client username.
     * @return A list of parsed history messages.
     */
    public static List<Message> parseHistoryMessages(String serializedString, String sessionUser) {
        List<Message> historyMessages = new ArrayList<>();
        JsonArray jsonArray = gson.fromJson(serializedString, JsonArray.class);
        for (JsonElement jsonElement : jsonArray) {
            Message chatMessage = fromJsonObject(jsonElement.getAsJsonObject(), sessionUser);
            historyMessages.add(chatMessage);
        }
        return historyMessages;
    }

    /**
     * Serializes a list of history messages to a JSON string.
     *
     * @param messages A list of history messages.
     * @return A serialized JSON string.
     */
    public static String serializeHistoryMessages(List<Message> messages) {
        JsonArray jsonArray = new JsonArray();
        for (Message message : messages) {
            /* We only serialize and store non-system messages. */
            if ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK)
                != MessageTypeConstant.SYSTEM) {
                JsonObject jsonObject = toJsonObject(message);
                jsonArray.add(jsonObject);
            }

        }

        return jsonArray.toString();
    }

    /**
     * Converts a json object from a message.
     *
     * @param message A message to convert.
     * @return A converted json object.
     */
    private static JsonObject toJsonObject(Message message) {
        JsonObject jsonObject = new JsonObject();
        jsonObject.add("MessageId",gson.toJsonTree(message.getMessageId()));
        jsonObject.add("Sender",gson.toJsonTree(message.getSender()));
        jsonObject.add("Receiver",gson.toJsonTree(message.getReceiver()));
        if (message.isImage()) {
            jsonObject.add("Payload",gson.toJsonTree(""));
        } else {
            jsonObject.add("Payload",gson.toJsonTree(message.getPayload()));
        }
        jsonObject.add("IsRead",gson.toJsonTree(message.isRead()));
        jsonObject.add("IsImage",gson.toJsonTree(message.isImage()));
        Date date = new Date(message.getTime() - utcOffset);
        jsonObject.add("SendTime", gson.toJsonTree(sdf.format(date)));
        if (message.getReceiver().equals(Message.BROADCAST_RECEIVER)) {
            jsonObject.add("Type", gson.toJsonTree(2));
        } else if (message.getSender().equals(Message.SYSTEM_SENDER)) {
            jsonObject.add("Type", gson.toJsonTree(1));
        } else {
            jsonObject.add("Type", gson.toJsonTree(0));
        }
        return jsonObject;
    }

    /**
     * Encodes a bitmap to base64 string.
     *
     * @param bmp A bitmap to encode.
     * @return The encoded base64 string.
     */
    public static String encodeToBase64(Bitmap bmp) {
        /* Empty by default */
        String messageImageContent = "";

        try (ByteArrayOutputStream stream = new ByteArrayOutputStream()) {
            bmp.compress(Bitmap.CompressFormat.JPEG, 25, stream);
            byte[] byteArray = stream.toByteArray();
            messageImageContent = Base64.getEncoder().encodeToString(byteArray);
        } catch (IOException ioe) {
            Log.e("createImageMessage", ioe.getLocalizedMessage());
        }

        return messageImageContent;
    }

    /**
     * Decodes a base64 string to a bitmap object.
     *
     * @param payload The base64 string to decode.
     * @return The decoded image bitmap.
     */
    public static Bitmap decodeToBitmap(String payload) {
        byte[] byteArray = Base64.getDecoder().decode(payload);
        return BitmapFactory.decodeByteArray(byteArray, 0, byteArray.length);
    }
}
