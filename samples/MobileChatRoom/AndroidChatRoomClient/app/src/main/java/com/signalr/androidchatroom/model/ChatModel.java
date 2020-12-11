package com.signalr.androidchatroom.model;

import android.graphics.Bitmap;
import android.util.Log;

import com.signalr.androidchatroom.activity.MainActivity;
import com.signalr.androidchatroom.contract.ChatContract;
import com.signalr.androidchatroom.contract.ServerContract;
import com.signalr.androidchatroom.model.entity.Message;
import com.signalr.androidchatroom.model.entity.MessageFactory;
import com.signalr.androidchatroom.presenter.ChatPresenter;
import com.signalr.androidchatroom.service.AuthenticationService;
import com.signalr.androidchatroom.service.SignalRService;

import org.apache.log4j.chainsaw.Main;

import java.util.List;

import io.reactivex.SingleObserver;
import io.reactivex.annotations.NonNull;
import io.reactivex.disposables.Disposable;
import io.reactivex.schedulers.Schedulers;

/**
 * Model component responsible for chatting.
 */
public class ChatModel extends BaseModel implements ChatContract.Model, ServerContract {
    private static final String TAG = "ChatModel";

    private ChatPresenter mChatPresenter;
    private MainActivity mMainActivity;

    public ChatModel(ChatPresenter chatPresenter, MainActivity mainActivity) {
        mChatPresenter = chatPresenter;
        mMainActivity = mainActivity;
        registerServerCallbacks();
    }

    private void registerServerCallbacks() {
        SignalRService.registerServerCallback("receiveSystemMessage", this::receiveSystemMessage,
                String.class, String.class, Long.class);
        SignalRService.registerServerCallback("receiveBroadcastMessage", this::receiveBroadcastMessage,
                String.class, String.class, String.class, String.class, Boolean.class, Long.class, String.class);
        SignalRService.registerServerCallback("receivePrivateMessage", this::receivePrivateMessage,
                String.class, String.class, String.class, String.class, Boolean.class, Long.class, String.class);

        SignalRService.registerServerCallback("receiveHistoryMessages", this::receiveHistoryMessages, String.class);
        SignalRService.registerServerCallback("receiveImageContent", this::receiveImageContent, String.class, String.class);

        SignalRService.registerServerCallback("serverAck", this::serverAck, String.class, Long.class);
        SignalRService.registerServerCallback("clientRead", this::clientRead, String.class, String.class);
        SignalRService.registerServerCallback("expireSession", this::expireSession, Boolean.class);
    }

    @Override
    public void sendPrivateMessage(Message privateMessage) {
        SignalRService.sendPrivateMessage(privateMessage.getMessageId(), privateMessage.getSender(), privateMessage.getReceiver(), privateMessage.getPayload(), privateMessage.isImage());
    }

    @Override
    public void sendBroadcastMessage(Message broadcastMessage) {
        SignalRService.sendBroadcastMessage(broadcastMessage.getMessageId(), broadcastMessage.getSender(), broadcastMessage.getPayload(), broadcastMessage.isImage());
    }

    @Override
    public void sendMessageRead(String messageId) {
        SignalRService.sendMessageRead(messageId);
    }

    @Override
    public void sendAck(String ackId) {
        SignalRService.sendAck(ackId);
    }

    @Override
    public void pullHistoryMessages(long untilTimeInLong) {
        SignalRService.pullHistoryMessages(untilTimeInLong);
    }

    @Override
    public void pullImageContent(String messageId) {
        SignalRService.pullImageContent(messageId);
    }

    @Override
    public void logout() {
        SignalRService
                .logout()
                .subscribeOn(Schedulers.io()) /* Use io-oriented thread scheduler */
                .observeOn(Schedulers.io())
                .subscribe(new SingleObserver<String>() {
                    @Override
                    public void onSubscribe(@NonNull Disposable d) {

                    }

                    @Override
                    public void onSuccess(@NonNull String s) {
                        /* Once server confirms the log out request
                         * stop the reconnect timer thread
                         * and then stop the hub connection.
                         */
                        SignalRService.stopReconnectTimer();
                        SignalRService.stopHubConnection();
                        AuthenticationService.signOut();
                    }

                    @Override
                    public void onError(@NonNull Throwable e) {
                        /* Once server fails to confirm log out request,
                         * log out anyway (Server will expire the client's
                         * session after a period of inactive time).
                         * Stop the reconnect timer thread and then stop
                         * the hub connection.
                         */
                        SignalRService.stopReconnectTimer();
                        SignalRService.stopHubConnection();
                        AuthenticationService.signOut();
                    }
                });
    }

    @Override
    public void receiveSystemMessage(String messageId, String payload, long sendTime) {
        Log.d(TAG, "receiveSystemMessage: " + payload);

        /* Create message */
        Message systemMessage = MessageFactory.createReceivedSystemMessage(messageId, payload, sendTime);

        /* Try to add message to chat presenter */
        mChatPresenter.addMessage(systemMessage);
    }

    @Override
    public void receiveBroadcastMessage(String messageId, String sender, String receiver, String payload, boolean isImage, long sendTime, String ackId) {
        Log.d(TAG, "receiveBroadcastMessage from: " + sender);

        /* Create message */
        Message chatMessage;
        if (isImage) {
            chatMessage = MessageFactory.createReceivedImageBroadcastMessage(messageId, sender, payload, sendTime);
        } else {
            chatMessage = MessageFactory.createReceivedTextBroadcastMessage(messageId, sender, payload, sendTime);
        }

        /* Try to add message to chat presenter */
        mChatPresenter.addMessage(chatMessage, ackId);
    }

    @Override
    public void receivePrivateMessage(String messageId, String sender, String receiver, String payload, boolean isImage, long sendTime, String ackId) {
        Log.d(TAG, "receivePrivateMessage from: " + sender);

        /* Create message */
        Message chatMessage;
        if (isImage) {
            chatMessage = MessageFactory.createReceivedImagePrivateMessage(messageId, sender, receiver, payload, sendTime);
        } else {
            chatMessage = MessageFactory.createReceivedTextPrivateMessage(messageId, sender, receiver, payload, sendTime);
        }

        /* Try to add message to chat presenter */
        mChatPresenter.addMessage(chatMessage, ackId);
    }

    @Override
    public void receiveImageContent(String messageId, String payload) {
        Log.d(TAG, "receiveImageContent");

        /* Decode base64 string to bitmap object */
        Bitmap bmp = MessageFactory.decodeToBitmap(payload);

        /* Send bitmap to chat presenter */
        mChatPresenter.receiveImageContent(messageId, bmp);
    }

    @Override
    public void receiveHistoryMessages(String serializedString) {
        Log.d(TAG, "receiveHistoryMessages");

        /* Parse JSON array string to list of messages */
        List<Message> historyMessages = MessageFactory.parseHistoryMessages(serializedString, SignalRService.getUsername());

        /* Add history messages to chat presenter */
        mChatPresenter.addAllMessages(historyMessages);
    }

    @Override
    public void serverAck(String messageId, long receivedTimeInLong) {
        mChatPresenter.receiveMessageAck(messageId, receivedTimeInLong);
    }

    @Override
    public void clientRead(String messageId, String username) {
        mChatPresenter.receiveMessageRead(messageId);
    }

    @Override
    public void detach() {
        mChatPresenter = null;
        mMainActivity = null;
    }

    @Override
    public void expireSession(boolean isForced) {
        mChatPresenter.confirmLogout(isForced);
        mChatPresenter.detach();
    }
}
