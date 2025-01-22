package com.signalr.androidchatroom.presenter;

import android.app.Activity;

import com.signalr.androidchatroom.activity.MainActivity;
import com.signalr.androidchatroom.contract.LoginContract;
import com.signalr.androidchatroom.model.LoginModel;
import com.signalr.androidchatroom.util.SimpleCallback;
import com.signalr.androidchatroom.view.LoginFragment;

/**
 * Presenter component responsible for logging in.
 */
public class LoginPresenter extends BasePresenter<LoginFragment, LoginModel> implements LoginContract.Presenter {

    public LoginPresenter(LoginFragment loginFragment, MainActivity mainActivity) {
        super(loginFragment, mainActivity);
    }

    @Override
    public void createModel(Activity activity) {
        mBaseModel = new LoginModel(this, (MainActivity) activity);
    }

    @Override
    public void signIn(SimpleCallback<Void> refreshUiCallback) {
        /* Connect to Azure Notification Hub and refresh a device Uuid */
        mBaseModel.refreshDeviceUuid();

        /* Create Client Auth App first */
        mBaseModel.createClientApplication(new SimpleCallback<Void>() {
            @Override
            public void onSuccess(Void aVoid) {
                /* If create succeeded, sign into AAD with client auth app */
                mBaseModel.signIn(new SimpleCallback<String>() {
                    @Override
                    public void onSuccess(String username) {
                        /*
                         * If sign into AAD was successful, must have a valid ID Token.
                         * Use the ID Token to send POST request to App Server.
                         * (Any POST request without a valid token will be rejected by App Server)
                         */
                        enterChatRoom(username, refreshUiCallback);
                    }

                    @Override
                    public void onError(String errorMessage) {
                        refreshUiCallback.onError(errorMessage);
                    }
                });
            }

            @Override
            public void onError(String errorMessage) {
                refreshUiCallback.onError(errorMessage);
            }
        });
    }

    private void enterChatRoom(String username, SimpleCallback<Void> refreshUiCallback) {
        /* Acquire id token first */
        mBaseModel.acquireIdToken(new SimpleCallback<String>() {
            @Override
            public void onSuccess(String idToken) {
                /* Succeeded in acquiring id token */
                mBaseModel.enterChatRoom(idToken, username, new SimpleCallback<Void>() {
                    @Override
                    public void onSuccess(Void v) {
                        mBaseFragment.setLogin(username);
                    }

                    @Override
                    public void onError(String errorMessage) {
                        refreshUiCallback.onError(errorMessage);
                    }
                });
            }

            @Override
            public void onError(String errorMessage) {
                refreshUiCallback.onError(errorMessage);
            }
        });

    }
}
