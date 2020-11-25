package com.microsoft.signalr.androidchatroom.contract;

import android.graphics.Bitmap;

import com.microsoft.signalr.androidchatroom.model.entity.Message;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;

import java.util.List;

/**
 * Contract for chatting functions
 * Defined in MVP (Model-View-Presenter) Pattern
 */
public interface ChatContract {
    interface Presenter {
        /* Called by view */

        /**
         * Sends a text message with given sender, receiver, and payload to chat model.
         *
         * @param sender A string representing sender name.
         * @param receiver A string representing receiver name.
         * @param payload A string representing message content.
         */
        void sendTextMessage(String sender, String receiver, String payload);

        /**
         * Sends a text message with given sender, receiver, and image bitmap to chat model
         *
         * @param sender A string representing sender name.
         * @param receiver A string representing receiver name.
         * @param image A bitmap representing the image to send.
         */
        void sendImageMessage(String sender, String receiver, Bitmap image);

        /**
         * Sends a read response to chat model indicating that the client has read
         * the message to chat model.
         *
         * @param messageId A string representing message id.
         */
        void sendMessageRead(String messageId);

        /**
         * Re-sends a message after a previous send operation's timing out to chat model.
         *
         * @param messageId A string representing message id.
         */
        void resendMessage(String messageId);

        /**
         * Requests to pull history messages given a callback scroll direction.
         *
         * @param callbackDirection 0   for keep current scroll position
         *                          1   for scroll to end after receiving messages
         *                          -1  for scroll to start after receiving messages
         */
        void pullHistoryMessages(int callbackDirection);

        /**
         * Requests to pull image content from chat model.
         *
         * @param messageId A string representing message id.
         */
        void pullImageContent(String messageId);

        /**
         * Saves the current collection of messages to the SharedPreference API
         */
        void saveHistoryMessages();

        /**
         * Manually requests to log out by client.
         */
        void requestLogout();

        /* Called by chat model */

        /**
         * Receives an Ack message and sets the corresponding message's status
         * to 'SENT'.
         *
         * q@param messageId A string representing message id.
         * @param receivedTimeInLong A long int representing the received time in milliseconds.
         */
        void receiveMessageAck(String messageId, long receivedTimeInLong);

        /**
         * Receives an Read message and sets the corresponding message's status
         * to 'READ'.
         *
         * @implNote Should ignore the call when the messageId of a non-private message,
         * since only private messages have a state of 'READ'.
         *
         * @param messageId A string representing message id.
         */
        void receiveMessageRead(String messageId);

        /**
         * Receives an image content and sets the message's image field
         *
         * @param messageId A string representing message id.
         * @param bmp A bitmap object containing the content of image.
         */
        void receiveImageContent(String messageId, Bitmap bmp);

        /**
         * Adds a message to the message list of presenter.
         *
         * @param message A message object to add.
         */
        void addMessage(Message message);

        /**
         * Adds a message and sends back the client ack message.
         *
         * @param message A message object to add.
         * @param ackId A unique string representing an ack message, generated and sent from server.
         */
        void addMessage(Message message, String ackId);

        /**
         * Adds a list of messages to the message list of presenter.
         *
         * @param messages A list of messages to add.
         */
        void addAllMessages(List<Message> messages);

        /**
         * Confirms a log out request and does corresponding log out works in presenter.
         *
         * @param isForced If true, display a force log out dialogue. Otherwise, simply log out.
         */
        void confirmLogout(boolean isForced);

    }

    interface View {
        /**
         * Activates all event listeners in the view. Usually called after presenter is set up.
         */
        void activateListeners();

        /**
         * Deactivate all event listeners in the view.
         */
        void deactivateListeners();

        /**
         * Set and refresh messages to display in view.
         *
         * @param messages A list of messages to display.
         * @param direction 0   for keep current scroll position
         *                  1   for scroll to end after setting messages
         *                  -1  for scroll to start after setting messages
         */
        void setMessages(List<Message> messages, int direction);

        /**
         * Set up log out works in view.
         *
         * @param isForced If true, display a force log out dialogue. Otherwise, simply log out.
         */
        void setLogout(boolean isForced);
    }

    interface Model {
        /**
         * Send a broadcast message to SignalR layer.
         *
         * @param broadcastMessage A broadcast message.
         */
        void sendBroadcastMessage(Message broadcastMessage);

        /**
         * Send a private message to SignalR layer.
         *
         * @param privateMessage A private message.
         */
        void sendPrivateMessage(Message privateMessage);

        /**
         * Send a Read response to SignalR layer.
         *
         * @param messageId A string representing the message id that the response wants to
         *                  confirm read.
         */
        void sendMessageRead(String messageId);

        /**
         * Send a Ack response to SignalR layer.
         *
         * @param ackId A string representing the ack id that the response wants to confirm ack.
         */
        void sendAck(String ackId);

        /**
         * Send a request to pull history messages before a given time to the SignalR layer.
         *
         * @param untilTimeInLong A long representing the given time.
         */
        void pullHistoryMessages(long untilTimeInLong);

        /**
         * Send a request to pull (downlaod) the content of a given image message
         * to the SignalR layer.
         *
         * @param messageId A string representing the message id of the image message.
         */
        void pullImageContent(String messageId);

        /**
         * Send a log out request to the SignalR layer.
         */
        void logout();
    }
}
