function startSignalR(tableId, group) {
    // Declare a proxy to reference the hub.
    var notificationHub = $.connection.notificationHub;
    $.connection.hub.url = "/signalr";
    // Create a function that the hub can call to broadcast messages.
    notificationHub.client.updateItem = function (item) {
        // Add the message to the page.
        let tr = $('#' + tableId + item.Id);
        if (tr) {
            tr.children().remove();
            appendCells(tr, item);
        }
    };
    // Start the connection.
    var connection = $.connection.hub;

    connection.disconnected(function () {
        reconnectOnFail("Disconnected.");
    });

    startConnectionWithRetry(connection);
    return notificationHub;

    function startConnectionWithRetry(hubConnection) {
        hubConnection.start()
            .done(async function () {
                let items = await notificationHub.server.listenGroup(group);
                $(`#${tableId} tbody tr`).remove();
                items.forEach(item => {
                    let tr = $(`<tr id="${tableId}${item.Id}" />`);
                    appendCells(tr, item);
                    $(`#${tableId} tbody`).append(tr);
                });
                $('#status').html(`<h5 class='success'>Connected</h5>`);
            });
    }

    function reconnectOnFail(message) {
        $('#status').html(`<h5 class='error'>${message} Reconnecting...</h5>`);
        setTimeout(function () {
            startConnectionWithRetry(connection)
        }, 1000);
    }
};

function appendRow(tableId, item) {
    let tr = $(`<tr id="${tableId}${item.Id}" />`);
    appendCells(tr, item);
    $(`#${tableId} tbody`).append(tr);
}

function appendCells(tr, item) {
    return tr.append($('<td />').text(item.Id)) +
        tr.append($('<td />').text(item.SubmitAt)) +
        tr.append($('<td />').text(item.Message)) +
        tr.append($('<td />').text(item.ProcessedAt)) +
        tr.append($('<td />').text(item.Status));
}
