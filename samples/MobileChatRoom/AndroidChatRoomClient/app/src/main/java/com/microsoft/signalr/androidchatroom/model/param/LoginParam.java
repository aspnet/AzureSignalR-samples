package com.microsoft.signalr.androidchatroom.model.param;

public class LoginParam {
    private String username;
    private String deviceUuid;

    public LoginParam(String username, String deviceUuid) {
        this.username = username;
        this.deviceUuid = deviceUuid;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public String getDeviceUuid() {
        return deviceUuid;
    }

    public void setDeviceUuid(String deviceUuid) {
        this.deviceUuid = deviceUuid;
    }
}
