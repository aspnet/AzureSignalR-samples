package com.microsoft.signalr.androidchatroom.message;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.util.Log;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Base64;
import java.util.List;
import java.util.Locale;
import java.util.function.UnaryOperator;

public class MessageFactory {
    private final static SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'", Locale.US);
    private final static long utcOffset = 1000 * 3600 * 8;
    private final static Gson gson = new Gson();

    public static Message createReceivedTextBroadcastMessage(String messageId, String sender, String payload, long time) {
        Message message = new Message(messageId, MessageTypeEnum.RECEIVED_TEXT_BROADCAST_MESSAGE);
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createReceivedImageBroadcastMessage(String messageId, String sender, String payload, long time) {
        Message message = new Message(messageId, MessageTypeEnum.RECEIVED_IMAGE_BROADCAST_MESSAGE);
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createSendingTextBroadcastMessage(String sender, String payload, long time) {
        Message message = new Message(MessageTypeEnum.SENDING_TEXT_BROADCAST_MESSAGE);
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createSendingImageBroadcastMessage(String sender, Bitmap bmp, long time, UnaryOperator<Message> callback) {
        Message message = new Message(MessageTypeEnum.SENDING_IMAGE_BROADCAST_MESSAGE);
        message.setSender(sender);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setBmp(bmp);
        new Thread(() -> {
            String payload = encodeToBase64(bmp);
            message.setPayload(payload);
            callback.apply(message);
        }).start();
        message.setTime(time);
        return message;
    }

    public static Message createReceivedTextPrivateMessage(String messageId, String sender, String receiver, String payload, long time) {
        Message message = new Message(messageId, MessageTypeEnum.RECEIVED_TEXT_PRIVATE_MESSAGE);
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createReceivedImagePrivateMessage(String messageId, String sender, String receiver, String payload, long time) {
        Message message = new Message(messageId, MessageTypeEnum.RECEIVED_IMAGE_PRIVATE_MESSAGE);
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createSendingTextPrivateMessage(String sender, String receiver, String payload, long time) {
        Message message = new Message(MessageTypeEnum.SENDING_TEXT_PRIVATE_MESSAGE);
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message createSendingImagePrivateMessage(String sender, String receiver, Bitmap bmp, long time, UnaryOperator<Message> callback) {
        Message message = new Message(MessageTypeEnum.SENDING_IMAGE_PRIVATE_MESSAGE);
        message.setSender(sender);
        message.setReceiver(receiver);
        message.setBmp(bmp);
        new Thread(() -> {
            String payload = encodeToBase64(bmp);
            message.setPayload(payload);
            callback.apply(message);
        }).start();
        message.setTime(time);
        return message;
    }

    public static Message createReceivedSystemMessage(String messageId, String payload, long time) {
        Message message = new Message(messageId, MessageTypeEnum.SYSTEM_MESSAGE);
        message.setSender(Message.SYSTEM_SENDER);
        message.setReceiver(Message.BROADCAST_RECEIVER);
        message.setPayload(payload);
        message.setTime(time);
        return message;
    }

    public static Message fromJsonObject(JsonObject jsonObject, String sessionUser) {
        String messageId = jsonObject.get("MessageId").getAsString();
        String sender = jsonObject.get("Sender").getAsString();
        String receiver = jsonObject.get("Receiver").getAsString();
        String payload = jsonObject.get("Payload").getAsString();
        boolean isImage = jsonObject.get("IsImage").getAsBoolean();
        long time;
        try {
            time = sdf.parse(jsonObject.get("SendTime").getAsString()).getTime() + utcOffset;
        } catch (Exception e) {
            time = 0;
        }
        MessageTypeEnum type;
        int rawType = jsonObject.get("Type").getAsInt();
        if (isImage) {
            if (rawType == 0) {
                if (sender.equals(sessionUser)) {
                    type = MessageTypeEnum.SENT_IMAGE_PRIVATE_MESSAGE;
                } else {
                    type = MessageTypeEnum.RECEIVED_IMAGE_PRIVATE_MESSAGE;
                }
            } else {
                if (sender.equals(sessionUser)) {
                    type = MessageTypeEnum.SENT_IMAGE_BROADCAST_MESSAGE;
                } else {
                    type = MessageTypeEnum.RECEIVED_IMAGE_BROADCAST_MESSAGE;
                }
            }
        } else {
            if (rawType == 0) {
                if (sender.equals(sessionUser)) {
                    type = MessageTypeEnum.SENT_TEXT_PRIVATE_MESSAGE;
                } else {
                    type = MessageTypeEnum.RECEIVED_TEXT_PRIVATE_MESSAGE;
                }
            } else {
                if (sender.equals(sessionUser)) {
                    type = MessageTypeEnum.SENT_TEXT_BROADCAST_MESSAGE;
                } else {
                    type = MessageTypeEnum.RECEIVED_TEXT_BROADCAST_MESSAGE;
                }
            }
        }
        Message message = new Message(messageId, type);
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
        Bitmap bmp = BitmapFactory.decodeByteArray(byteArray, 0, byteArray.length);
        return bmp;
    }
}
