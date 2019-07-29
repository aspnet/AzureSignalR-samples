const azure = require('azure-storage');

const tableService = azure.createTableService();
const tableName = 'connection';

tableService.createTableIfNotExists(tableName, function(error, result, response){
    if(result.created){
        context.log(`Table ${tableName} created`);
    }
});

module.exports = async function (context, eventGridEvent) {
    context.log(typeof eventGridEvent);
    context.log(eventGridEvent);

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
                entity = await getEntry(partitionKey, rowKey);
                operation = 'replace';
            } catch (error) {
                context.log(error);
                operation = 'insert';
            }

            if (operation === 'replace') {
                newConnectionCount = parseInt(entity.Count._) + (eventGridEvent.eventType == 'Microsoft.SignalRService.ClientConnectionConnected' ? 1 : -1);
                entity.Count._ = newConnectionCount;
                await replaceEntity(entity);
                token = false;
            } else if (operation === 'insert') {
                newConnectionCount = eventGridEvent.eventType == 'Microsoft.SignalRService.ClientConnectionConnected' ? 1 : 0;
                let entryGen = azure.TableUtilities.entityGenerator;
                entity = {
                    PartitionKey: entryGen.String(partitionKey),
                    RowKey: entryGen.String(rowKey),
                    Count: entryGen.Int32(newConnectionCount),
                };
                await insertEntity(entity);
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

const getEntry = (partitionKey, rowKey) => new Promise((resolve, reject) => {
    tableService.retrieveEntity(tableName, partitionKey, rowKey, (error, result, response) => {
        if (error) {
            reject(error);
        } else {
            resolve(result);
        }
    });
});

const replaceEntity = (entry) => new Promise((resolve, reject) => {
    tableService.replaceEntity(tableName, entry, (error, result, response) => {
        if (error) {
            reject(error);
        } else {
            resolve(result);
        }
    });
});

const insertEntity = (entry) => new Promise((resolve, reject) => {
    tableService.insertEntity(tableName, entry, (error, result, response) => {
        if (error) {
            reject(error);
        } else {
            resolve(result);
        }
    });
});

const getLastPart = (data) => {
    let n = data.lastIndexOf('/');
    if (n == -1) {
        return data;
    } else {
        return data.substring(n+1);
    }
};
