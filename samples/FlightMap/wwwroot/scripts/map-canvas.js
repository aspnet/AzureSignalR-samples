function getMap() {
    map = new Microsoft.Maps.Map(document.getElementById('myMap'), {
        center: new Microsoft.Maps.Location(39.9611755, -82.9987942),
        mapTypeId: Microsoft.Maps.MapTypeId.road,
        liteMode: true,
        zoom: zoomLevel
    });
    pushpinLayer = new Microsoft.Maps.Layer();
    map.layers.insert(pushpinLayer);

    Microsoft.Maps.registerModule('CanvasOverlayModule', '/scripts/CanvasOverlayModule.js');
    Microsoft.Maps.loadModule('CanvasOverlayModule', function () {
        overlay = new CanvasOverlay();
        map.layers.insert(overlay);
    });

    return map;
}

function addPins(aircraftList) {
    fabric.Image.fromURL('images/plane-white.png', (img) => {
        var l = aircraftList.length;
        console.log(aircraftList.length + ' planes are flying.');
        added = {};
        for (var i = 0; i < l; i++) {
            var aircraft = aircraftList[i];
            var key = aircraft['icao'];
            if (key in aircraftDict) continue;
            var location = new Microsoft.Maps.Location(aircraft.lat, aircraft.long);
            added[key] = location;
        }
        console.log('added cnt ', Object.keys(added).length);
        for (var key in added) {
            var pt = loc2pt(added[key]);
            var img2 = new fabric.Image(img.getElement(), {
                left: pt.x,
                top: pt.y,
                angle: 0,
                opacity: 0.0,
                originX: 'center',
                originY: 'center'
            });
            img2.key = key;
            overlay._fabric.add(img2);
            aircraftDict[key] = { obj: img2, loc: added[key], rotate: false };
        }
    });

}

function initAircraft(aircraftList) {
    console.log('initAircraft');
    clearAllPlanes();
    addPins(aircraftList);
}

function clearAllPlanes() {
    for (var key in aircraftDict) {
        overlay._fabric.remove(aircraftDict[key].obj);
    }
    aircraftDict = {};
}

function clearPlanes(newAircraftList) {
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

    // clear planes not in list any more
    for (key in aircraftDict) {
        if (!(key in curKeys)) {
            overlay._fabric.remove(aircraftDict[key].obj);
            delete aircraftDict[key];
        }
    }
}

function movePins(newAircraftList) {
    var startTime = new Date().getTime();
    var angles = {};
    var dAngles = {};
    var oAngles = {};
    newAircraftList.map((ac) => {
        if (ac.icao in aircraftDict) {
            oAngles[ac.icao] = aircraftDict[ac.icao].obj.angle;
            var from = aircraftDict[ac.icao].loc;
            var to = new Microsoft.Maps.Location(ac.lat, ac.long);
            var toAngle = compDegAnglePt(loc2pt(from), loc2pt(to));
            var fromAngle = aircraftDict[ac.icao].obj.angle;
            angles[ac.icao] = toAngle;
            dAngles[ac.icao] = toAngle - fromAngle;
            if (Math.abs(toAngle + 360 - fromAngle) <= 180) {
                angles[ac.icao] = toAngle + 360;
                dAngles[ac.icao] = toAngle + 360 - fromAngle;
            } else if (Math.abs(toAngle - 360 - fromAngle) <= 180) {
                angles[ac.icao] = toAngle - 360;
                dAngles[ac.icao] = toAngle - 360 - fromAngle;
            }
        }
        return ac;
    });

    var isInitAngle = false;
    var frames = 0;
    var animatePins = function () {

        frames++;
        var curTime = new Date().getTime();
        var elapseTime = curTime - startTime;
        showTime(curTimestamp + elapseTime * speedup);

        // exit animation
        if (curTime >= startTime + updateDuration) {
            console.log('fps:', Math.round(frames / updateDuration * 1000));
            // update aircraftDict
            newAircraftList.map((ac, i) => {
                if (ac.icao in aircraftDict) {
                    aircraftDict[ac.icao].loc = new Microsoft.Maps.Location(ac.lat, ac.long);
                }
                return ac;
            });
            if (isInitAngle)
                for (var key in aircraftDict) {
                    aircraftDict[key].rotate = true;
                }
            return;
        }
        // update location
        for (var i = 0; i < newAircraftList.length; i++) {
            var ac = newAircraftList[i];
            if (!(ac.icao in aircraftDict)) continue;
            var from = aircraftDict[ac.icao].loc;
            var to = new Microsoft.Maps.Location(ac.lat, ac.long);
            var loc = interpolatePosition(from, to, curTime, startTime, updateDuration);
            var pt = loc2pt(loc);
            aircraftDict[ac.icao].obj.left = pt.x;
            aircraftDict[ac.icao].obj.top = pt.y;
            if (Object.keys(angles).length != 0) {
                // update angle
                aircraftDict[ac.icao].obj.angle = angles[ac.icao];
                isInitAngle = true;

                // update opacity
                if (ac.icao in added && isInit == true) aircraftDict[ac.icao].obj.opacity = 1.0;
            }
        }

        // next frame
        fabric.util.requestAnimFrame(animatePins, overlay._fabric.getElement());
        overlay._fabric.renderAll();
    };

    animatePins();

}

function updateAircraft(newAircraftList) {
    addPins(newAircraftList);
    clearPlanes(newAircraftList);
    movePins(newAircraftList);
}
