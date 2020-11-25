package com.microsoft.signalr.androidchatroom.model;

/**
 * Base model component in Model-View-Presenter design.
 */
public abstract class BaseModel {
    private static final String TAG = "BaseModel";

    public abstract void detach();

}
