package com.signalr.androidchatroom.service;

import android.app.Activity;
import android.content.Context;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.microsoft.identity.client.AuthenticationCallback;
import com.microsoft.identity.client.IAccount;
import com.microsoft.identity.client.IAuthenticationResult;
import com.microsoft.identity.client.IPublicClientApplication;
import com.microsoft.identity.client.ISingleAccountPublicClientApplication;
import com.microsoft.identity.client.PublicClientApplication;
import com.microsoft.identity.client.SilentAuthenticationCallback;
import com.microsoft.identity.client.exception.MsalException;
import com.signalr.androidchatroom.R;
import com.signalr.androidchatroom.util.SimpleCallback;

/**
 * Authentication layer that interacts with Azure Active Directory using MSAL
 * See https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-android#integrate-with-microsoft-authentication-library
 * for reference.
 */
public class AuthenticationService {
    private static final String TAG = "AuthenticationService";

    /* We only need the very basic scope. You can always add more. */
    private final static String[] SCOPES = {"User.Read"};

    /* You can find the value at Azure Portal -> Azure Active Directory -> App Registration
     * -> Endpoints.
     * For general purpose sign in (Microsoft/Outlook/Live),
     * use https://login.microsoftonline.com/common
     */
    private final static String AUTHORITY = "https://login.microsoftonline.com/common";
    private static ISingleAccountPublicClientApplication mSingleAccountApp;

    public static void createClientApplication(Context context, SimpleCallback<Void> callback) {
        PublicClientApplication.createSingleAccountPublicClientApplication(context,
                R.raw.auth_config_single_account, new IPublicClientApplication.ISingleAccountApplicationCreatedListener() {
                    @Override
                    public void onCreated(ISingleAccountPublicClientApplication application) {
                        Log.d(TAG, "Client Application Created.");
                        mSingleAccountApp = application;
                        callback.onSuccess(null);
                    }

                    @Override
                    public void onError(MsalException exception) {
                        Log.e(TAG, "Client Application Creation Failed.");
                        callback.onError(exception.getMessage());
                    }
                });
    }

    public static void signOut() {
        mSingleAccountApp.signOut(new ISingleAccountPublicClientApplication.SignOutCallback() {
            @Override
            public void onSignOut() {

            }

            @Override
            public void onError(@NonNull MsalException e) {

            }
        });
    }

    public static void signIn(Activity activity, SimpleCallback<String> usernameCallback) {
        mSingleAccountApp.signIn(activity, null, SCOPES, new AuthenticationCallback() {
            @Override
            public void onCancel() {
                Log.d(TAG, "MSAL signIn cancelled.");
                usernameCallback.onError("Signing in Cancelled.");
            }

            @Override
            public void onSuccess(IAuthenticationResult iAuthenticationResult) {
                /* A successful signing in can guarantee you a valid username */
                Log.d(TAG, "MSAL signIn succeeded.");
                usernameCallback.onSuccess(iAuthenticationResult.getAccount().getUsername());
            }

            @Override
            public void onError(MsalException e) {
                Log.e(TAG, "MSAL signIn error.");
                usernameCallback.onError(e.getMessage());
            }
        });
    }

    public static void loadActiveAccountOrSignIn(Activity activity, SimpleCallback<String> usernameCallback) {
        mSingleAccountApp.getCurrentAccountAsync(new ISingleAccountPublicClientApplication.CurrentAccountCallback() {
            @Override
            public void onAccountLoaded(@Nullable IAccount activeAccount) {
                /* Exists a logged in account */
                if (activeAccount == null) {
                    signIn(activity, usernameCallback);
                } else {
                    usernameCallback.onSuccess(activeAccount.getUsername());
                }

            }

            @Override
            public void onAccountChanged(@Nullable IAccount priorAccount, @Nullable IAccount currentAccount) {
                if (currentAccount == null) {
                    signIn(activity, usernameCallback);
                }
            }

            @Override
            public void onError(@NonNull MsalException exception) {
                Log.e(TAG, "Load Existing Account Error.");
                usernameCallback.onError(exception.getMessage());
            }
        });

    }

    public static void acquireIdToken(SimpleCallback<String> idTokenCallback) {
        mSingleAccountApp.acquireTokenSilentAsync(SCOPES, AUTHORITY, new SilentAuthenticationCallback() {
            @Override
            public void onSuccess(IAuthenticationResult iAuthenticationResult) {
                /* Passing the ID Token back to model layer */
                idTokenCallback.onSuccess(iAuthenticationResult.getAccount().getIdToken());
            }

            @Override
            public void onError(MsalException e) {
                idTokenCallback.onSuccess(e.getMessage());
            }
        });
    }

}
