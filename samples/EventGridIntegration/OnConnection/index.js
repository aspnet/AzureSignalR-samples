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

    var newConnectionCount;
    var token = true;

    // Use resource name and hub as partition key and row key separately
    var partitionKey = getLastPart(eventGridEvent.topic);
    var rowKey = eventGridEvent.data.hubName;

    var deltaCount = eventGridEvent.eventType == 'Microsoft.SignalRService.ClientConnectionConnected' ? 1 : -1

    while (token) {
        try {
            try {
                var entity = await getEntry(partitionKey, rowKey);
                newConnectionCount = parseInt(entity.Count._) + deltaCount;
                entity.Count._ = newConnectionCount;
                await replaceEntity(entity);
                token = false;

            } catch (error) {
                newConnectionCount = eventGridEvent.eventType == 'Microsoft.SignalRService.ClientConnectionConnected' ? 1 : 0;
                var entryGen = azure.TableUtilities.entityGenerator;
                var entity = {
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
        var message = new Map();
        message.text = 'Welcome to Serverless Chat'
        message.sender = '__SYSTEM__'
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
    var n = data.lastIndexOf('/');
    if (n == -1) {
        return data;
    } else {
        return data.substring(n+1);
    }
}