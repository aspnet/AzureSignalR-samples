var connection = null
var realUserName = null
updateConnectionStatus(false)

document.getElementById('userName').addEventListener('keydown', function (event) {
    if (!realUserName && event.key === 'Enter') {
        submitName();
    }
});

document.getElementById('chatInput').addEventListener('keydown', function (event) {
    const textValue = document.getElementById('chatInput').value;
    if (textValue && event.key === 'Enter') {
        sendMessage();
    }
});

function submitName() {
    const userName = document.getElementById('userName').value;
    if (userName) {
        document.getElementById('namePrompt').classList.add('hidden');
        document.getElementById('groupSelection').classList.remove('hidden');
        document.getElementById('userNameDisplay').innerText = userName;

        realUserName = userName;
    } else {
        alert('Please enter your name');
    }
}

function createGroup() {
    const groupName = Math.random().toString(36).substr(2, 6);
    joinGroupWithName(groupName);
}

function joinGroup() {
    const groupName = document.getElementById('groupName').value;
    if (groupName) {
        joinGroupWithName(groupName);
    } else {
        alert('Please enter a group name');
    }
}

function joinGroupWithName(groupName) {
    document.getElementById('groupSelection').classList.add('hidden');
    document.getElementById('chatGroupName').innerText = 'Group: ' + groupName;
    document.getElementById('chatPage').classList.remove('hidden');

    connection = new signalR.HubConnectionBuilder().withUrl(`/groupChat`).withAutomaticReconnect().build();
    bindConnectionMessages(connection);
    connection.start().then(() => {
        updateConnectionStatus(true);
        onConnected(connection);
        connection.send("JoinGroup", groupName);
    }).catch(error => {
        updateConnectionStatus(false);
        console.error(error);
    })
}

function bindConnectionMessages(connection) {
    connection.on('newMessage', (name, message) => {
        appendMessage(false, `${name}: ${message}`);
    });
    connection.on('newMessageWithId', (name, id, message) => {
        appendMessageWithId(id, `${name}: ${message}`);
    });
    connection.onclose(() => {
        updateConnectionStatus(false);
    });
}

function onConnected(connection) {
    console.log('connection started');
}

function sendMessage() {
    const message = document.getElementById('chatInput').value;
    if (message) {
        appendMessage(true, message);
        document.getElementById('chatInput').value = '';
        connection.send("Chat", realUserName, message);
    }
}

function appendMessage(isSender, message) {
    const chatMessages = document.getElementById('chatMessages');
    const messageElement = createMessageElement(message, isSender, null)
    chatMessages.appendChild(messageElement);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function appendMessageWithId(id, message) {
    // We update the full message
    const chatMessages = document.getElementById('chatMessages');
    if (document.getElementById(id)) {
        let messageElement = document.getElementById(id);
        messageElement.innerText = message;
    } else {
        let messageElement = createMessageElement(message, false, id);
        chatMessages.appendChild(messageElement);
    }
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function createMessageElement(message, isSender, id) {
    const messageElement = document.createElement('div');
    messageElement.classList.add('message', isSender ? 'sent' : 'received');
    messageElement.innerText = message;
    if (id) {
        messageElement.id = id;
    }
    return messageElement;
}

function updateConnectionStatus(isConnected) {
    const statusElement = document.getElementById('connectionStatus');
    if (isConnected) {
        statusElement.innerText = 'Connected';
        statusElement.classList.remove('status-disconnected');
        statusElement.classList.add('status-connected');
    } else {
        statusElement.innerText = 'Disconnected';
        statusElement.classList.remove('status-connected');
        statusElement.classList.add('status-disconnected');
    }
}