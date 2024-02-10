package com.signalr.androidchatroom.view;

import android.content.Context;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.navigation.fragment.NavHostFragment;

import com.signalr.androidchatroom.R;
import com.signalr.androidchatroom.activity.MainActivity;
import com.signalr.androidchatroom.contract.LoginContract;
import com.signalr.androidchatroom.presenter.LoginPresenter;
import com.signalr.androidchatroom.util.SimpleCallback;

public class LoginFragment extends BaseFragment implements LoginContract.View {
    private static final String TAG = "LoginFragment";

    private LoginPresenter mLoginPresenter;

    private Button mLoginButton;

    @Override
    public void onAttach(@NonNull Context context) {
        super.onAttach(context);
        ((MainActivity) context).setLoginFragment(this);
    }

    @Override
    public void onDetach() {
        super.onDetach();
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        mLoginPresenter = new LoginPresenter(this, (MainActivity) requireActivity());
    }

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        /* Inflate the layout for this fragment */
        View view = inflater.inflate(R.layout.fragment_login, container, false);

        /* Get element references */
        mLoginButton = view.findViewById(R.id.button_login_LoginFragment);

        return view;
    }

    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        mLoginButton.setOnClickListener(v -> {
            mLoginButton.setClickable(false);
            mLoginButton.setText(R.string.connecting);
            mLoginPresenter.signIn(new SimpleCallback<Void>() {
                @Override
                public void onError(String errorMessage) {
                    Log.e(TAG, errorMessage);
                    requireActivity().runOnUiThread(() -> {
                        mLoginButton.setClickable(true);
                        mLoginButton.setText(R.string.login);
                    });
                }
            });
        });
    }

    @Override
    public void setLogin(String username) {
        Bundle bundle = new Bundle();
        bundle.putString("username", username);
        NavHostFragment.findNavController(LoginFragment.this)
                .navigate(R.id.action_LoginFragment_to_ChatFragment, bundle);
    }

    @Override
    public void detach() {
        super.detach();
        mLoginPresenter.detach();
        mLoginPresenter = null;
    }
}