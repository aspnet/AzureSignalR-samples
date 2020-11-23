package com.microsoft.signalr.androidchatroom.presenter;


import android.content.Context;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.util.Log;

import com.google.gson.Gson;
import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.contract.ChatContract;
import com.microsoft.signalr.androidchatroom.model.ChatModel;
import com.microsoft.signalr.androidchatroom.model.entity.Message;
import com.microsoft.signalr.androidchatroom.model.entity.MessageFactory;
import com.microsoft.signalr.androidchatroom.model.entity.MessageTypeConstant;
import com.microsoft.signalr.androidchatroom.util.MessageTypeUtils;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;
import com.microsoft.signalr.androidchatroom.view.ChatFragment;

import org.jetbrains.annotations.NotNull;

import java.util.ArrayList;
import java.util.List;
import java.util.Set;
import java.util.Timer;
import java.util.TimerTask;
import java.util.stream.Collectors;

public class ChatPresenter extends BasePresenter<ChatFragment, ChatModel> implements ChatContract.Presenter {
    private static final String TAG = "ChatPresenter";

    private final List<Message> messages = new ArrayList<>();
    private final List<Message> sendingMessages = new ArrayList<>();
    private final String username;
    private final String deviceUuid;
    private int callbackDirection = 1;

    private final Timer timeoutTimer;

    public ChatPresenter(ChatFragment chatFragment, String username, String deviceUuid) {
        super(chatFragment);

        this.username = username;
        this.deviceUuid = deviceUuid;

        mBaseFragment.activateListeners();

        mBaseFragment.configureRecyclerView(messages, this);

        timeoutTimer = new Timer();
        timeoutTimer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                timeout();
            }
        }, 0, 10000);

        restoreOrPullHistoryMessages();
    }

    private void timeout() {
        boolean needUpdate = false;
        for (Message message : messages) {
            if ((message.getMessageType() & MessageTypeConstant.MESSAGE_STATUS_MASK) == MessageTypeConstant.SENDING) {
                if (System.currentTimeMillis() - message.getTime() > 10000) {
                    message.timeout();
                    needUpdate = true;
                }
            }

        }

        if (needUpdate) {
            // Set view messages
            mBaseFragment.setMessages(messages, 0);
        }
    }

    private void restoreOrPullHistoryMessages() {
        Context context = mBaseFragment.getContext();
        SharedPreferences sharedPreferences = context.getSharedPreferences(context.getString(R.string.saved_messages_key), Context.MODE_PRIVATE);
        String storedJsonMessages = sharedPreferences.getString(context.getString(R.string.saved_messages_key), null);
        if (storedJsonMessages == null || "[]".equals(storedJsonMessages)) {
            // Pull History Messages
            Log.d(TAG, "First Login: Pulling History messages.");
            mBaseModel.pullHistoryMessages(calculateUntilTime());
        } else {
            // Restore History Messages
            Log.d(TAG, "First Login: Restoring history messages.");
            List<Message> historyMessages = MessageFactory.parseHistoryMessages(storedJsonMessages, username);
            mBaseFragment.setMessages(historyMessages, 1);
        }
    }

    @Override
    public void saveHistoryMessages() {
        Context context = mBaseFragment.getContext();
        SharedPreferences sharedPreferences = context.getSharedPreferences(context.getString(R.string.saved_messages_key), Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        editor.putString(context.getString(R.string.saved_messages_key), MessageFactory.serializeHistoryMessages(messages)).apply();
    }

    @Override
    public void sendTextMessage(String sender, String receiver, String payload) {
        Message message;
        if (receiver.length() == 0) {
            message = MessageFactory.createSendingTextBroadcastMessage(sender, payload, System.currentTimeMillis());
        } else {
            message = MessageFactory.createSendingTextPrivateMessage(sender, receiver, payload, System.currentTimeMillis());
        }

        messages.add(message);
        if ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK) == MessageTypeConstant.PRIVATE) {
            mBaseModel.sendPrivateMessage(message);
        } else if ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK) == MessageTypeConstant.BROADCAST) {
            mBaseModel.sendBroadcastMessage(message);
        }

        mBaseFragment.setMessages(messages, 1);

        startAckWaiting(message);
    }

    @Override
    public void sendImageMessage(String sender, String receiver, Bitmap image) {
        SimpleCallback<Message> callback = new SimpleCallback<Message>() {
            @Override
            public void onSuccess(Message message) {
                messages.add(message);
                if ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK) == MessageTypeConstant.PRIVATE) {
                    mBaseModel.sendPrivateMessage(message);
                } else if ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK) == MessageTypeConstant.BROADCAST) {
                    mBaseModel.sendBroadcastMessage(message);
                }
                startAckWaiting(message);
            }

            @Override
            public void onError(String errorMessage) {

            }
        };

        if (receiver.length() == 0) {
            MessageFactory.createSendingImageBroadcastMessage(sender, image, System.currentTimeMillis(), callback);
        } else {
            MessageFactory.createSendingImagePrivateMessage(sender, receiver, image, System.currentTimeMillis(), callback);
        }

    }

    @Override
    public void resendMessage(String messageId) {
        Message message = getMessageWithId(messageId);
        if (message == null) {
            return;
        }

        message.setTime(System.currentTimeMillis());
        message.setMessageType(MessageTypeUtils.setStatus(message.getMessageType(), MessageTypeConstant.SENDING));

        if ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK) == MessageTypeConstant.BROADCAST) {
            mBaseModel.sendBroadcastMessage(message);
        } else if ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK) == MessageTypeConstant.PRIVATE) {
            mBaseModel.sendPrivateMessage(message);
        }

        startAckWaiting(message);
    }

    private void startAckWaiting(Message message) {
        sendingMessages.add(message);
    }

    @Override
    public void sendMessageRead(String messageId) {
        mBaseModel.sendMessageRead(messageId);
    }


    @Override
    public void receiveMessageAck(String messageId, long receivedTimeInLong) {
        Message message = getMessageWithId(messageId);
        sendingMessages.remove(message);

        message.ack(receivedTimeInLong);
        mBaseFragment.setMessages(messages, 0);
    }

    @Override
    public void receiveMessageRead(String messageId) {
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                message.read();
                break;
            }
        }
        // Set view messages
        mBaseFragment.setMessages(messages, 0);
    }

    @Override
    public void receiveImageContent(String messageId, Bitmap bmp) {
        Message message = getMessageWithId(messageId);
        if (message != null) {
            message.setPayload("");
            message.setBmp(bmp);
            // Set view messages
            mBaseFragment.setMessages(messages, 0);
        }
    }

    @Override
    public void pullHistoryMessages(int callbackDirection) {
        this.callbackDirection = callbackDirection;
        mBaseModel.pullHistoryMessages(calculateUntilTime());
    }

    private long calculateUntilTime() {
        long untilTime = System.currentTimeMillis();
        for (Message message : messages) {
            if ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK) != MessageTypeConstant.SYSTEM) {
                untilTime = message.getTime();
                break;
            }
        }
        return untilTime;
    }

    @Override
    public void pullImageContent(String messageId) {
        mBaseModel.pullImageContent(messageId);
    }

    @Override
    public void requestLogout(boolean isForced) {
        mBaseModel.logout();
    }

    @Override
    public void confirmLogout(boolean isForced) {
        mBaseFragment.setLogout(isForced);
    }


    @Override
    public void addMessage(@NotNull Message message) {
        // Check for duplicated message
        boolean isDuplicateMessage = checkForDuplicatedMessage(message.getMessageId());

        // If not duplicated, create ChatMessage according to parameters
        if (!isDuplicateMessage) {
            messages.add(message);

            // Tell the server the message was read
            sendMessageRead(message);
        }

        // Sort messages by send time
        messages.sort((m1, m2) -> (int) (m1.getTime() - m2.getTime()));

        // Set view messages
        mBaseFragment.setMessages(messages, 1);
    }

    @Override
    public void addMessage(Message message, String ackId) {
        // Send back ack
        mBaseModel.sendAck(ackId);

        addMessage(message);
    }

    @Override
    public void addAllMessages(List<Message> receivedMessages) {
        // Record all messages for now
        Set<String> existedMessageIds = this.messages.stream().map(Message::getMessageId).collect(Collectors.toSet());

        // Iterate through message list
        for (Message message : receivedMessages) {
            if (!existedMessageIds.contains(message.getMessageId())) {
                // If found a new message, add it to message list
                this.messages.add(message);
                existedMessageIds.add(message.getMessageId());

                // Tell the server the message was read
                sendMessageRead(message);
            }
        }

        // Sort messages by send time
        messages.sort((m1, m2) -> (int) (m1.getTime() - m2.getTime()));

        // Set view messages
        mBaseFragment.setMessages(messages, callbackDirection);
    }

    private void sendMessageRead(Message message) {
        if (isUnreadReceivedPrivateMessage(message)) {
            mBaseModel.sendMessageRead(message.getMessageId());
        }
    }

    private boolean isUnreadReceivedPrivateMessage(Message message) {
        return !message.isRead() &&
                ((message.getMessageType() & MessageTypeConstant.MESSAGE_STATUS_MASK) == MessageTypeConstant.RECEIVED)
                && ((message.getMessageType() & MessageTypeConstant.MESSAGE_TYPE_MASK) == MessageTypeConstant.PRIVATE);
    }


    private boolean checkForDuplicatedMessage(String messageId) {
        boolean isDuplicateMessage = false;
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                isDuplicateMessage = true;
                break;
            }
        }
        return isDuplicateMessage;
    }

    private Message getMessageWithId(String messageId) {
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                return message;
            }
        }

        return null;
    }

    @Override
    public void createModel() {
        mBaseModel = new ChatModel(this);
    }

}
