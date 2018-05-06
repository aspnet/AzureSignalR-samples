function configureConnection(connection) {
    var updateAircraftsCallback = function (duration, aircrafts, ind, serverTimestamp, timestamp) {
        speedup = (timestamp - curTimestamp) / duration;
        curTimestamp = timestamp;
        updateDuration = duration;
        if (ind == 0)
            isInit = false;
        if (!isInit) {
            initAircrafts(aircrafts);
            isInit = true;
        } else
            updateAircrafts(aircrafts);
    };

    var updateVisitorsCallback = (totalVisitors) => {
        $("#counter").text(`${totalVisitors} joined`);
    }

    // Create a function that the hub can call to broadcast messages.
    connection.on('updateAircraft', updateAircraftsCallback);
    connection.on('updateVisitors', updateVisitorsCallback);
}
