var debugCnt = 2;
function configureConnection(connection) {
    var updateAircraftsCallback = function (duration, aircrafts, ind, serverTimestamp, timestamp, speedupRatio) {
        var now = new Date().getTime();
        if (isInit == true  && debugCnt >=4 && receiveTimestamp + updateDuration > now - 10) stopAnimation = true;
        debugCnt = (debugCnt+1) % 100 + isInit * 10;
        console.log('debugCnt', debugCnt, 'stopAnimation', stopAnimation, 'minus', receiveTimestamp + updateDuration - now);
        listCache = aircrafts;
        receiveTimestamp = now;

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
