package com.signalr;

import com.google.gson.Gson;
import com.microsoft.azure.functions.ExecutionContext;
import com.microsoft.azure.functions.HttpMethod;
import com.microsoft.azure.functions.HttpRequestMessage;
import com.microsoft.azure.functions.HttpResponseMessage;
import com.microsoft.azure.functions.HttpStatus;
import com.microsoft.azure.functions.annotation.AuthorizationLevel;
import com.microsoft.azure.functions.annotation.FunctionName;
import com.microsoft.azure.functions.annotation.HttpTrigger;
import com.microsoft.azure.functions.annotation.TimerTrigger;
import com.microsoft.azure.functions.signalr.*;
import com.microsoft.azure.functions.signalr.annotation.*;

import org.apache.commons.io.IOUtils;


import java.io.IOException;
import java.io.InputStream;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.net.http.HttpResponse.BodyHandlers;
import java.nio.charset.StandardCharsets;
import java.util.Optional;

public class Function {
    private static String Etag = "";
    private static String StarCount;

    @FunctionName("index")
    public HttpResponseMessage run(
            @HttpTrigger(
                name = "req",
                methods = {HttpMethod.GET},
                authLevel = AuthorizationLevel.ANONYMOUS)HttpRequestMessage<Optional<String>> request,
            final ExecutionContext context) throws IOException {
        
        InputStream inputStream = getClass().getClassLoader().getResourceAsStream("content/index.html");
        String text = IOUtils.toString(inputStream, StandardCharsets.UTF_8.name());
        return request.createResponseBuilder(HttpStatus.OK).header("Content-Type", "text/html").body(text).build();
    }

    @FunctionName("negotiate")
    public SignalRConnectionInfo negotiate(
            @HttpTrigger(
                name = "req",
                methods = { HttpMethod.POST },
                authLevel = AuthorizationLevel.ANONYMOUS) HttpRequestMessage<Optional<String>> req,
            @SignalRConnectionInfoInput(
                name = "connectionInfo",
                hubName = "serverless") SignalRConnectionInfo connectionInfo) {
                
        return connectionInfo;
    }

    @FunctionName("broadcast")
    @SignalROutput(name = "$return", hubName = "serverless")
    public SignalRMessage broadcast(
        @TimerTrigger(name = "timeTrigger", schedule = "*/5 * * * * *") String timerInfo) throws IOException, InterruptedException {
        HttpClient client = HttpClient.newHttpClient();
        HttpRequest req = HttpRequest.newBuilder().uri(URI.create("https://api.github.com/repos/azure/azure-signalr")).header("User-Agent", "serverless").header("If-None-Match", Etag).build();
        HttpResponse<String> res = client.send(req, BodyHandlers.ofString());
        if (res.headers().firstValue("Etag").isPresent())
        {
            Etag = res.headers().firstValue("Etag").get();
        }
        if (res.statusCode() == 200)
        {
            Gson gson = new Gson();
            GitResult result = gson.fromJson(res.body(), GitResult.class);
            StarCount = result.stargazers_count;
        }
        
        return new SignalRMessage("newMessage", "Current start count of https://github.com/Azure/azure-signalr is:".concat(StarCount));
    }

    class GitResult {
        public String stargazers_count;
    }
}
