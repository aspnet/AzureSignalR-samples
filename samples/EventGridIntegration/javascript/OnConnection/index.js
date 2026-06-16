const { TableClient } = require('@azure/data-tables');

const tableName = 'connection';
const tableClient = TableClient.fromConnectionString(process.env.AZURE_STORAGE_CONNECTION_STRING, tableName);

// Create the table once per process. createTable does not throw if it already exists.
let ensureTablePromise;
const ensureTable = () => {
    if (!ensureTablePromise) {
        ensureTablePromise = tableClient.createTable().catch((error) => {
            ensureTablePromise = undefined;
            throw error;
        });
    }
    return ensureTablePromise;
};

module.exports = async function (context, eventGridEvent) {
    context.log(typeof eventGridEvent);
    context.log(eventGridEvent);

    await ensureTable();

    // Use resource name and hub as partition key and row key separately
    let partitionKey = getLastPart(eventGridEvent.topic);
    let rowKey = eventGridEvent.data.hubName;
    let operation;
    let newConnectionCount;
    let token = true;

    while (token) {
        try {
            let entity;
            try {
                entity = await tableClient.getEntity(partitionKey, rowKey);
                operation = 'replace';
            } catch (error) {
                context.log(error);
                operation = 'insert';
            }

            if (operation === 'replace') {
                newConnectionCount = parseInt(entity.Count, 10) + (eventGridEvent.eventType == 'Microsoft.SignalRService.ClientConnectionConnected' ? 1 : -1);
                await tableClient.updateEntity({
                    partitionKey: partitionKey,
                    rowKey: rowKey,
                    Count: newConnectionCount,
                }, 'Replace', { etag: entity.etag });
                token = false;
            } else if (operation === 'insert') {
                newConnectionCount = eventGridEvent.eventType == 'Microsoft.SignalRService.ClientConnectionConnected' ? 1 : 0;
                await tableClient.createEntity({
                    partitionKey: partitionKey,
                    rowKey: rowKey,
                    Count: newConnectionCount,
                });
                token = false;
            }
        } catch (error) {
            context.log(error);
        }
    }
    
    if (eventGridEvent.eventType == 'Microsoft.SignalRService.ClientConnectionConnected') {
        let message = new Map();
        message.text = 'Welcome to Serverless Chat';
        message.sender = '__SYSTEM__';
        context.bindings.sendToConnection = [{
            "connectionId": eventGridEvent.data.connectionId,
            "target": "newMessage",
            "arguments": [ message ]
        }];
    }

    context.bindings.broadcast = [{
        "target": "connectionCount",
        "arguments": [ newConnectionCount ]
    }];
};

const getLastPart = (data) => {
    let n = data.lastIndexOf('/');
    if (n == -1) {
        return data;
    } else {
        return data.substring(n+1);
    }
};
