function configureConnection(connection) {
    var updateAircraftsCallback = function (duration, aircrafts, ind, serverTimestamp, timestamp, speedupRatio) {
        curTimestamp = timestamp;
        speedup = speedupRatio;
        updateDuration = duration;
        console.log("dt = ", new Date().getTime() - serverTimestamp);
        console.log('updateDuration', updateDuration);
        aircraftJsonStrCache = aircrafts;
        if (ind == 0)
            isInit = false;
        if (!isInit) {
            initAircraft(aircrafts);
            isInit = true;
        } else
            updateAircraft(aircrafts);

    };

    var countVisitorsCallback = (totalVisitors) => {
        $("#counter-checkin").text(`${totalVisitors} joined`);
    }

    // Create a function that the hub can call to broadcast messages.
    connection.on('startUpdate', updateAircraftsCallback);
    connection.on('updateAircraft', updateAircraftsCallback);
    connection.on('countVisitors', countVisitorsCallback);
}
