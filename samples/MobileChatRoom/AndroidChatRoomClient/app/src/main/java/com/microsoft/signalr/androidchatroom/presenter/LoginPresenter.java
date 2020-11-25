package com.microsoft.signalr.androidchatroom.presenter;

import com.microsoft.signalr.androidchatroom.contract.LoginContract;
import com.microsoft.signalr.androidchatroom.model.LoginModel;
import com.microsoft.signalr.androidchatroom.model.param.LoginParam;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;
import com.microsoft.signalr.androidchatroom.view.LoginFragment;

/**
 * Presenter component responsible for logging in.
 */
public class LoginPresenter extends BasePresenter<LoginFragment, LoginModel> implements LoginContract.Presenter {

    public LoginPresenter(LoginFragment loginFragment) {
        super(loginFragment);
    }

    @Override
    public void createModel() {
        mBaseModel = new LoginModel(this);
    }

    @Override
    public void login(String username, String deviceUuid) {
        mBaseModel.login(new LoginParam(username, deviceUuid), new SimpleCallback<String>() {
            @Override
            public void onSuccess(String isSuccess) {
                mBaseFragment.setLogin(isSuccess);
            }

            @Override
            public void onError(String errorMessage) {

            }
        });
    }
}
