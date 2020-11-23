package com.microsoft.signalr.androidchatroom.contract;

import com.microsoft.signalr.androidchatroom.model.param.LoginParam;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;

// Contract for login functions
// Defined in MVP (Model-View-Presenter) Pattern
public interface LoginContract {

    interface Presenter {
        void login(String username, String deviceUuid);
    }

    interface View {
        void setLogin(String isSuccess);

        void setDeviceUuid(String deviceUuid);
    }

    interface Model {
        void login(LoginParam loginParam, SimpleCallback<String> callback);
    }
}
