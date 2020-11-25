package com.microsoft.signalr.androidchatroom.model;

import com.microsoft.signalr.androidchatroom.contract.LoginContract;
import com.microsoft.signalr.androidchatroom.model.param.LoginParam;
import com.microsoft.signalr.androidchatroom.presenter.LoginPresenter;
import com.microsoft.signalr.androidchatroom.service.SignalRService;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;

import io.reactivex.SingleObserver;
import io.reactivex.annotations.NonNull;
import io.reactivex.disposables.Disposable;
import io.reactivex.schedulers.Schedulers;

public class LoginModel extends BaseModel implements LoginContract.Model {

    private LoginPresenter mLoginPresenter;

    public LoginModel(LoginPresenter loginPresenter) {
        mLoginPresenter = loginPresenter;
    }


    @Override
    public void login(LoginParam loginParam, SimpleCallback<String> callback) {
        SignalRService.login(loginParam)
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
                        callback.onSuccess(s);

                        // Start timer in service
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
    public void detach() {
        mLoginPresenter.detach();
        mLoginPresenter = null;
    }
}
