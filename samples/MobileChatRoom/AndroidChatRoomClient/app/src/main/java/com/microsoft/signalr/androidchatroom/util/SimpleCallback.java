package com.microsoft.signalr.androidchatroom.util;

public interface SimpleCallback<T> {
    default void onSuccess(T t) {

    }

    default void onError(String errorMessage) {

    }
}
