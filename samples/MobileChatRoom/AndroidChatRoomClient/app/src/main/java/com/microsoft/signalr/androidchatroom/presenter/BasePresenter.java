package com.microsoft.signalr.androidchatroom.presenter;

import com.microsoft.signalr.androidchatroom.model.BaseModel;
import com.microsoft.signalr.androidchatroom.view.BaseFragment;

/**
 * Base presenter component for Model-View-Presenter design
 * @param <F> Fragment (View)
 * @param <M> Model
 */
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

    public void detach() {
        if (mBaseFragment != null) {
            mBaseFragment.detach();
            mBaseFragment = null;
        }

        mBaseModel = null;
    }

    public abstract void createModel();
}
