package com.microsoft.signalr.androidchatroom.fragment;

import android.app.AlertDialog;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.navigation.Navigation;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.activity.MainActivity;
import com.microsoft.signalr.androidchatroom.fragment.buttonhandler.ImageButtonHandler;
import com.microsoft.signalr.androidchatroom.fragment.buttonhandler.SendButtonHandler;
import com.microsoft.signalr.androidchatroom.fragment.chatrecyclerview.ChatContentAdapter;
import com.microsoft.signalr.androidchatroom.message.MessageFactory;
import com.microsoft.signalr.androidchatroom.message.Message;
import com.microsoft.signalr.androidchatroom.service.ChatService;
import com.microsoft.signalr.androidchatroom.service.NotificationService;

import org.jetbrains.annotations.NotNull;

import java.io.FileNotFoundException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;

import static android.app.Activity.RESULT_OK;

public class ChatFragment extends Fragment implements ChatUserInterface {
    private static final String TAG = "ChatFragment";
    public static final int RESULT_LOAD_IMAGE = 1;

    // Services
    private ChatService chatService;
    private NotificationService notificationService;

    // Messages
    private final List<Message> messages = new ArrayList<>();
    private String username;
    private String deviceUuid;

    // Click Listeners
    private SendButtonHandler sendButtonHandler;
    private ImageButtonHandler imageButtonHandler;

    // View elements and adapters
    private EditText chatBoxReceiverEditText;
    private EditText chatBoxMessageEditText;
    private Button chatBoxSendButton;
    private Button chatBoxImageButton;
    private RecyclerView chatContentRecyclerView;
    private ChatContentAdapter chatContentAdapter;
    private LinearLayoutManager layoutManager;

    public EditText getChatBoxReceiverEditText() {
        return chatBoxReceiverEditText;
    }

    public EditText getChatBoxMessageEditText() {
        return chatBoxMessageEditText;
    }

    public String getUsername() {
        return username;
    }

    @Override
    public void onAttach(@NotNull Context context) {
        super.onAttach(context);
        try {
            chatService = ((MainActivity) context).getChatService();
            notificationService = ((MainActivity) context).getNotificationService();
        } catch (ClassCastException e) {
            Log.e(TAG, e.getMessage());
        }
    }

    @Override
    public void onDetach() {
        super.onDetach();

        // Remove activity reference
        chatService = null;
        notificationService = null;
    }

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_chat, container, false);

        // Get view element references
        this.chatBoxReceiverEditText = view.findViewById(R.id.edit_chat_receiver);
        this.chatBoxMessageEditText = view.findViewById(R.id.edit_chat_message);
        this.chatBoxSendButton = view.findViewById(R.id.button_chatbox_send);
        this.chatBoxImageButton = view.findViewById(R.id.button_chatbox_image);
        this.chatContentRecyclerView = view.findViewById(R.id.recyclerview_chatcontent);

        // Create objects
        this.chatContentAdapter = new ChatContentAdapter(messages, getContext(), this, chatService);
        this.layoutManager = new LinearLayoutManager(this.getActivity());

        // Configure RecyclerView
        configureRecyclerView();

        this.sendButtonHandler = new SendButtonHandler(this, chatService);
        this.imageButtonHandler = new ImageButtonHandler(this, chatService);

        return view;
    }

    private void configureRecyclerView() {
        // Add append new messages to end (bottom)
        layoutManager.setStackFromEnd(true);

        chatContentRecyclerView.setLayoutManager(layoutManager);
        chatContentRecyclerView.setAdapter(chatContentAdapter);
    }

    @Override
    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        // Get passed username
        if ((username = getArguments().getString("username")) == null) {
            username = "EMPTY_PLACEHOLDER";
        }

        // Get deviceUuid
        deviceUuid = notificationService.getDeviceUuid();

        // Register user info into chat service
        new Thread(() -> {
            chatService.register(username, deviceUuid, this);
            chatService.startSession();
        }).start();
    }

    @Override
    public void activateClickEvent() {
        chatBoxSendButton.setOnClickListener(sendButtonHandler);
        chatBoxImageButton.setOnClickListener(imageButtonHandler);
        chatContentRecyclerView.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                super.onScrolled(recyclerView, dx, dy);
                if (!recyclerView.canScrollVertically(-1)) {
                    Log.d(TAG, "OnScroll cannot scroll vertical -1");
                    long untilTime = System.currentTimeMillis();
                    for (Message message : messages) {
                        if (!message.getMessageType().name().contains("SYSTEM")) {
                            untilTime = message.getTime();
                            break;
                        }
                    }
                    chatService.pullHistoryMessages(untilTime);
                }
            }
        });
    }

    @Override
    public void disableClickEvent() {
        chatBoxSendButton.setOnClickListener(null);
        chatBoxImageButton.setOnClickListener(null);
        chatContentRecyclerView.addOnScrollListener(null);
    }

    @Override
    public void tryAddMessage(Message message, int direction) {
        // Check for duplicated message
        boolean isDuplicateMessage = checkForDuplicatedMessage(message.getMessageId());

        // If not duplicated, create ChatMessage according to parameters
        if (!isDuplicateMessage) {
            messages.add(message);

            // Tell the server the message was read
            setReceivedMessageRead(message);
        }

        refreshUiThread(true, direction);
    }

    @Override
    public void tryAddAllMessages(List<Message> messages, int direction) {
        // Record all messages for now
        Set<String> existedMessageIds = this.messages.stream().map(Message::getMessageId).collect(Collectors.toSet());

        // Iterate through message list
        for (Message message : messages) {
            if (!existedMessageIds.contains(message.getMessageId())) {
                // If found a new message, add it to message list
                this.messages.add(message);
                existedMessageIds.add(message.getMessageId());

                // Tell the server the message was read
                setReceivedMessageRead(message);
            }
        }

        refreshUiThread(true, direction);
    }

    @Override
    public void setSentMessageAck(String messageId, long receivedTimeInLong) {
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                message.ack(receivedTimeInLong);
                Log.d("setMessageAck", messageId);
                break;
            }
        }

        refreshUiThread(true, 0);
    }

    @Override
    public void setSentMessageRead(String messageId) {
        for (Message message : messages) {
            if (message.getMessageId().equals(messageId)) {
                message.read();
                break;
            }
        }

        refreshUiThread(true, 0);
    }

    private void setReceivedMessageRead(Message message) {
        if (isUnreadReceivedPrivateMessage(message)) {
            chatService.sendMessageRead(message.getMessageId());
        }
    }

    private boolean isUnreadReceivedPrivateMessage(Message message) {
        return !message.isRead() &&
                message.getMessageType().name().contains("RECEIVED")
                && message.getMessageType().name().contains("PRIVATE");
    }

    @Override
    public void showSessionExpiredDialog() {
        requireActivity().runOnUiThread(() -> {
            AlertDialog.Builder builder = new AlertDialog.Builder(getContext());
            builder.setMessage(R.string.alert_message)
                    .setTitle(R.string.alert_title)
                    .setCancelable(false);
            builder.setPositiveButton(R.string.alert_ok, (dialog, id) -> {
                Navigation.findNavController(requireView()).navigate(R.id.action_ChatFragment_to_LoginFragment);
                requireActivity().recreate();
            });
            AlertDialog dialog = builder.create();
            dialog.show();
        });
    }

    @Override
    public void onActivityResult(int reqCode, int resultCode, Intent data) {
        super.onActivityResult(reqCode, resultCode, data);
        if (resultCode == RESULT_OK) {
            try {
                final Uri imageUri = data.getData();
                final InputStream imageStream = requireActivity().getContentResolver().openInputStream(imageUri);
                final Bitmap selectedImage = BitmapFactory.decodeStream(imageStream);
                Message imageMessage = imageButtonHandler.createAndSendImageMessage(selectedImage);
                messages.add(imageMessage);
                refreshUiThread(false,1);
            } catch (FileNotFoundException e) {
                e.printStackTrace();
                Toast.makeText(getContext(), "Image picking failed.", Toast.LENGTH_LONG).show();
            }
        } else {
            Toast.makeText(getContext(), "You haven't picked Image.", Toast.LENGTH_LONG).show();
        }
    }

    @Override
    public void setImageContent(String messageId, String payload) {
        Message message = getMessageWithId(messageId);
        if (message != null) {
            message.ackPullImage();
            message.setPayload(payload);
            message.setBmp(MessageFactory.decodeToBitmap(payload));
            refreshUiThread(false,0);
        }
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
    public void refreshUiThread(boolean sortMessageList, int direction) {
        // Sort by send time first
        if (sortMessageList) {
            messages.sort((m1, m2) -> (int) (m1.getTime() - m2.getTime()));
        }

        // Then refresh the UiThread
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            switch (direction) {
                case 1:
                    Log.d(TAG, "Finger swipe up" + (messages.size() - 1));
                    chatContentRecyclerView.scrollToPosition(messages.size() - 1);
                    break;
                case -1:
                    Log.d(TAG, "Finger swipe down");
                    chatContentRecyclerView.scrollToPosition(0);
                    break;
                default:
            }
        });
    }
}
