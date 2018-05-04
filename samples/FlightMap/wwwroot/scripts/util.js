function compDegAngle(src, dest) {
    var latVec = dest.latitude - src.latitude;
    var longVec = dest.longitude - src.longitude;
    var x = longVec;
    var y = -latVec;
    const eps = 1e-6;
    if (Math.abs(latVec) < eps) {
        if (longVec > 0) {
            return 0;
        }
        return 180;
    }
    if (Math.abs(longVec) < eps) {
        if (latVec < 0.) {
            return 90;
        } else {
            return -90;
        }

    }

    var r = Math.sqrt(x*x + y*y);
    var cos = x / r;
    var angle = Math.acos(cos);
    if (y < 0) {
        angle = Math.PI * 2 - angle;
    }
    return angle * 180 / Math.PI;
}

function compDegAnglePt(pt1, pt2) {
    var x = pt2.x - pt1.x;
    var y = pt2.y - pt1.y;
    const eps = 1e-6;
    if (Math.abs(pt2.y - pt1.y) < eps) {
        if (pt2.x - pt1.x > 0) {
            return 0;
        }
        return 180;
    }
    if (Math.abs(pt2.x - pt1.x) < eps) {
        if (pt2.y - pt1.y < 0.) {
            return -90;
        } else {
            return 90;
        }

    }

    var r = Math.sqrt(x*x + y*y);
    var cos = x / r;
    var angle = Math.acos(cos);
    if (y < 0) {
        angle = Math.PI * 2 - angle;
    }
    return angle * 180 / Math.PI;
}

function loc2pt(loc) {
    return map.tryLocationToPixel(loc, Microsoft.Maps.PixelReference.control);
}

function interpolatePosition(src, dest, curTimestamp, startTimeStamp, duration) {
    if (duration == 0) {
        return dest;
    }
    var latVec = dest.latitude - src.latitude;
    var longVec = dest.longitude - src.longitude;

    var ratio = (curTimestamp - startTimeStamp) / duration;
    var curLat = src.latitude + latVec * ratio;
    var curLong = src.longitude + longVec * ratio;

    return new Microsoft.Maps.Location(curLat, curLong);
}

function showTime(time) {
    var date = new Date(time);
    var hours = date.getHours();
    var minutes = "0" + date.getMinutes();
    var seconds = "0" + date.getSeconds();
    var formattedTime = hours + ':' + minutes.substr(-2) + ':' + seconds.substr(-2);
    $('#time').text(formattedTime);
}