package com.microsoft.signalr.androidchatroom.view.chatrecyclerview;

import com.microsoft.signalr.androidchatroom.model.entity.Message;

public interface RecyclerViewItemClickListener {
    void onClickItem(Message message);
}
