package com.signalr.androidchatroom.model;

import android.util.Log;

import com.signalr.androidchatroom.activity.MainActivity;
import com.signalr.androidchatroom.contract.LoginContract;
import com.signalr.androidchatroom.presenter.LoginPresenter;
import com.signalr.androidchatroom.service.AuthenticationService;
import com.signalr.androidchatroom.service.SignalRService;
import com.signalr.androidchatroom.util.SimpleCallback;


import io.reactivex.SingleObserver;
import io.reactivex.annotations.NonNull;
import io.reactivex.disposables.Disposable;
import io.reactivex.schedulers.Schedulers;

/**
 * Model component responsible for authorization and logging in.
 */
public class LoginModel extends BaseModel implements LoginContract.Model {
    private static final String TAG = "LoginModel";

    private LoginPresenter mLoginPresenter;
    private MainActivity mMainActivity;
    private String mDeviceUuid;

    public LoginModel(LoginPresenter loginPresenter, MainActivity mainActivity) {
        mLoginPresenter = loginPresenter;
        mMainActivity = mainActivity;
    }

    @Override
    public void createClientApplication(SimpleCallback<Void> createApplicationCallback) {
        AuthenticationService
                .createClientApplication(
                        mMainActivity.getApplicationContext(),
                        new SimpleCallback<Void>() {
                            @Override
                            public void onSuccess(Void aVoid) {
                                createApplicationCallback.onSuccess(null);
                            }

                            @Override
                            public void onError(String errorMessage) {
                                createApplicationCallback.onError(errorMessage);
                            }
                        });
    }

    @Override
    public void signIn(SimpleCallback<String> signInCallback) {
        AuthenticationService
                .loadActiveAccountOrSignIn(mMainActivity, new SimpleCallback<String>() {
                    @Override
                    public void onSuccess(String username) {
                        Log.d(TAG, "signIn callback get Username: "+username);
                        signInCallback.onSuccess(username);
                    }

                    @Override
                    public void onError(String errorMessage) {
                        signInCallback.onError(errorMessage);
                    }
                });
    }

    @Override
    public void acquireIdToken(SimpleCallback<String> idTokenCallback) {
        AuthenticationService
                .acquireIdToken(new SimpleCallback<String>() {
                    @Override
                    public void onSuccess(String idToken) {
                        idTokenCallback.onSuccess(idToken);
                    }

                    @Override
                    public void onError(String errorMessage) {
                        idTokenCallback.onError(errorMessage);
                    }
                });
    }

    @Override
    public void enterChatRoom(String idToken, String username, SimpleCallback<Void> callback) {
        SignalRService.startHubConnection(new SimpleCallback<Void>() {
            @Override
            public void onSuccess(Void aVoid) {
                SignalRService.login(mDeviceUuid, username)
                        .subscribeOn(Schedulers.io())
                        .observeOn(Schedulers.io())
                        .subscribe(new SingleObserver<String>() {
                            @Override
                            public void onSubscribe(@NonNull Disposable d) {

                            }

                            @Override
                            public void onSuccess(@NonNull String s) {
                                /* Once server confirms the log in request,
                                 * call onSuccess callback and then start
                                 * the reconnect timer.
                                 */
                                callback.onSuccess(null);

                                /* Start timer in service */
                                SignalRService.startReconnectTimer();
                            }

                            @Override
                            public void onError(@NonNull Throwable e) {
                                /* If server fails to confirm the log in
                                 * request, call onError callback.
                                 */
                                callback.onError(e.getMessage());
                            }
                        });
            }

            @Override
            public void onError(String errorMessage) {
                /* If fails to start hub connection (negotiation)
                 * request, call onError callback.
                 */
                callback.onError(errorMessage);
            }
        }, idToken);


    }

    @Override
    public void refreshDeviceUuid() {
        mDeviceUuid = mMainActivity.getNotificationService().getDeviceUuid();
    }

    @Override
    public void detach() {
        mLoginPresenter = null;
        mMainActivity = null;
        mDeviceUuid = null;
    }
}
