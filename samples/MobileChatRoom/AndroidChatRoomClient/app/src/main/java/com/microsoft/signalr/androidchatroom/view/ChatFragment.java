package com.microsoft.signalr.androidchatroom.view;

import android.app.AlertDialog;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
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
import androidx.annotation.Nullable;
import androidx.navigation.fragment.NavHostFragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.activity.MainActivity;
import com.microsoft.signalr.androidchatroom.contract.ChatContract;
import com.microsoft.signalr.androidchatroom.model.entity.Message;
import com.microsoft.signalr.androidchatroom.presenter.ChatPresenter;
import com.microsoft.signalr.androidchatroom.view.chatrecyclerview.ChatContentAdapter;

import java.io.FileNotFoundException;
import java.io.InputStream;
import java.util.List;

import static android.app.Activity.RESULT_OK;

public class ChatFragment extends BaseFragment implements ChatContract.View {
    public static final int RESULT_LOAD_IMAGE = 1;
    private static final String TAG = "ChatFragment";
    private ChatPresenter mChatPresenter;

    private String username;
    private String deviceUuid;

    // View elements and adapters
    private EditText chatBoxReceiverEditText;
    private EditText chatBoxMessageEditText;
    private Button chatBoxSendButton;
    private Button chatBoxImageButton;
    private RecyclerView chatContentRecyclerView;
    private ChatContentAdapter chatContentAdapter;
    private SwipeRefreshLayout chatContentSwipeRefreshLayout;
    private LinearLayoutManager layoutManager;

    @Override
    public void onAttach(@NonNull Context context) {
        super.onAttach(context);
        ((MainActivity) context).setChatFragment(this);
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // Get passed username
        if ((username = getArguments().getString("username")) == null) {
            username = "EMPTY_PLACEHOLDER";
        }

        if ((deviceUuid = getArguments().getString("deviceUuid")) == null) {
            deviceUuid = "EMPTY_PLACEHOLDER";
        }
    }

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_chat, container, false);

        // Get view element references
        chatBoxReceiverEditText = view.findViewById(R.id.edit_chat_receiver);
        chatBoxMessageEditText = view.findViewById(R.id.edit_chat_message);
        chatBoxSendButton = view.findViewById(R.id.button_chatbox_send);
        chatBoxImageButton = view.findViewById(R.id.button_chatbox_image);
        chatContentRecyclerView = view.findViewById(R.id.recyclerview_chatcontent);
        chatContentSwipeRefreshLayout = view.findViewById(R.id.swipe_refresh_layout_chatcontent);

        // Create objects
        mChatPresenter = new ChatPresenter(this, username, deviceUuid);

        return view;
    }

    public void configureRecyclerView(List<Message> messages, ChatPresenter chatPresenter) {
        chatContentAdapter = new ChatContentAdapter(messages, this, chatPresenter);
        layoutManager = new LinearLayoutManager(this.getActivity());

        // Add append new messages to end (bottom)
        layoutManager.setStackFromEnd(true);

        chatContentRecyclerView.setLayoutManager(layoutManager);
        chatContentRecyclerView.setAdapter(chatContentAdapter);

        chatContentSwipeRefreshLayout.setOnRefreshListener(() -> {
            mChatPresenter.pullHistoryMessages(0);
        });
    }

    @Override
    public void activateListeners() {
        chatBoxSendButton.setOnClickListener(this::sendButtonOnClickListener);
        chatBoxImageButton.setOnClickListener(this::imageButtonOnClickListener);
    }

    @Override
    public void deactivateListeners() {
        chatBoxSendButton.setOnClickListener(null);
        chatBoxImageButton.setOnClickListener(null);
    }

    @Override
    public void setMessages(List<Message> messages, int direction) {
        updateRecyclerView(messages, direction);
    }

    @Override
    public void setLogout(boolean isForced) {
        if (isForced) {
            requireActivity().runOnUiThread(() -> {
                AlertDialog.Builder builder = new AlertDialog.Builder(getContext());
                builder.setMessage(R.string.alert_message)
                        .setTitle(R.string.alert_title)
                        .setCancelable(false);
                builder.setPositiveButton(R.string.alert_ok, (dialog, id) -> {
                    NavHostFragment.findNavController(ChatFragment.this).navigate(R.id.action_ChatFragment_to_LoginFragment);
                    requireActivity().recreate();
                });
                AlertDialog dialog = builder.create();
                dialog.show();
            });
        } else {
            NavHostFragment.findNavController(ChatFragment.this)
                    .navigate(R.id.action_ChatFragment_to_LoginFragment);
        }
    }

    @Override
    public void onActivityResult(int reqCode, int resultCode, Intent data) {
        super.onActivityResult(reqCode, resultCode, data);
        if (resultCode == RESULT_OK) {
            try {
                Uri imageUri = data.getData();
                InputStream imageStream = requireActivity().getContentResolver().openInputStream(imageUri);
                Bitmap selectedImage = BitmapFactory.decodeStream(imageStream);

                mChatPresenter.sendImageMessage(username, chatBoxReceiverEditText.getText().toString(), selectedImage);
            } catch (FileNotFoundException e) {
                e.printStackTrace();
                Toast.makeText(getContext(), R.string.image_picking_failed, Toast.LENGTH_LONG).show();
            }
        } else {
            Toast.makeText(getContext(), R.string.no_image_picked, Toast.LENGTH_LONG).show();
        }
    }

    private void sendButtonOnClickListener(View view) {
        if (chatBoxMessageEditText.getText().length() > 0) { // Empty message not allowed
            // Create and send message
            mChatPresenter.sendTextMessage(username, chatBoxReceiverEditText.getText().toString(), chatBoxMessageEditText.getText().toString());
            chatBoxMessageEditText.getText().clear();
        } else {
            Toast.makeText(getContext(), R.string.empty_message_not_allowed, Toast.LENGTH_LONG).show();
        }
    }

    private void imageButtonOnClickListener(View view) {
        Intent photoPickerIntent = new Intent(Intent.ACTION_PICK);
        photoPickerIntent.setType("image/*");
        startActivityForResult(photoPickerIntent, ChatFragment.RESULT_LOAD_IMAGE);
    }

    public void onBackPressed() {
        mChatPresenter.saveHistoryMessages();
        mChatPresenter.requestLogout(false);
    }

    public void updateRecyclerView(List<Message> messages, int direction) {
        chatContentAdapter.setMessages(messages);

        // Then refresh the UiThread
        requireActivity().runOnUiThread(() -> {
            chatContentAdapter.notifyDataSetChanged();
            switch (direction) {
                case 1:
                    chatContentRecyclerView.scrollToPosition(messages.size() - 1);
                    break;
                case -1:
                    chatContentRecyclerView.scrollToPosition(0);
                    break;
                default:
            }
        });

        chatContentSwipeRefreshLayout.setRefreshing(false);
    }
}
