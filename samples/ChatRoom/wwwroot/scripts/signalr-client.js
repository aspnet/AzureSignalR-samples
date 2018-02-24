(function(f){if(typeof exports==="object"&&typeof module!=="undefined"){module.exports=f()}else if(typeof define==="function"&&define.amd){define([],f)}else{var g;if(typeof window!=="undefined"){g=window}else if(typeof global!=="undefined"){g=global}else if(typeof self!=="undefined"){g=self}else{g=this}g.signalR = f()}})(function(){var define,module,exports;return (function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
// Rough polyfill of https://developer.mozilla.org/en-US/docs/Web/API/AbortController
// We don't actually ever use the API being polyfilled, we always use the polyfill because
// it's a very new API right now.
class AbortController {
    constructor() {
        this.isAborted = false;
    }
    abort() {
        if (!this.isAborted) {
            this.isAborted = true;
            if (this.onabort) {
                this.onabort();
            }
        }
    }
    get signal() {
        return this;
    }
    get aborted() {
        return this.isAborted;
    }
}
exports.AbortController = AbortController;

},{}],2:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
class Base64EncodedHubProtocol {
    constructor(protocol) {
        this.wrappedProtocol = protocol;
        this.name = this.wrappedProtocol.name;
        this.type = 1 /* Text */;
    }
    parseMessages(input) {
        // The format of the message is `size:message;`
        let pos = input.indexOf(":");
        if (pos == -1 || input[input.length - 1] != ';') {
            throw new Error("Invalid payload.");
        }
        let lenStr = input.substring(0, pos);
        if (!/^[0-9]+$/.test(lenStr)) {
            throw new Error(`Invalid length: '${lenStr}'`);
        }
        let messageSize = parseInt(lenStr, 10);
        // 2 accounts for ':' after message size and trailing ';'
        if (messageSize != input.length - pos - 2) {
            throw new Error("Invalid message size.");
        }
        let encodedMessage = input.substring(pos + 1, input.length - 1);
        // atob/btoa are browsers APIs but they can be polyfilled. If this becomes problematic we can use
        // base64-js module
        let s = atob(encodedMessage);
        let payload = new Uint8Array(s.length);
        for (let i = 0; i < payload.length; i++) {
            payload[i] = s.charCodeAt(i);
        }
        return this.wrappedProtocol.parseMessages(payload.buffer);
    }
    writeMessage(message) {
        let payload = new Uint8Array(this.wrappedProtocol.writeMessage(message));
        let s = "";
        for (let i = 0; i < payload.byteLength; i++) {
            s += String.fromCharCode(payload[i]);
        }
        // atob/btoa are browsers APIs but they can be polyfilled. If this becomes problematic we can use
        // base64-js module
        let encodedMessage = btoa(s);
        return `${encodedMessage.length.toString()}:${encodedMessage};`;
    }
}
exports.Base64EncodedHubProtocol = Base64EncodedHubProtocol;

},{}],3:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
class HttpError extends Error {
    constructor(errorMessage, statusCode) {
        super(errorMessage);
        this.statusCode = statusCode;
    }
}
exports.HttpError = HttpError;
class TimeoutError extends Error {
    constructor(errorMessage = "A timeout occurred.") {
        super(errorMessage);
    }
}
exports.TimeoutError = TimeoutError;

},{}],4:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
var TextMessageFormat;
(function (TextMessageFormat) {
    const RecordSeparator = String.fromCharCode(0x1e);
    function write(output) {
        return `${output}${RecordSeparator}`;
    }
    TextMessageFormat.write = write;
    function parse(input) {
        if (input[input.length - 1] != RecordSeparator) {
            throw new Error("Message is incomplete.");
        }
        let messages = input.split(RecordSeparator);
        messages.pop();
        return messages;
    }
    TextMessageFormat.parse = parse;
})(TextMessageFormat = exports.TextMessageFormat || (exports.TextMessageFormat = {}));
var BinaryMessageFormat;
(function (BinaryMessageFormat) {
    // The length prefix of binary messages is encoded as VarInt. Read the comment in
    // the BinaryMessageParser.TryParseMessage for details.
    function write(output) {
        // msgpack5 uses returns Buffer instead of Uint8Array on IE10 and some other browser
        //  in which case .byteLength does will be undefined
        let size = output.byteLength || output.length;
        let lenBuffer = [];
        do {
            let sizePart = size & 0x7f;
            size = size >> 7;
            if (size > 0) {
                sizePart |= 0x80;
            }
            lenBuffer.push(sizePart);
        } while (size > 0);
        // msgpack5 uses returns Buffer instead of Uint8Array on IE10 and some other browser
        //  in which case .byteLength does will be undefined
        size = output.byteLength || output.length;
        let buffer = new Uint8Array(lenBuffer.length + size);
        buffer.set(lenBuffer, 0);
        buffer.set(output, lenBuffer.length);
        return buffer.buffer;
    }
    BinaryMessageFormat.write = write;
    function parse(input) {
        let result = [];
        let uint8Array = new Uint8Array(input);
        const maxLengthPrefixSize = 5;
        const numBitsToShift = [0, 7, 14, 21, 28];
        for (let offset = 0; offset < input.byteLength;) {
            let numBytes = 0;
            let size = 0;
            let byteRead;
            do {
                byteRead = uint8Array[offset + numBytes];
                size = size | ((byteRead & 0x7f) << (numBitsToShift[numBytes]));
                numBytes++;
            } while (numBytes < Math.min(maxLengthPrefixSize, input.byteLength - offset) && (byteRead & 0x80) != 0);
            if ((byteRead & 0x80) !== 0 && numBytes < maxLengthPrefixSize) {
                throw new Error("Cannot read message size.");
            }
            if (numBytes === maxLengthPrefixSize && byteRead > 7) {
                throw new Error("Messages bigger than 2GB are not supported.");
            }
            if (uint8Array.byteLength >= (offset + numBytes + size)) {
                // IE does not support .slice() so use subarray
                result.push(uint8Array.slice
                    ? uint8Array.slice(offset + numBytes, offset + numBytes + size)
                    : uint8Array.subarray(offset + numBytes, offset + numBytes + size));
            }
            else {
                throw new Error("Incomplete message.");
            }
            offset = offset + numBytes + size;
        }
        return result;
    }
    BinaryMessageFormat.parse = parse;
})(BinaryMessageFormat = exports.BinaryMessageFormat || (exports.BinaryMessageFormat = {}));

},{}],5:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
const Errors_1 = require("./Errors");
class HttpResponse {
    constructor(statusCode, statusText, content) {
        this.statusCode = statusCode;
        this.statusText = statusText;
        this.content = content;
    }
}
exports.HttpResponse = HttpResponse;
class HttpClient {
    get(url, options) {
        return this.send(Object.assign({}, options, { method: "GET", url: url }));
    }
    post(url, options) {
        return this.send(Object.assign({}, options, { method: "POST", url: url }));
    }
}
exports.HttpClient = HttpClient;
class DefaultHttpClient extends HttpClient {
    send(request) {
        return new Promise((resolve, reject) => {
            let xhr = new XMLHttpRequest();
            xhr.open(request.method, request.url, true);
            xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
            if (request.headers) {
                request.headers.forEach((value, header) => xhr.setRequestHeader(header, value));
            }
            if (request.responseType) {
                xhr.responseType = request.responseType;
            }
            if (request.abortSignal) {
                request.abortSignal.onabort = () => {
                    xhr.abort();
                };
            }
            if (request.timeout) {
                xhr.timeout = request.timeout;
            }
            xhr.onload = () => {
                if (request.abortSignal) {
                    request.abortSignal.onabort = null;
                }
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(new HttpResponse(xhr.status, xhr.statusText, xhr.response || xhr.responseText));
                }
                else {
                    reject(new Errors_1.HttpError(xhr.statusText, xhr.status));
                }
            };
            xhr.onerror = () => {
                reject(new Errors_1.HttpError(xhr.statusText, xhr.status));
            };
            xhr.ontimeout = () => {
                reject(new Errors_1.TimeoutError());
            };
            xhr.send(request.content || "");
        });
    }
}
exports.DefaultHttpClient = DefaultHttpClient;

},{"./Errors":3}],6:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const Transports_1 = require("./Transports");
const HttpClient_1 = require("./HttpClient");
const ILogger_1 = require("./ILogger");
const Loggers_1 = require("./Loggers");
class HttpConnection {
    constructor(url, options = {}) {
        this.features = {};
        this.logger = Loggers_1.LoggerFactory.createLogger(options.logger);
        this.baseUrl = this.resolveUrl(url);
        options = options || {};
        this.httpClient = options.httpClient || new HttpClient_1.DefaultHttpClient();
        this.connectionState = 2 /* Disconnected */;
        this.options = options;
    }
    start() {
        return __awaiter(this, void 0, void 0, function* () {
            if (this.connectionState !== 2 /* Disconnected */) {
                return Promise.reject(new Error("Cannot start a connection that is not in the 'Disconnected' state."));
            }
            this.connectionState = 0 /* Connecting */;
            this.startPromise = this.startInternal();
            return this.startPromise;
        });
    }
    startInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            try {
                if (this.options.transport === Transports_1.TransportType.WebSockets) {
                    // No need to add a connection ID in this case
                    this.url = this.baseUrl;
                    this.transport = this.createTransport(this.options.transport, [Transports_1.TransportType[Transports_1.TransportType.WebSockets]]);
                }
                else {
                    let headers;
                    if (this.options.accessToken) {
                        headers = new Map();
                        headers.set("Authorization", `Bearer ${this.options.accessToken()}`);
                    }
                    let negotiatePayload = yield this.httpClient.post(this.resolveNegotiateUrl(this.baseUrl), {
                        content: "",
                        headers
                    });
                    let negotiateResponse = JSON.parse(negotiatePayload.content);
                    this.connectionId = negotiateResponse.connectionId;
                    // the user tries to stop the the connection when it is being started
                    if (this.connectionState == 2 /* Disconnected */) {
                        return;
                    }
                    if (this.connectionId) {
                        this.url = this.baseUrl + (this.baseUrl.indexOf("?") === -1 ? "?" : "&") + `id=${this.connectionId}`;
                        this.transport = this.createTransport(this.options.transport, negotiateResponse.availableTransports);
                    }
                }
                this.transport.onreceive = this.onreceive;
                this.transport.onclose = e => this.stopConnection(true, e);
                let requestedTransferMode = this.features.transferMode === 2 /* Binary */
                    ? 2 /* Binary */
                    : 1 /* Text */;
                this.features.transferMode = yield this.transport.connect(this.url, requestedTransferMode, this);
                // only change the state if we were connecting to not overwrite
                // the state if the connection is already marked as Disconnected
                this.changeState(0 /* Connecting */, 1 /* Connected */);
            }
            catch (e) {
                this.logger.log(ILogger_1.LogLevel.Error, "Failed to start the connection. " + e);
                this.connectionState = 2 /* Disconnected */;
                this.transport = null;
                throw e;
            }
            ;
        });
    }
    createTransport(transport, availableTransports) {
        if ((transport === null || transport === undefined) && availableTransports.length > 0) {
            transport = Transports_1.TransportType[availableTransports[0]];
        }
        if (transport === Transports_1.TransportType.WebSockets && availableTransports.indexOf(Transports_1.TransportType[transport]) >= 0) {
            return new Transports_1.WebSocketTransport(this.options.accessToken, this.logger);
        }
        if (transport === Transports_1.TransportType.ServerSentEvents && availableTransports.indexOf(Transports_1.TransportType[transport]) >= 0) {
            return new Transports_1.ServerSentEventsTransport(this.httpClient, this.options.accessToken, this.logger);
        }
        if (transport === Transports_1.TransportType.LongPolling && availableTransports.indexOf(Transports_1.TransportType[transport]) >= 0) {
            return new Transports_1.LongPollingTransport(this.httpClient, this.options.accessToken, this.logger);
        }
        if (this.isITransport(transport)) {
            return transport;
        }
        throw new Error("No available transports found.");
    }
    isITransport(transport) {
        return typeof (transport) === "object" && "connect" in transport;
    }
    changeState(from, to) {
        if (this.connectionState == from) {
            this.connectionState = to;
            return true;
        }
        return false;
    }
    send(data) {
        if (this.connectionState != 1 /* Connected */) {
            throw new Error("Cannot send data if the connection is not in the 'Connected' State");
        }
        return this.transport.send(data);
    }
    stop(error) {
        return __awaiter(this, void 0, void 0, function* () {
            let previousState = this.connectionState;
            this.connectionState = 2 /* Disconnected */;
            try {
                yield this.startPromise;
            }
            catch (e) {
                // this exception is returned to the user as a rejected Promise from the start method
            }
            this.stopConnection(/*raiseClosed*/ previousState == 1 /* Connected */, error);
        });
    }
    stopConnection(raiseClosed, error) {
        if (this.transport) {
            this.transport.stop();
            this.transport = null;
        }
        this.connectionState = 2 /* Disconnected */;
        if (raiseClosed && this.onclose) {
            this.onclose(error);
        }
    }
    resolveUrl(url) {
        // startsWith is not supported in IE
        if (url.lastIndexOf("https://", 0) === 0 || url.lastIndexOf("http://", 0) === 0) {
            return url;
        }
        if (typeof window === 'undefined' || !window || !window.document) {
            throw new Error(`Cannot resolve '${url}'.`);
        }
        let parser = window.document.createElement("a");
        parser.href = url;
        let baseUrl = (!parser.protocol || parser.protocol === ":")
            ? `${window.document.location.protocol}//${(parser.host || window.document.location.host)}`
            : `${parser.protocol}//${parser.host}`;
        if (!url || url[0] != '/') {
            url = '/' + url;
        }
        let normalizedUrl = baseUrl + url;
        this.logger.log(ILogger_1.LogLevel.Information, `Normalizing '${url}' to '${normalizedUrl}'`);
        return normalizedUrl;
    }
    resolveNegotiateUrl(url) {
        let index = url.indexOf("?");
        let negotiateUrl = url.substring(0, index === -1 ? url.length : index);
        if (negotiateUrl[negotiateUrl.length - 1] !== "/") {
            negotiateUrl += "/";
        }
        negotiateUrl += "negotiate";
        negotiateUrl += index === -1 ? "" : url.substring(index);
        return negotiateUrl;
    }
}
exports.HttpConnection = HttpConnection;

},{"./HttpClient":5,"./ILogger":8,"./Loggers":10,"./Transports":12}],7:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const HttpConnection_1 = require("./HttpConnection");
const Observable_1 = require("./Observable");
const JsonHubProtocol_1 = require("./JsonHubProtocol");
const Formatters_1 = require("./Formatters");
const Base64EncodedHubProtocol_1 = require("./Base64EncodedHubProtocol");
const ILogger_1 = require("./ILogger");
const Loggers_1 = require("./Loggers");
var Transports_1 = require("./Transports");
exports.TransportType = Transports_1.TransportType;
var HttpConnection_2 = require("./HttpConnection");
exports.HttpConnection = HttpConnection_2.HttpConnection;
var JsonHubProtocol_2 = require("./JsonHubProtocol");
exports.JsonHubProtocol = JsonHubProtocol_2.JsonHubProtocol;
var ILogger_2 = require("./ILogger");
exports.LogLevel = ILogger_2.LogLevel;
var Loggers_2 = require("./Loggers");
exports.ConsoleLogger = Loggers_2.ConsoleLogger;
exports.NullLogger = Loggers_2.NullLogger;
const DEFAULT_TIMEOUT_IN_MS = 30 * 1000;
class HubConnection {
    constructor(urlOrConnection, options = {}) {
        options = options || {};
        this.timeoutInMilliseconds = options.timeoutInMilliseconds || DEFAULT_TIMEOUT_IN_MS;
        if (typeof urlOrConnection === "string") {
            this.connection = new HttpConnection_1.HttpConnection(urlOrConnection, options);
        }
        else {
            this.connection = urlOrConnection;
        }
        this.logger = Loggers_1.LoggerFactory.createLogger(options.logger);
        this.protocol = options.protocol || new JsonHubProtocol_1.JsonHubProtocol();
        this.connection.onreceive = (data) => this.processIncomingData(data);
        this.connection.onclose = (error) => this.connectionClosed(error);
        this.callbacks = new Map();
        this.methods = new Map();
        this.closedCallbacks = [];
        this.id = 0;
    }
    processIncomingData(data) {
        if (this.timeoutHandle !== undefined) {
            clearTimeout(this.timeoutHandle);
        }
        // Parse the messages
        let messages = this.protocol.parseMessages(data);
        for (var i = 0; i < messages.length; ++i) {
            var message = messages[i];
            switch (message.type) {
                case 1 /* Invocation */:
                    this.invokeClientMethod(message);
                    break;
                case 2 /* StreamItem */:
                case 3 /* Completion */:
                    let callback = this.callbacks.get(message.invocationId);
                    if (callback != null) {
                        if (message.type === 3 /* Completion */) {
                            this.callbacks.delete(message.invocationId);
                        }
                        callback(message);
                    }
                    break;
                case 6 /* Ping */:
                    // Don't care about pings
                    break;
                default:
                    this.logger.log(ILogger_1.LogLevel.Warning, "Invalid message type: " + data);
                    break;
            }
        }
        this.configureTimeout();
    }
    configureTimeout() {
        if (!this.connection.features || !this.connection.features.inherentKeepAlive) {
            // Set the timeout timer
            this.timeoutHandle = setTimeout(() => this.serverTimeout(), this.timeoutInMilliseconds);
        }
    }
    serverTimeout() {
        // The server hasn't talked to us in a while. It doesn't like us anymore ... :(
        // Terminate the connection
        this.connection.stop(new Error("Server timeout elapsed without receiving a message from the server."));
    }
    invokeClientMethod(invocationMessage) {
        let methods = this.methods.get(invocationMessage.target.toLowerCase());
        if (methods) {
            methods.forEach(m => m.apply(this, invocationMessage.arguments));
            if (invocationMessage.invocationId) {
                // This is not supported in v1. So we return an error to avoid blocking the server waiting for the response.
                let message = "Server requested a response, which is not supported in this version of the client.";
                this.logger.log(ILogger_1.LogLevel.Error, message);
                this.connection.stop(new Error(message));
            }
        }
        else {
            this.logger.log(ILogger_1.LogLevel.Warning, `No client method with the name '${invocationMessage.target}' found.`);
        }
    }
    connectionClosed(error) {
        this.callbacks.forEach(callback => {
            callback(undefined, error ? error : new Error("Invocation canceled due to connection being closed."));
        });
        this.callbacks.clear();
        this.closedCallbacks.forEach(c => c.apply(this, [error]));
    }
    start() {
        return __awaiter(this, void 0, void 0, function* () {
            let requestedTransferMode = (this.protocol.type === 2 /* Binary */)
                ? 2 /* Binary */
                : 1 /* Text */;
            this.connection.features.transferMode = requestedTransferMode;
            yield this.connection.start();
            var actualTransferMode = this.connection.features.transferMode;
            yield this.connection.send(Formatters_1.TextMessageFormat.write(JSON.stringify({ protocol: this.protocol.name })));
            this.logger.log(ILogger_1.LogLevel.Information, `Using HubProtocol '${this.protocol.name}'.`);
            if (requestedTransferMode === 2 /* Binary */ && actualTransferMode === 1 /* Text */) {
                this.protocol = new Base64EncodedHubProtocol_1.Base64EncodedHubProtocol(this.protocol);
            }
            this.configureTimeout();
        });
    }
    stop() {
        if (this.timeoutHandle) {
            clearTimeout(this.timeoutHandle);
        }
        return this.connection.stop();
    }
    stream(methodName, ...args) {
        let invocationDescriptor = this.createStreamInvocation(methodName, args);
        let subject = new Observable_1.Subject(() => {
            let cancelInvocation = this.createCancelInvocation(invocationDescriptor.invocationId);
            let message = this.protocol.writeMessage(cancelInvocation);
            this.callbacks.delete(invocationDescriptor.invocationId);
            return this.connection.send(message);
        });
        this.callbacks.set(invocationDescriptor.invocationId, (invocationEvent, error) => {
            if (error) {
                subject.error(error);
                return;
            }
            if (invocationEvent.type === 3 /* Completion */) {
                let completionMessage = invocationEvent;
                if (completionMessage.error) {
                    subject.error(new Error(completionMessage.error));
                }
                else {
                    subject.complete();
                }
            }
            else {
                subject.next(invocationEvent.item);
            }
        });
        let message = this.protocol.writeMessage(invocationDescriptor);
        this.connection.send(message)
            .catch(e => {
            subject.error(e);
            this.callbacks.delete(invocationDescriptor.invocationId);
        });
        return subject;
    }
    send(methodName, ...args) {
        let invocationDescriptor = this.createInvocation(methodName, args, true);
        let message = this.protocol.writeMessage(invocationDescriptor);
        return this.connection.send(message);
    }
    invoke(methodName, ...args) {
        let invocationDescriptor = this.createInvocation(methodName, args, false);
        let p = new Promise((resolve, reject) => {
            this.callbacks.set(invocationDescriptor.invocationId, (invocationEvent, error) => {
                if (error) {
                    reject(error);
                    return;
                }
                if (invocationEvent.type === 3 /* Completion */) {
                    let completionMessage = invocationEvent;
                    if (completionMessage.error) {
                        reject(new Error(completionMessage.error));
                    }
                    else {
                        resolve(completionMessage.result);
                    }
                }
                else {
                    reject(new Error(`Unexpected message type: ${invocationEvent.type}`));
                }
            });
            let message = this.protocol.writeMessage(invocationDescriptor);
            this.connection.send(message)
                .catch(e => {
                reject(e);
                this.callbacks.delete(invocationDescriptor.invocationId);
            });
        });
        return p;
    }
    on(methodName, method) {
        if (!methodName || !method) {
            return;
        }
        methodName = methodName.toLowerCase();
        if (!this.methods.has(methodName)) {
            this.methods.set(methodName, []);
        }
        this.methods.get(methodName).push(method);
    }
    off(methodName, method) {
        if (!methodName || !method) {
            return;
        }
        methodName = methodName.toLowerCase();
        let handlers = this.methods.get(methodName);
        if (!handlers) {
            return;
        }
        var removeIdx = handlers.indexOf(method);
        if (removeIdx != -1) {
            handlers.splice(removeIdx, 1);
        }
    }
    onclose(callback) {
        if (callback) {
            this.closedCallbacks.push(callback);
        }
    }
    createInvocation(methodName, args, nonblocking) {
        if (nonblocking) {
            return {
                type: 1 /* Invocation */,
                target: methodName,
                arguments: args,
            };
        }
        else {
            let id = this.id;
            this.id++;
            return {
                type: 1 /* Invocation */,
                invocationId: id.toString(),
                target: methodName,
                arguments: args,
            };
        }
    }
    createStreamInvocation(methodName, args) {
        let id = this.id;
        this.id++;
        return {
            type: 4 /* StreamInvocation */,
            invocationId: id.toString(),
            target: methodName,
            arguments: args,
        };
    }
    createCancelInvocation(id) {
        return {
            type: 5 /* CancelInvocation */,
            invocationId: id,
        };
    }
}
exports.HubConnection = HubConnection;

},{"./Base64EncodedHubProtocol":2,"./Formatters":4,"./HttpConnection":6,"./ILogger":8,"./JsonHubProtocol":9,"./Loggers":10,"./Observable":11,"./Transports":12}],8:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
var LogLevel;
(function (LogLevel) {
    LogLevel[LogLevel["Trace"] = 0] = "Trace";
    LogLevel[LogLevel["Information"] = 1] = "Information";
    LogLevel[LogLevel["Warning"] = 2] = "Warning";
    LogLevel[LogLevel["Error"] = 3] = "Error";
    LogLevel[LogLevel["None"] = 4] = "None";
})(LogLevel = exports.LogLevel || (exports.LogLevel = {}));

},{}],9:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
const Formatters_1 = require("./Formatters");
class JsonHubProtocol {
    constructor() {
        this.name = "json";
        this.type = 1 /* Text */;
    }
    parseMessages(input) {
        if (!input) {
            return [];
        }
        // Parse the messages
        let messages = Formatters_1.TextMessageFormat.parse(input);
        let hubMessages = [];
        for (var i = 0; i < messages.length; ++i) {
            hubMessages.push(JSON.parse(messages[i]));
        }
        return hubMessages;
    }
    writeMessage(message) {
        return Formatters_1.TextMessageFormat.write(JSON.stringify(message));
    }
}
exports.JsonHubProtocol = JsonHubProtocol;

},{"./Formatters":4}],10:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
const ILogger_1 = require("./ILogger");
class NullLogger {
    log(logLevel, message) {
    }
}
exports.NullLogger = NullLogger;
class ConsoleLogger {
    constructor(minimumLogLevel) {
        this.minimumLogLevel = minimumLogLevel;
    }
    log(logLevel, message) {
        if (logLevel >= this.minimumLogLevel) {
            switch (logLevel) {
                case ILogger_1.LogLevel.Error:
                    console.error(`${ILogger_1.LogLevel[logLevel]}: ${message}`);
                    break;
                case ILogger_1.LogLevel.Warning:
                    console.warn(`${ILogger_1.LogLevel[logLevel]}: ${message}`);
                    break;
                case ILogger_1.LogLevel.Information:
                    console.info(`${ILogger_1.LogLevel[logLevel]}: ${message}`);
                    break;
                default:
                    console.log(`${ILogger_1.LogLevel[logLevel]}: ${message}`);
                    break;
            }
        }
    }
}
exports.ConsoleLogger = ConsoleLogger;
var LoggerFactory;
(function (LoggerFactory) {
    function createLogger(logging) {
        if (logging === undefined) {
            return new ConsoleLogger(ILogger_1.LogLevel.Information);
        }
        if (logging === null) {
            return new NullLogger();
        }
        if (logging.log) {
            return logging;
        }
        return new ConsoleLogger(logging);
    }
    LoggerFactory.createLogger = createLogger;
})(LoggerFactory = exports.LoggerFactory || (exports.LoggerFactory = {}));

},{"./ILogger":8}],11:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
class Subscription {
    constructor(subject, observer) {
        this.subject = subject;
        this.observer = observer;
    }
    dispose() {
        let index = this.subject.observers.indexOf(this.observer);
        if (index > -1) {
            this.subject.observers.splice(index, 1);
        }
        if (this.subject.observers.length === 0) {
            this.subject.cancelCallback().catch((_) => { });
        }
    }
}
exports.Subscription = Subscription;
class Subject {
    constructor(cancelCallback) {
        this.observers = [];
        this.cancelCallback = cancelCallback;
    }
    next(item) {
        for (let observer of this.observers) {
            observer.next(item);
        }
    }
    error(err) {
        for (let observer of this.observers) {
            if (observer.error) {
                observer.error(err);
            }
        }
    }
    complete() {
        for (let observer of this.observers) {
            if (observer.complete) {
                observer.complete();
            }
        }
    }
    subscribe(observer) {
        this.observers.push(observer);
        return new Subscription(this, observer);
    }
}
exports.Subject = Subject;

},{}],12:[function(require,module,exports){
"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const Errors_1 = require("./Errors");
const ILogger_1 = require("./ILogger");
const AbortController_1 = require("./AbortController");
var TransportType;
(function (TransportType) {
    TransportType[TransportType["WebSockets"] = 0] = "WebSockets";
    TransportType[TransportType["ServerSentEvents"] = 1] = "ServerSentEvents";
    TransportType[TransportType["LongPolling"] = 2] = "LongPolling";
})(TransportType = exports.TransportType || (exports.TransportType = {}));
class WebSocketTransport {
    constructor(accessToken, logger) {
        this.logger = logger;
        this.accessToken = accessToken;
    }
    connect(url, requestedTransferMode, connection) {
        return new Promise((resolve, reject) => {
            url = url.replace(/^http/, "ws");
            if (this.accessToken) {
                let token = this.accessToken();
                url += (url.indexOf("?") < 0 ? "?" : "&") + `signalRTokenHeader=${token}`;
            }
            let webSocket = new WebSocket(url);
            if (requestedTransferMode == 2 /* Binary */) {
                webSocket.binaryType = "arraybuffer";
            }
            webSocket.onopen = (event) => {
                this.logger.log(ILogger_1.LogLevel.Information, `WebSocket connected to ${url}`);
                this.webSocket = webSocket;
                resolve(requestedTransferMode);
            };
            webSocket.onerror = (event) => {
                reject();
            };
            webSocket.onmessage = (message) => {
                this.logger.log(ILogger_1.LogLevel.Trace, `(WebSockets transport) data received: ${message.data}`);
                if (this.onreceive) {
                    this.onreceive(message.data);
                }
            };
            webSocket.onclose = (event) => {
                // webSocket will be null if the transport did not start successfully
                if (this.onclose && this.webSocket) {
                    if (event.wasClean === false || event.code !== 1000) {
                        this.onclose(new Error(`Websocket closed with status code: ${event.code} (${event.reason})`));
                    }
                    else {
                        this.onclose();
                    }
                }
            };
        });
    }
    send(data) {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.webSocket.send(data);
            return Promise.resolve();
        }
        return Promise.reject("WebSocket is not in the OPEN state");
    }
    stop() {
        if (this.webSocket) {
            this.webSocket.close();
            this.webSocket = null;
        }
        return Promise.resolve();
    }
}
exports.WebSocketTransport = WebSocketTransport;
class ServerSentEventsTransport {
    constructor(httpClient, accessToken, logger) {
        this.httpClient = httpClient;
        this.accessToken = accessToken;
        this.logger = logger;
    }
    connect(url, requestedTransferMode, connection) {
        if (typeof (EventSource) === "undefined") {
            Promise.reject("EventSource not supported by the browser.");
        }
        this.url = url;
        return new Promise((resolve, reject) => {
            if (this.accessToken) {
                let token = this.accessToken();
                url += (url.indexOf("?") < 0 ? "?" : "&") + `signalRTokenHeader=${token}`;
            }
            let eventSource = new EventSource(url);
            try {
                eventSource.onmessage = (e) => {
                    if (this.onreceive) {
                        try {
                            this.logger.log(ILogger_1.LogLevel.Trace, `(SSE transport) data received: ${e.data}`);
                            this.onreceive(e.data);
                        }
                        catch (error) {
                            if (this.onclose) {
                                this.onclose(error);
                            }
                            return;
                        }
                    }
                };
                eventSource.onerror = (e) => {
                    reject();
                    // don't report an error if the transport did not start successfully
                    if (this.eventSource && this.onclose) {
                        this.onclose(new Error(e.message || "Error occurred"));
                    }
                };
                eventSource.onopen = () => {
                    this.logger.log(ILogger_1.LogLevel.Information, `SSE connected to ${this.url}`);
                    this.eventSource = eventSource;
                    // SSE is a text protocol
                    resolve(1 /* Text */);
                };
            }
            catch (e) {
                return Promise.reject(e);
            }
        });
    }
    send(data) {
        return __awaiter(this, void 0, void 0, function* () {
            return send(this.httpClient, this.url, this.accessToken, data);
        });
    }
    stop() {
        if (this.eventSource) {
            this.eventSource.close();
            this.eventSource = null;
        }
        return Promise.resolve();
    }
}
exports.ServerSentEventsTransport = ServerSentEventsTransport;
class LongPollingTransport {
    constructor(httpClient, accessToken, logger) {
        this.httpClient = httpClient;
        this.accessToken = accessToken;
        this.logger = logger;
        this.pollAbort = new AbortController_1.AbortController();
    }
    connect(url, requestedTransferMode, connection) {
        this.url = url;
        // Set a flag indicating we have inherent keep-alive in this transport.
        connection.features.inherentKeepAlive = true;
        if (requestedTransferMode === 2 /* Binary */ && (typeof new XMLHttpRequest().responseType !== "string")) {
            // This will work if we fix: https://github.com/aspnet/SignalR/issues/742
            throw new Error("Binary protocols over XmlHttpRequest not implementing advanced features are not supported.");
        }
        this.poll(this.url, requestedTransferMode);
        return Promise.resolve(requestedTransferMode);
    }
    poll(url, transferMode) {
        return __awaiter(this, void 0, void 0, function* () {
            let pollOptions = {
                timeout: 120000,
                abortSignal: this.pollAbort.signal,
                headers: new Map(),
            };
            if (transferMode === 2 /* Binary */) {
                pollOptions.responseType = "arraybuffer";
            }
            if (this.accessToken) {
                pollOptions.headers.set("Authorization", `Bearer ${this.accessToken()}`);
            }
            while (!this.pollAbort.signal.aborted) {
                try {
                    let pollUrl = `${url}&_=${Date.now()}`;
                    this.logger.log(ILogger_1.LogLevel.Trace, `(LongPolling transport) polling: ${pollUrl}`);
                    let response = yield this.httpClient.get(pollUrl, pollOptions);
                    if (response.statusCode === 204) {
                        this.logger.log(ILogger_1.LogLevel.Information, "(LongPolling transport) Poll terminated by server");
                        // Poll terminated by server
                        if (this.onclose) {
                            this.onclose();
                        }
                        this.pollAbort.abort();
                    }
                    else if (response.statusCode !== 200) {
                        this.logger.log(ILogger_1.LogLevel.Error, `(LongPolling transport) Unexpected response code: ${response.statusCode}`);
                        // Unexpected status code
                        if (this.onclose) {
                            this.onclose(new Errors_1.HttpError(response.statusText, response.statusCode));
                        }
                        this.pollAbort.abort();
                    }
                    else {
                        // Process the response
                        if (response.content) {
                            this.logger.log(ILogger_1.LogLevel.Trace, `(LongPolling transport) data received: ${response.content}`);
                            if (this.onreceive) {
                                this.onreceive(response.content);
                            }
                        }
                        else {
                            // This is another way timeout manifest.
                            this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Poll timed out, reissuing.");
                        }
                    }
                }
                catch (e) {
                    if (e instanceof Errors_1.TimeoutError) {
                        // Ignore timeouts and reissue the poll.
                        this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Poll timed out, reissuing.");
                    }
                    else {
                        // Close the connection with the error as the result.
                        if (this.onclose) {
                            this.onclose(e);
                        }
                        this.pollAbort.abort();
                    }
                }
            }
        });
    }
    send(data) {
        return __awaiter(this, void 0, void 0, function* () {
            return send(this.httpClient, this.url, this.accessToken, data);
        });
    }
    stop() {
        this.pollAbort.abort();
        return Promise.resolve();
    }
}
exports.LongPollingTransport = LongPollingTransport;
function send(httpClient, url, accessToken, content) {
    return __awaiter(this, void 0, void 0, function* () {
        let headers;
        if (accessToken) {
            headers = new Map();
            headers.set("Authorization", `Bearer ${accessToken()}`);
        }
        yield httpClient.post(url, {
            content,
            headers
        });
    });
}

},{"./AbortController":1,"./Errors":3,"./ILogger":8}]},{},[7])(7)
});