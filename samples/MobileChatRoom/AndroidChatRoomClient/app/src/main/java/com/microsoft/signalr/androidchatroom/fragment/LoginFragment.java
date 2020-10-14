package com.microsoft.signalr.androidchatroom.fragment;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.navigation.fragment.NavHostFragment;

import com.microsoft.signalr.androidchatroom.R;

public class LoginFragment extends Fragment {
    private Button loginButton;
    private TextView usernameTextView;

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        // Inflate the layout for this fragment
        View view = inflater.inflate(R.layout.fragment_login, container, false);

        // Get element references
        this.usernameTextView = view.findViewById(R.id.text_username_LoginFragment);
        this.loginButton = view.findViewById(R.id.button_login_LoginFragment);

        return view;
    }

    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        if (loginButton != null && usernameTextView != null) {
            loginButton.setOnClickListener(
                (v) -> {
                    if (usernameTextView.getText().toString().length() > 0) {
                        Bundle bundle = new Bundle();
                        bundle.putString("username", usernameTextView.getText().toString());
                        NavHostFragment.findNavController(LoginFragment.this)
                                .navigate(R.id.action_LoginFragment_to_ChatFragment, bundle);
                    }
                }
            );
        }
    }

}