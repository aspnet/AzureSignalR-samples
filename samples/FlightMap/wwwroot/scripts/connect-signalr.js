function configureConnection(connection) {
    var updateAircraftsCallback = function (duration, aircrafts, ind, serverTimestamp, timestamp) {
        var now = new Date().getTime();
        if (isInit == true  && avoidStopAtStart >=2 && receiveTimestamp + updateDuration > now - 10) 
            stopCurAnimation = true;
        avoidStopAtStart = (avoidStopAtStart+1) % 100 + isInit * 10;
        aircraftListCache = aircrafts;
        receiveTimestamp = now;

        speedup = (timestamp - curTimestamp) / duration;
        curTimestamp = timestamp;
        updateDuration = duration;
        console.log("delay from server to client:", new Date().getTime() - serverTimestamp, ' ms');
        aircraftJsonStrCache = aircrafts;
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
    connection.on('startUpdate', updateAircraftsCallback);
    connection.on('updateAircraft', updateAircraftsCallback);
    connection.on('updateVisitors', updateVisitorsCallback);
}
