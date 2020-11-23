package com.microsoft.signalr.androidchatroom.contract;

import android.graphics.Bitmap;

import com.microsoft.signalr.androidchatroom.model.entity.Message;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;

import java.util.List;

// Contract for chatting functions
// Defined in MVP (Model-View-Presenter) Pattern
public interface ChatContract {
    interface Presenter {
        // Called by View
        void sendTextMessage(String sender, String receiver, String payload);

        void sendImageMessage(String sender, String receiver, Bitmap image);

        void sendMessageRead(String messageId);

        void resendMessage(String messageId);

        void pullHistoryMessages(int callbackDirection);

        void pullImageContent(String messageId);

        void saveHistoryMessages();

        void requestLogout(boolean isForced);

        // Called by Model
        void receiveMessageAck(String messageId, long receivedTimeInLong);

        void receiveMessageRead(String messageId);

        void receiveImageContent(String messageId, Bitmap bmp);

        void addMessage(Message message);

        void addMessage(Message message, String ackId);

        void addAllMessages(List<Message> messages);

        void confirmLogout(boolean isForced);

    }

    interface View {
        void activateListeners();

        void deactivateListeners();

        void setMessages(List<Message> messages, int direction);

        void setLogout(boolean isForced);
    }

    interface Model {
        void sendBroadcastMessage(Message broadcastMessage);

        void sendPrivateMessage(Message privateMessage);

        void sendMessageRead(String messageId);

        void sendAck(String ackId);

        void pullHistoryMessages(long untilTimeInLong);

        void pullImageContent(String messageId);

        void logout();
    }
}
