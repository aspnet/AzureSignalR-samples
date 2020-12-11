package com.signalr.androidchatroom.contract;

import android.app.Activity;

import com.signalr.androidchatroom.util.SimpleCallback;

/**
 * Contract for login and start connection functions
 * Defined in MVP (Model-View-Presenter) Pattern
 */
public interface LoginContract {

    interface Presenter {
        /* Called by view */

        /**
         * Try to sign into the chat server.
         *
         * @param refreshUiCallback Defines the corresponding behavior when success/error happens
         */
        void signIn(SimpleCallback<Void> refreshUiCallback);
    }

    interface View {
        /* Called by model */

        /**
         * Sets a login status and navigate to chat fragment when successful.
         *
         * @param username Username confirmed by chat server.
         */
        void setLogin(String username);
    }

    interface Model {
        /* Called by presenter*/

        /**
         * Create a ISingleAccountPublicClientApplication for AAD sign-in.
         *
         * @param callback Action to take when success/error
         */
        void createClientApplication(SimpleCallback<Void> callback);

        /**
         * Sign into AAD.
         *
         * @param usernameCallback If success pass back the username, otherwise handle error
         */
        void signIn(SimpleCallback<String> usernameCallback);

        /**
         * Acquire idToken again in case the sign-in response returned a broken token.
         *
         * @param idTokenCallback If success pass back the idToken, otherwise handle error
         */
        void acquireIdToken(SimpleCallback<String> idTokenCallback);

        /**
         * Call SignalR layer methods to enter the chat room with given credentials.
         *
         * @param idToken AAD id token
         * @param username Username
         * @param callback Action to take when success/error
         */
        void enterChatRoom(String idToken, String username, SimpleCallback<Void> callback);

        /**
         * Fetch device uuid after connection to notification hub is established.
         */
        void refreshDeviceUuid();

    }
}
