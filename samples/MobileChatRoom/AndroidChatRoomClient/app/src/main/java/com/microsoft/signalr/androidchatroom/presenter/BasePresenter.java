package com.microsoft.signalr.androidchatroom.presenter;

import com.microsoft.signalr.androidchatroom.model.BaseModel;
import com.microsoft.signalr.androidchatroom.view.BaseFragment;

public abstract class BasePresenter<F extends BaseFragment, M extends BaseModel> {
    protected F mBaseFragment;
    protected M mBaseModel;

    public BasePresenter() {
        createModel();
    }

    public BasePresenter(F baseFragment) {
        attachFragment(baseFragment);
        createModel();
    }

    public void attachFragment(F baseFragment) {
        mBaseFragment = baseFragment;
    }

    public void detachFragment() {
        mBaseFragment = null;
        if (mBaseModel != null) {
            mBaseModel.detach();
        }
    }

    public abstract void createModel();
}
