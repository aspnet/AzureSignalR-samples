function connect(url, connected, disconnected) {
  let connectWithRetry = c => c.start().then(() => connected()).catch(error => {
    console.log('Failed to start SignalR connection: ' + error.message);
    setTimeout(() => connectWithRetry(c), 5000);
  });

  // create connection
  let c = new signalR.HubConnectionBuilder().withUrl(url).withAutomaticReconnect().build();

  c.onreconnecting(() => disconnected());
  c.onreconnected(() => connected());

  connectWithRetry(c);
  return c;
}

async function resizeImage(data, maxSize) {
  return new Promise(resolve => {
    if (!maxSize) {
      resolve();
      return;
    }
    let dataURLToBlob = dataURL => {
      let BASE64_MARKER = ';base64,';
      if (dataURL.indexOf(BASE64_MARKER) == -1) {
        let parts = dataURL.split(',');
        let contentType = parts[0].split(':')[1];
        let raw = parts[1];

        return new Blob([raw], { type: contentType });
      }

      let parts = dataURL.split(BASE64_MARKER);
      let contentType = parts[0].split(':')[1];
      let raw = window.atob(parts[1]);
      let rawLength = raw.length;

      let uInt8Array = new Uint8Array(rawLength);

      for (let i = 0; i < rawLength; ++i) {
        uInt8Array[i] = raw.charCodeAt(i);
      }

      return new Blob([uInt8Array], { type: contentType });
    }

    let reader = new FileReader();
    reader.onload = readerEvent => {
      let image = new Image();
      image.onload = function () {
        let canvas = document.createElement('canvas');
        let ratio = Math.max(image.width / maxSize, image.height / maxSize);
        if (ratio < 1) {
          resolve();
          return;
        }
        canvas.width = image.width / ratio;
        canvas.height = image.height / ratio;
        canvas.getContext('2d').drawImage(image, 0, 0, canvas.width, canvas.height);
        resolve(dataURLToBlob(canvas.toDataURL('image/jpeg')));
      }
      image.src = readerEvent.target.result;
    }
    reader.readAsDataURL(data);
  });
}

let Diagram = function (element, tools) {
  let id;
  let shapes = {};
  let past = [], future = [];
  let timestamp = 0;
  let buffer = [];
  let background;
  let scale = 1;
  let offset = [0, 0];

  let shapeUpdateCallback = shapePatchCallback = shapeRemoveCallback = clearCallback = historyChangeCallback = () => { };

  function generateId() {
    return Math.floor((1 + Math.random()) * 0x100000000).toString(16).substring(1);
  }

  function tryNotify(c) {
    let t = new Date().getTime();
    if (t - timestamp < 250) return;
    c();
    timestamp = t;
  }

  function historyChange() {
    historyChangeCallback(past.length > 0, future.length > 0);
  }

  function applyStyle(e, c, w) {
    return e.fill('none').stroke({ color: c, width: w, linecap: 'round' });
  }

  function translate(x, y) {
    return [offset[0] + x / scale, offset[1] + y / scale];
  }

  function serialize(m) {
    let t = tools[m.kind];
    return {
      color: m.color,
      width: m.width,
      ...t.serialize(m.data)
    };
  }

  function startShape(k, c, w, x, y) {
    if (id) return;
    id = generateId();
    [x, y] = translate(x, y);
    let m = { kind: k, color: c, width: w, data: tools[k].start(x, y) };
    shapes[id] = { view: applyStyle(tools[k].draw(element, m.data), c, w), model: m };
    future = [];
    past.push(id);
    historyChange();
    shapeUpdateCallback(id, k, serialize(m));
  }

  function drawShape(x, y) {
    if (!id) return;
    [x, y] = translate(x, y);
    let s = shapes[id];
    let t = tools[s.model.kind];
    let d = t.move(x, y, s.model.data);
    t.update(s.view, s.model.data);
    if (d) {
      buffer = buffer.concat(d);
      tryNotify(() => {
        shapePatchCallback(id, s.model.kind, t.serialize(buffer));
        buffer = [];
      });
    } else tryNotify(() => shapeUpdateCallback(id, s.model.kind, serialize(s.model)));
  }

  function endShape() {
    if (!id) return;
    let s = shapes[id];
    let t = tools[s.model.kind];
    if (buffer.length > 0) {
      shapePatchCallback(id, s.model.kind, t.serialize(buffer));
      buffer = [];
    } else shapeUpdateCallback(id, shapes[id].model.kind, serialize(shapes[id].model));
    id = null;
  }

  function updateShapeInternal(i, m) {
    if (shapes[i]) {
      shapes[i].model = m;
      tools[m.kind].update(shapes[i].view, m.data);
      applyStyle(shapes[i].view, m.color, m.width);
    } else shapes[i] = { view: applyStyle(tools[m.kind].draw(element, m.data), m.color, m.width), model: m };
  }

  function updateShape(i, k, d) {
    let t = tools[k];
    let m = { color: d.color, width: d.width, kind: k, data: t.deserialize(d) };
    updateShapeInternal(i, m);
  }

  function patchShape(i, d) {
    if (shapes[i]) {
      let m = shapes[i].model;
      let t = tools[m.kind];
      if (d.color) m.color = d.color;
      if (d.width) m.width = d.width;
      m.data = m.data.concat(t.deserialize(d));
      t.update(shapes[i].view, m.data);
      applyStyle(shapes[i].view, m.color, m.width);
    }
  }

  function removeShape(i) {
    if (!shapes[i]) return;
    shapes[i].view.remove();
    delete shapes[i];
  }

  function clear() {
    removeAll();
    clearCallback();
  }

  function removeAll() {
    id = null;
    shapes = {};
    past = [], future = [];
    timestamp = 0;
    buffer = [];
    background = null;
    element.clear();
    historyChange();
  }

  function updateBackground(file) {
    if (background) background.remove();
    background = element.image(file).back();
  }

  function resizeViewbox(w, h) {
    let v = element.viewbox();
    element.viewbox(v.x, v.y, w / scale, h / scale);
  }

  function pan(dx, dy) {
    let v = element.viewbox();
    offset = [v.x + dx / scale, v.y + dy / scale];
    element.viewbox(offset[0], offset[1], v.width, v.height);
  }

  function zoom(r) {
    scale *= r;
    let v = element.viewbox();
    element.viewbox(v.x, v.y, v.width / r, v.height / r);
  }

  function undo() {
    let i = past.pop();
    if (!i) return;
    future.push(shapes[i].model);
    removeShape(i);
    shapeRemoveCallback(i);
    historyChange();
  }

  function redo() {
    let m = future.pop();
    if (!m) return;
    let i = generateId();
    updateShapeInternal(i, m);
    shapeUpdateCallback(i, m.kind, serialize(m));
    past.push(i);
    historyChange();
  }

  return {
    startShape: startShape,
    drawShape: drawShape,
    endShape: endShape,
    updateShape: updateShape,
    patchShape: patchShape,
    removeShape: removeShape,
    clear: clear,
    removeAll: removeAll,
    updateBackground: updateBackground,
    resizeViewbox: resizeViewbox,
    pan: pan,
    zoom: zoom,
    undo: undo,
    redo: redo,
    onShapeUpdate: c => shapeUpdateCallback = c,
    onShapeRemove: c => shapeRemoveCallback = c,
    onShapePatch: c => shapePatchCallback = c,
    onClear: c => clearCallback = c,
    onHistoryChange: c => historyChangeCallback = c
  };
};

let modes = {
  panAndZoom: {
    startOne: p => 0,
    moveOne: (p, pp) => diagram.pan(pp[0] - p[0], pp[1] - p[1]),
    startTwo: (p1, p2) => 0,
    moveTwo: (p1, p2, pp1, pp2) => {
      let r = Math.sqrt(((p2[0] - p1[0]) * (p2[0] - p1[0]) + (p2[1] - p1[1]) * (p2[1] - p1[1]))
        / ((pp2[0] - pp1[0]) * (pp2[0] - pp1[0]) + (pp2[1] - pp1[1]) * (pp2[1] - pp1[1])));
      diagram.pan(pp1[0] - p1[0] / r, pp1[1] - p1[1] / r);
      diagram.zoom(r);
    },
    end: () => 0
  },
  draw: {
    startOne: p => { if (appData.connected.value) diagram.startShape(appData.tool, appData.color, appData.width, p[0], p[1]); },
    moveOne: (p, pp) => { if (appData.connected.value) diagram.drawShape(p[0], p[1]); },
    startTwo: () => 0,
    moveTwo: () => 0,
    end: () => { if (appData.connected.value) diagram.endShape(); }
  }
};

let tools = {
  'Polyline': {
    start: (x, y) => [x, y],
    move: (x, y, d) => { d.push(x, y); return [x, y]; },
    draw: (b, d) => b.polyline(d),
    update: (e, d) => e.plot(d),
    serialize: d => ({
      points: d.reduce((a, c, i) => {
        if (i % 2 === 0) a.push({ x: c, y: d[i + 1] });
        return a;
      }, [])
    }),
    deserialize: d => d.points.reduce((a, c) => a.concat(c.x, c.y), [])
  },
  'Line': {
    start: (x, y) => [x, y, x, y],
    move: (x, y, d) => { d[2] = x; d[3] = y; },
    draw: (b, d) => b.line(d),
    update: (e, d) => e.plot(d),
    serialize: d => ({
      start: { x: d[0], y: d[1] },
      end: { x: d[2], y: d[3] }
    }),
    deserialize: d => [d.start.x, d.start.y, d.end.x, d.end.y]
  },
  'Rect': {
    start: (x, y) => [x, y, x, y],
    move: (x, y, d) => { d[2] = x; d[3] = y; },
    draw: (b, d) => b.rect(Math.abs(d[2] - d[0]), Math.abs(d[3] - d[1])).move(Math.min(d[0], d[2]), Math.min(d[1], d[3])),
    update: (e, d) => e.x(Math.min(d[2], d[0])).y(Math.min(d[1], d[3])).size(Math.abs(d[2] - d[0]), Math.abs(d[3] - d[1])),
    serialize: d => ({
      topLeft: { x: d[0], y: d[1] },
      bottomRight: { x: d[2], y: d[3] }
    }),
    deserialize: d => [d.topLeft.x, d.topLeft.y, d.bottomRight.x, d.bottomRight.y]
  },
  'Circle': {
    start: (x, y) => [x, y, 0],
    move: (x, y, d) => { d[2] = Math.floor(Math.sqrt((d[0] - x) * (d[0] - x) + (d[1] - y) * (d[1] - y))) },
    draw: (b, d) => b.circle(d[2] * 2).cx(d[0]).cy(d[1]),
    update: (e, d) => e.cx(d[0]).cy(d[1]).radius(d[2]),
    serialize: d => ({
      center: { x: d[0], y: d[1] },
      radius: d[2]
    }),
    deserialize: d => [d.center.x, d.center.y, d.radius]
  },
  'Ellipse': {
    start: (x, y) => [x, y, x, y],
    move: (x, y, d) => { d[2] = x; d[3] = y; },
    draw: (b, d) => b.ellipse(Math.abs(d[2] - d[0]), Math.abs(d[3] - d[1])).cx((d[0] + d[2]) / 2).cy((d[1] + d[3]) / 2),
    update: (e, d) => e.cx((d[0] + d[2]) / 2).cy((d[1] + d[3]) / 2).radius(Math.abs(d[2] - d[0]) / 2, Math.abs(d[3] - d[1]) / 2),
    serialize: d => ({
      topLeft: { x: d[0], y: d[1] },
      bottomRight: { x: d[2], y: d[3] }
    }),
    deserialize: d => [d.topLeft.x, d.topLeft.y, d.bottomRight.x, d.bottomRight.y]
  }
};

let connection = connect('/draw', () => {
  appData.connected.value = true;
  diagram.removeAll();
}, () => appData.connected.value = false);

let diagram = new Diagram(SVG('whiteboard'), tools);
diagram.onShapeUpdate((i, k, m) => connection.send(`addOrUpdate${k}`, i, m));
diagram.onShapePatch((i, k, m) => connection.send(`patch${k}`, i, m));
diagram.onShapeRemove(i => connection.send('removeShape', i));
diagram.onClear(() => connection.send('clear'));
diagram.onHistoryChange((p, f) => [appData.hasUndo.value, appData.hasRedo.value] = [p, f]);
connection.on('clear', diagram.removeAll);
connection.on('shapeUpdated', diagram.updateShape);
connection.on('shapePatched', diagram.patchShape);
connection.on('shapeRemoved', diagram.removeShape);
connection.on('backgroundUpdated', i => diagram.updateBackground('/background/' + i));
connection.on('newMessage', (n, m) => appData.messages.push({ name: n, message: m }));
connection.on('userUpdated', n => appData.totalUsers.value = n);

const { createApp, reactive, ref } = Vue;

let appData = {
  diagram,
  connected: ref(false),
  totalUsers: ref(1),
  hasUndo: ref(false),
  hasRedo: ref(false),
  tool: 'Polyline',
  color: 'black',
  width: 1,
  tools: Object.keys(tools),
  colors: ['black', 'grey', 'darkred', 'red', 'orange', 'yellow', 'green', 'deepskyblue', 'indigo', 'purple'],
  widths: [1, 2, 4, 8],
  messages: reactive([]),
  messageColor: 'black',
  name: '',
  draft: '',
  showLog: true,
  maxImageSize: 1920
};

let app = createApp({
  data: () => appData,
  methods: {
    upload: async function (e) {
      let f = document.querySelector('#uploadForm');
      let formData = new FormData(f);
      let b = await resizeImage(e.target.files[0], this.maxImageSize);
      if (b) {
        formData.delete('file');
        formData.append('file', b);
      }
      await fetch('/background/upload', {
        method: 'POST',
        body: formData,
        cache: 'no-cache'
      });

      f.reset();
    },
    zoomIn: () => diagram.zoom(1.25),
    zoomOut: () => diagram.zoom(0.8),
    sendMessage: function () {
      if (!this.draft) return;
      this.messages.push({ name: this.name, message: this.draft });
      connection.send('sendMessage', this.name, this.draft);
      this.draft = '';
    },
    setName: function () { if (this.name) inputName.hide(); },
    toggleLog: function () { this.showLog = !this.showLog; },
    showSettings: () => new bootstrap.Modal(document.querySelector("#settings"), { backdrop: 'static', keyboard: false }).show()
  }
});

app.mount('#app');

let inputNameElement = document.querySelector('#inputName');
// disable keyboard events for username dialog
let inputName = new bootstrap.Modal(inputNameElement, {
  backdrop: 'static',
  keyboard: false
});

// UI initialization
(function () {
  // hook mouse and touch events for whiteboard
  let mode;
  let prev;
  let started;
  let start = p => {
    if (!mode) return;
    prev = p;
  };
  let move = p => {
    if (!mode) return;
    if (prev.length !== p.length) return;
    // do not start if the move is too small
    if (!started && p.length === 1 && Math.abs(p[0][0] - prev[0][0]) < 5 && Math.abs(p[0][1] - prev[0][1]) < 5) return;
    else {
      started = true;
      if (p.length === 1) modes[mode].startOne(prev[0]);
      else if (p.length === 2) modes[mode].startTwo(prev[0], prev[1]);
    }
    if (p.length === 1) modes[mode].moveOne(p[0], prev[0]);
    else if (p.length === 2) modes[mode].moveTwo(p[0], p[1], prev[0], prev[1]);
    prev = p;
  };
  let end = p => {
    if (!mode) return;
    if (started) modes[mode].end();
    prev = started = null;
  };
  let flatten = (ts, f) => {
    let ps = [];
    for (let i = 0; i < ts.length; i++) ps.push(f(ts[i]));
    return ps;
  };

  const whiteboard = document.querySelector('#whiteboard');
  whiteboard.addEventListener('mousedown', e => {
    mode = e.ctrlKey ? 'panAndZoom' : 'draw';
    start([[e.offsetX, e.offsetY]]);
  });

  whiteboard.addEventListener('mousemove', e => {
    move([[e.offsetX, e.offsetY]]);
  });

  whiteboard.addEventListener('mouseup', e => {
    end();
    mode = null;
  });

  whiteboard.addEventListener('touchstart', e => {
    if (e.touches.length > 2) return;
    if (prev) end();
    mode = e.touches.length === 1 ? 'draw' : 'panAndZoom';
    start(flatten(e.touches, t => [t.pageX, t.pageY - 66]));
    e.preventDefault();
  });

  whiteboard.addEventListener('touchmove', e => {
    move(flatten(e.touches, t => [t.pageX, t.pageY - 66]));
    e.preventDefault();
  });

  whiteboard.addEventListener('touchend', e => {
    end();
    mode = null;
    e.preventDefault();
  });

  whiteboard.addEventListener('touchcancel', e => {
    end();
    mode = null;
    e.preventDefault();
  });

  inputNameElement.addEventListener('shown.bs.modal', () => {
    document.querySelector('#username').focus();
  });
  inputName.show();

  // update zoom level for small devices
  let w = window.innerWidth;
  diagram.zoom(w < 576 ? 1 / 3 :
    w < 768 ? 1 / 2 :
      w < 992 ? 2 / 3 :
        w < 1200 ? 5 / 6 :
          1);

  // hook window resize event to set correct viewbox size
  window.onresize = () => diagram.resizeViewbox(document.querySelector('#whiteboard').clientWidth, document.querySelector('#whiteboard').clientHeight);
})();