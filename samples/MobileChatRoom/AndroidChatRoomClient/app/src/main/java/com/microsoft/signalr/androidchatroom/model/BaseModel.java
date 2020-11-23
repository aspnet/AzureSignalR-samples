package com.microsoft.signalr.androidchatroom.model;

import com.microsoft.signalr.HubConnection;

public abstract class BaseModel {

    protected HubConnection mHubConnection;

    public void detach() {
        mHubConnection = null;
    }

}
