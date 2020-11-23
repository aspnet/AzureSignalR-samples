package com.microsoft.signalr.androidchatroom.model.entity;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.util.Log;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.reflect.TypeToken;
import com.microsoft.signalr.androidchatroom.util.MessageTypeUtils;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;

import org.json.JSONArray;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Base64;
import java.util.Date;
import java.util.List;
import java.util.Locale;

public class MessageFactory {
    private final static SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'", Locale.US);
    private final static long utcOffset = 1000 * 3600 * 8;
    private final static Gson gson = new Gson();

    public static Message createReceivedTextBroadcastMessage(String messageId, String sender, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.BROADCAST, MessageTypeConstant.TEXT, MessageTypeConstant.RECEIVED));
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createReceivedImageBroadcastMessage(String messageId, String sender, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.BROADCAST, MessageTypeConstant.IMAGE, MessageTypeConstant.RECEIVED));
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createSendingTextBroadcastMessage(String sender, String payload, long time) {
        Message message = new Message(MessageTypeUtils.calculateMessageType(MessageTypeConstant.BROADCAST, MessageTypeConstant.TEXT, MessageTypeConstant.SENDING));
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

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

    public static Message createReceivedTextPrivateMessage(String messageId, String sender, String receiver, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.PRIVATE, MessageTypeConstant.TEXT, MessageTypeConstant.RECEIVED));
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createReceivedImagePrivateMessage(String messageId, String sender, String receiver, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.PRIVATE, MessageTypeConstant.IMAGE, MessageTypeConstant.RECEIVED));
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createSendingTextPrivateMessage(String sender, String receiver, String payload, long time) {
        Message message = new Message(MessageTypeUtils.calculateMessageType(MessageTypeConstant.PRIVATE, MessageTypeConstant.TEXT, MessageTypeConstant.SENDING));
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

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

    public static Message createReceivedSystemMessage(String messageId, String payload, long time) {
        Message message = new Message(messageId, MessageTypeUtils.calculateMessageType(MessageTypeConstant.SYSTEM, MessageTypeConstant.TEXT, MessageTypeConstant.RECEIVED));
        message.setSender(Message.SYSTEM_SENDER);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    private static Message fromJsonObject(JsonObject jsonObject, String sessionUser) {
        String messageId = jsonObject.get("MessageId").getAsString();
        String sender = jsonObject.get("Sender").getAsString();
        String receiver = jsonObject.get("Receiver").getAsString();
        String payload = jsonObject.get("Payload").getAsString();
        boolean isImage = jsonObject.get("IsImage").getAsBoolean();
        boolean isRead = jsonObject.get("IsRead").getAsBoolean();
        int rawType = jsonObject.get("Type").getAsInt();
        boolean isSelf = sender.equals(sessionUser);
        boolean isPrivate = rawType == 0;
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

    public static List<Message> parseHistoryMessages(String serializedString, String username) {
        List<Message> historyMessages = new ArrayList<>();
        JsonArray jsonArray = gson.fromJson(serializedString, JsonArray.class);
        for (JsonElement jsonElement : jsonArray) {
            Message chatMessage = fromJsonObject(jsonElement.getAsJsonObject(), username);
            historyMessages.add(chatMessage);
        }
        return historyMessages;
    }

    public static String serializeHistoryMessages(List<Message> messages) {
        JsonArray jsonArray = new JsonArray();
        for (Message message : messages) {
            JsonObject jsonObject = toJsonObject(message);
            jsonArray.add(jsonObject);
        }

        return jsonArray.toString();
    }

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
            jsonObject.add("Type", gson.toJsonTree(0));
        } else {
            jsonObject.add("Type", gson.toJsonTree(1));
        }
        return jsonObject;
    }

    public static String encodeToBase64(Bitmap bmp) {
        // Encoding image into base64 string
        String messageImageContent = ""; // Empty by default
        try (ByteArrayOutputStream stream = new ByteArrayOutputStream()) {
            bmp.compress(Bitmap.CompressFormat.JPEG, 25, stream);
            byte[] byteArray = stream.toByteArray();
            messageImageContent = Base64.getEncoder().encodeToString(byteArray);
        } catch (IOException ioe) {
            Log.e("createImageMessage", ioe.getLocalizedMessage());
        }
        return messageImageContent;
    }

    public static Bitmap decodeToBitmap(String payload) {
        byte[] byteArray = Base64.getDecoder().decode(payload);
        return BitmapFactory.decodeByteArray(byteArray, 0, byteArray.length);
    }
}
