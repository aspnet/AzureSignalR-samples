package com.signalr.androidchatroom.presenter;

import android.app.Activity;

import com.signalr.androidchatroom.model.BaseModel;
import com.signalr.androidchatroom.view.BaseFragment;

/**
 * Base presenter component for Model-View-Presenter design
 * @param <F> Fragment (View)
 * @param <M> Model
 */
public abstract class BasePresenter<F extends BaseFragment, M extends BaseModel> {
    protected F mBaseFragment;
    protected M mBaseModel;

    public BasePresenter(F baseFragment, Activity activity) {
        attachFragment(baseFragment);
        createModel(activity);
    }

    public void attachFragment(F baseFragment) {
        mBaseFragment = baseFragment;
    }

    public void detach() {
        if (mBaseFragment != null) {
            mBaseFragment.detach();
            mBaseFragment = null;
        }

        if (mBaseModel != null) {
            mBaseModel.detach();
            mBaseModel = null;
        }
    }

    public abstract void createModel(Activity activity);
}
