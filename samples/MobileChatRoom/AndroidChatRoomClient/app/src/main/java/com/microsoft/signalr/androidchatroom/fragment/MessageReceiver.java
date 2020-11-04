package com.microsoft.signalr.androidchatroom.fragment;

import com.microsoft.signalr.androidchatroom.message.Message;

import java.util.List;
import java.util.Set;

public interface MessageReceiver {

    void activateClickEvent();

    void tryAddMessage(Message message, int direction);

    void tryAddAllMessages(List<Message> messages, int direction);

    void setMessageAck(String messageId, long receivedTimeInLong);

    void setMessageRead(String messageId);

    void loadImageContent(String messageId, String payload);

    void showSessionExpiredDialog();

    void refreshUiThread(boolean sort, int direction);
}
