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

    private final LoginPresenter mLoginPresenter;

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
                        // Callback on presenter
                        callback.onSuccess(s);

                        // Start timer in service
                        SignalRService.startReconnectTimer();
                    }

                    @Override
                    public void onError(@NonNull Throwable e) {
                        callback.onError(e.getMessage());
                    }
                });
    }
}
