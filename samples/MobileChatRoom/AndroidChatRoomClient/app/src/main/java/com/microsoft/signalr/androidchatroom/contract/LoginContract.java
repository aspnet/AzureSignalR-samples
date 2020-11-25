package com.microsoft.signalr.androidchatroom.contract;

import com.microsoft.signalr.androidchatroom.model.param.LoginParam;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;

/**
 * Contract for login and start connection functions
 * Defined in MVP (Model-View-Presenter) Pattern
 */
public interface LoginContract {

    interface Presenter {
        /* Called by view */

        /**
         * Sends a login request to login model.
         *
         * @param username A string representing client's username.
         * @param deviceUuid A unique string representing client device.
         *                   Usually used for notification receiver calculation.
         */
        void login(String username, String deviceUuid);
    }

    interface View {
        /* Called by model */

        /**
         * Sets a login status and navigate to chat fragment when successful.
         *
         * @param isSuccess "success" for a successful login. This will trigger a navigation from
         *                  login fragment to chat fragment.
         *                  "failure" for a failed. This will keep the active fragment
         *                  to login fragment
         */
        void setLogin(String isSuccess);

        /* Called by notification service */

        /**
         * Sets a device uuid.
         *
         * @param deviceUuid A unique string representing client device.
         *                   Usually used for notification receiver calculation.
         */
        void setDeviceUuid(String deviceUuid);
    }

    interface Model {
        /**
         * Send a login request to the SignalR layer.
         *
         * @param loginParam A wrapper for login parameters.
         * @param callback A callback specifying what to do after the login returns a result.
         */
        void login(LoginParam loginParam, SimpleCallback<String> callback);
    }
}
