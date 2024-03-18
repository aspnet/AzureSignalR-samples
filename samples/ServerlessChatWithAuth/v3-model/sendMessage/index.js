module.exports = async function (context, req) {
    const message = req.body;
    message.sender = req.headers && req.headers['x-ms-client-principal-name'] || '';

    let recipientUserId = '';
    if (message.recipient) {
        recipientUserId = message.recipient;
        message.isPrivate = true;
    }

    return {
        'userId': recipientUserId,
        'target': 'newMessage',
        'arguments': [message]
    };
};