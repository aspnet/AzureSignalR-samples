package com.microsoft.signalr.androidchatroom.util;

/**
 * Defines a simple callback wrapper interface.
 * @param <T>
 */
public interface SimpleCallback<T> {
    default void onSuccess(T t) {

    }

    default void onError(String errorMessage) {

    }
}
