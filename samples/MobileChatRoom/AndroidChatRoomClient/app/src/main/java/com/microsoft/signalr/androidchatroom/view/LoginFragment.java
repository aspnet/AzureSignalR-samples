package com.microsoft.signalr.androidchatroom.view;

import android.content.Context;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.navigation.fragment.NavHostFragment;

import com.microsoft.signalr.androidchatroom.R;
import com.microsoft.signalr.androidchatroom.activity.MainActivity;
import com.microsoft.signalr.androidchatroom.contract.LoginContract;
import com.microsoft.signalr.androidchatroom.presenter.LoginPresenter;
import com.microsoft.signalr.androidchatroom.service.NotificationService;
import com.microsoft.signalr.androidchatroom.service.SignalRService;
import com.microsoft.signalr.androidchatroom.util.SimpleCallback;

public class LoginFragment extends BaseFragment implements LoginContract.View {
    private static final String TAG = "LoginFragment";

    private LoginPresenter mLoginPresenter;

    private Button mLoginButton;
    private TextView mUsernameTextView;

    private NotificationService mNotificationService;

    private String username = null;
    private String deviceUuid = null;
    private boolean isLogging = false;

    @Override
    public void onAttach(@NonNull Context context) {
        super.onAttach(context);
        ((MainActivity) context).setLoginFragment(this);
    }

    @Override
    public void onDetach() {
        super.onDetach();
        mNotificationService = null;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        mLoginPresenter = new LoginPresenter(this);
        try {
            mNotificationService = ((MainActivity) requireActivity()).getNotificationService();
        } catch (ClassCastException e) {
            Log.e(TAG, e.getMessage());
        }
    }

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_login, container, false);

        // Get element references
        mUsernameTextView = view.findViewById(R.id.text_username_LoginFragment);
        mLoginButton = view.findViewById(R.id.button_login_LoginFragment);

        return view;
    }

    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        mLoginButton.setClickable(false);
        if (mLoginButton != null && mUsernameTextView != null) {
            mLoginButton.setOnClickListener(
                    (v) -> {
                        mLoginButton.setClickable(false);
                        mLoginButton.setText(R.string.connecting);
                        SignalRService.startHubConnection(new SimpleCallback<String>() {
                            @Override
                            public void onSuccess(String s) {
                                if (!isLogging && mUsernameTextView.getText().toString().length() > 0) {
                                    isLogging = true;
                                    username = mUsernameTextView.getText().toString();
                                    mLoginPresenter.login(username, deviceUuid);
                                }
                            }

                            @Override
                            public void onError(String errorMessage) {
                                Log.e(TAG, errorMessage);
                                requireActivity().runOnUiThread(() -> {
                                    mLoginButton.setClickable(true);
                                    mLoginButton.setText(R.string.login);
                                });
                            }
                        });
                    }
            );
            mLoginButton.setClickable(true);
        }

    }

    @Override
    public void setLogin(String isSuccess) {
        isLogging = false;
        if ("success".equals(isSuccess)) {
            Bundle bundle = new Bundle();
            bundle.putString("username", username);
            bundle.putString("deviceUuid", deviceUuid);
            NavHostFragment.findNavController(LoginFragment.this)
                    .navigate(R.id.action_LoginFragment_to_ChatFragment, bundle);
        } else {
            username = null;
            deviceUuid = null;
        }
    }

    @Override
    public void setDeviceUuid(String deviceUuid) {
        this.deviceUuid = deviceUuid;
    }
}