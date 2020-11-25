package com.microsoft.signalr.androidchatroom.model.param;

/**
 * Wrapper class for login parameters.
 */
public class LoginParam {
    private final String username;
    private final String deviceUuid;

    public LoginParam(String username, String deviceUuid) {
        this.username = username;
        this.deviceUuid = deviceUuid;
    }

    public String getUsername() {
        return username;
    }

    public String getDeviceUuid() {
        return deviceUuid;
    }
}
