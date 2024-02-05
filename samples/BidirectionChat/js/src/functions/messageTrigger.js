const { app, output } = require('@azure/functions');
const newMessageTarget = "newMessage";

const signalR = output.generic({
    type: 'signalR',
    name: 'signalR',
    hubName: 'hub',
    connectionStringSetting: 'AzureSignalRConnectionString',
});

app.generic("connected",
    {
        trigger: { "type": "signalRTrigger", "name": "connected", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "connected", "category": "connections" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.log(`Connection ${triggerInput.ConnectionId} is connected.`)
            context.extraOutputs.set(signalR, {
                "target": "newConnection",
                "arguments": [new NewConnection(triggerInput.ConnectionId, "text")],
            });
        }
    })

app.generic("disconnected",
    {
        trigger: { "type": "signalRTrigger", "name": "disconnected", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "disconnected", "category": "connections" },
        handler: (triggerInput, context) => { context.log(`Connection ${triggerInput.ConnectionId} is disconnected.`) }
    })


class NewConnection {
    constructor(connectionId, auth) {
        this.ConnectionId = connectionId;
        this.auth = auth;
    }
}

app.generic("broadcast",
    {
        trigger: { "type": "signalRTrigger", "name": "broadcast", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "broadcast", "category": "messages" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.extraOutputs.set(signalR, {
                "target": newMessageTarget,
                "arguments": [new NewMessage(triggerInput, triggerInput.Arguments[0])]
            });
        }
    })

app.generic("sendToGroup",
    {
        trigger: { "type": "signalRTrigger", "name": "sendToGroup", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "sendToGroup", "category": "messages" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.extraOutputs.set(signalR, {
                "target": newMessageTarget,
                "groupName": triggerInput.Arguments[0],
                "arguments": [new NewMessage(triggerInput, triggerInput.Arguments[1])],
            });
        }
    })

app.generic("sendToUser",
    {
        trigger: { "type": "signalRTrigger", "name": "sendToUser", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "sendToUser", "category": "messages" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.extraOutputs.set(signalR, {
                "target": newMessageTarget,
                "userId": triggerInput.Arguments[0],
                "arguments": [new NewMessage(triggerInput, triggerInput.Arguments[1])],
            });
        }
    })

app.generic("sendToConnection",
    {
        trigger: { "type": "signalRTrigger", "name": "sendToConnection", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "sendToConnection", "category": "messages" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.extraOutputs.set(signalR, {
                "target": newMessageTarget,
                "connectionId": triggerInput.Arguments[0],
                "arguments": [new NewMessage(triggerInput, triggerInput.Arguments[1])],
            });
        }
    })

app.generic("joinGroup",
    {
        trigger: { "type": "signalRTrigger", "name": "joinGroup", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "joinGroup", "category": "messages" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.extraOutputs.set(signalR, {
                "connectionId": triggerInput.Arguments[0],
                "groupName": triggerInput.Arguments[1],
                "action": "add",
            });
        }
    })

app.generic("leaveGroup",
    {
        trigger: { "type": "signalRTrigger", "name": "leaveGroup", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "leaveGroup", "category": "messages" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.extraOutputs.set(signalR, {
                "connectionId": triggerInput.Arguments[0],
                "groupName": triggerInput.Arguments[1],
                "action": "remove",
            });
        }
    })

app.generic("joinUserToGroup",
    {
        trigger: { "type": "signalRTrigger", "name": "joinUserToGroup", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "joinUserToGroup", "category": "messages" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.extraOutputs.set(signalR, {
                "userId": triggerInput.Arguments[0],
                "groupName": triggerInput.Arguments[1],
                "action": "add",
            });
        }
    })

app.generic("leaveUserFromGroup",
    {
        trigger: { "type": "signalRTrigger", "name": "leaveUserFromGroup", "direction": "in", "hubName": "hub", "connectionStringSetting": "AzureSignalRConnectionString", "event": "leaveUserFromGroup", "category": "messages" },
        extraOutputs: [signalR],
        handler: (triggerInput, context) => {
            context.extraOutputs.set(signalR, {
                "userId": triggerInput.Arguments[0],
                "groupName": triggerInput.Arguments[1],
                "action": "remove",
            });
        }
    })

class NewMessage {
    constructor(invocationContext, message) {
        this.Sender = invocationContext.UserId ? invocationContext.UserId : '';
        this.ConnectionId = invocationContext.ConnectionId;
        this.Text = message;
    }
}