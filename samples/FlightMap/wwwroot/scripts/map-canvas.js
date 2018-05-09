function flightDataReceived(duration, aircrafts, ind, serverTimestamp, timestamp) {
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

function updateVisitors(totalVisitors) {
    $("#counter").text(`${totalVisitors} joined`);
}

function getMap() {
    map = new Microsoft.Maps.Map(document.getElementById('myMap'), {
        center: new Microsoft.Maps.Location(39.9611755, -82.9987942),
        mapTypeId: Microsoft.Maps.MapTypeId.road,
        liteMode: true,
        zoom: 6
    });

    Microsoft.Maps.registerModule('CanvasOverlayModule', '/scripts/CanvasOverlayModule.js');
    Microsoft.Maps.loadModule('CanvasOverlayModule', function () {
        overlay = new CanvasOverlay();
        map.layers.insert(overlay);

        var connectWithRetry = c => c.start().catch(error => {
            console.log("Failed to start SignalR connection: " + error.message);
            setTimeout(() => connectWithRetry(c), 5000);
        });

        // create connection
        var connection = new signalR.HubConnectionBuilder()
            .withUrl('/flightData')
            .build();

        // setup callbacks
        connection.on('updateAircraft', flightDataReceived);
        connection.on('updateVisitors', updateVisitors);

        // auto reconnect when connection is closed
        connection.onclose(() => {
            console.log("Disconnected, try to reconnect.");
            connectWithRetry(connection);
        });

        connectWithRetry(connection);
    });
}

function addAircrafts(aircraftList) {
    fabric.Image.fromURL('images/plane-white.png', (img) => {
        var addedAircrafts = aircraftList.filter(a => !(a.icao in aircraftDict)).forEach(a => {
            var icao = a.icao;
            var loc = new Microsoft.Maps.Location(a.lat, a.long);
            var pt = loc2pt(loc);
            var newImg = new fabric.Image(img.getElement(), {
                left: pt.x,
                top: pt.y,
                angle: 0,
                opacity: 1.0,
                originX: 'center',
                originY: 'center'
            });
            newImg.key = icao;
            overlay._fabric.add(newImg);
            aircraftDict[icao] = { obj: newImg, loc: loc };
        });
    });
}

function initAircrafts(aircraftList) {
    clearAllAircrafts();
    addAircrafts(aircraftList);
}

function clearAllAircrafts() {
    for (var key in aircraftDict) {
        overlay._fabric.remove(aircraftDict[key].obj);
    }
    aircraftDict = {};
}

function clearAircrafts(newAircraftList) {
    var curKeys = {};
    var l = newAircraftList.length;
    for (var i = 0; i < l; i++) {
        var key = newAircraftList[i]['icao'];
        curKeys[key] = 1;
        if (newAircraftList[i]['Gnd'] == true && key in aircraftDict) {
            overlay._fabric.remove(aircraftDict[key].obj);
            delete aircraftDict[key];
        }
    }

    // clear aircrafts not in list any more
    for (key in aircraftDict) {
        if (!(key in curKeys)) {
            overlay._fabric.remove(aircraftDict[key].obj);
            delete aircraftDict[key];
        }
    }
}

function prepareMoveData(newAircraftList) {
    return newAircraftList.map(a => {
        var from = aircraftDict[a.icao].loc;
        var to = new Microsoft.Maps.Location(a.lat, a.long);
        var angle = compDegAnglePt(loc2pt(from), loc2pt(to));
        return {
            icao: a.icao,
            from: from,
            to: to,
            angle: angle
        };
    });
}

function moveAircrafts(moveData) {
    var startTime = globalStart = new Date().getTime();

    var animate = function () {
        if (startTime !== globalStart) return;
        var curTime = new Date().getTime();
        var elapsedTime = curTime - startTime;
        if (elapsedTime > updateDuration) return;
        showTime(curTimestamp + elapsedTime * speedup);

        // update location
        moveData.forEach(d => {
            var loc = interpolatePosition(d.from, d.to, curTime, startTime, updateDuration);
            var pt = loc2pt(loc);
            aircraftDict[d.icao].obj.left = pt.x;
            aircraftDict[d.icao].obj.top = pt.y;
            aircraftDict[d.icao].obj.angle = d.angle;
            aircraftDict[d.icao].obj.setCoords();
            aircraftDict[d.icao].loc = loc;
        });

        // next frame
        fabric.util.requestAnimFrame(animate, overlay._fabric.getElement());
        overlay._fabric.renderAll();
    };

    animate();
}

function updateAircrafts(newAircraftList) {
    if (!overlay._fabric) return;
    addAircrafts(newAircraftList);
    clearAircrafts(newAircraftList);
    moveAircrafts(prepareMoveData(newAircraftList));
}
