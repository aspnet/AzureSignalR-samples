if (process.argv.length < 8) {
  console.log('node generate <time> <plane_count> <top> <bottom> <left> <right>');
  return;
}

var time = Number.parseInt(process.argv[2]),
    planeCount = Number.parseInt(process.argv[3]),
    top = Number.parseFloat(process.argv[4]),
    bottom = Number.parseFloat(process.argv[5]),
    left = Number.parseFloat(process.argv[6]),
    right = Number.parseFloat(process.argv[7]);

var current = new Date().getTime();
var rand = () => [top + (bottom - top) * Math.random(), left + (right - left) * Math.random()];
var planes = Array(planeCount).fill().map((v, i) => ({
  icao: i.toString(),
  from: rand(),
  to: rand()
}));
var count = time * 60 / 5;
var data = Array(count + 1).fill().map((v, i) => planes.map(p => ({
  Icao: p.icao,
  PosTime: current + 5000 * i,
  Lat: p.from[0] + (p.to[0] - p.from[0]) * i / count,
  Long: p.from[1] + (p.to[1] - p.from[1]) * i / count,
})));

console.log(JSON.stringify(data));
