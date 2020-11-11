package com.microsoft.signalr.androidchatroom.fragment;

import com.microsoft.signalr.androidchatroom.message.Message;

import java.util.List;
import java.util.Set;

public interface ChatUserInterface {

    void activateClickEvent();

    void disableClickEvent();

    void tryAddMessage(Message message, int direction);

    void tryAddAllMessages(List<Message> messages, int direction);

    void setSentMessageAck(String messageId, long receivedTimeInLong);

    void setSentMessageRead(String messageId);

    void setImageContent(String messageId, String payload);

    void showSessionExpiredDialog();

    void refreshUiThread(boolean sortMessageList, int direction);
}
