package com.signalr.androidchatroom.util;

/**
 * Defines a simple callback wrapper interface.
 * @param <T> Type of parameter you want to pass into onSuccess method.
 */
public interface SimpleCallback<T> {
    default void onSuccess(T t) {

    }

    default void onError(String errorMessage) {

    }
}
