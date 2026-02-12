var socket, path;

function sendNativeMessage(message) {
    if (message == null) {
        message = { error: "Error occured while connecting to active tab: " + chrome.runtime.lastError.message };
    }
    console.debug("Send message to native messaging host: " + JSON.stringify(message));
    socket.send(JSON.stringify(message));
}

function onNativeMessage(message) {
	message = message.replace(/(?:\r\n|\r|\n)/g, '');
	console.debug("Received message from native messaging host: " + JSON.stringify(message));
	var msg = JSON.parse(message);
    if (msg.extensionCommand) {
        execExtensionCommand(msg.extensionCommand, function (result) {
            sendNativeMessage(result);
        });
    } else {
        sendRequestToPage(msg);
    }
}

function onDisconnected() {
    console.info("Disconnected from native messaging host: " + chrome.runtime.lastError.message);
    port = null;
    setTimeout(connect, 1000);
}

function connect() {
    path = 'ws://127.215.40.10:9696';
    try {
        socket = new WebSocket(path);
        socket.onopen = function () {
            console.log("Opening a connection...");
            window.identified = false;
            //socket.send("Connected");
        };
        socket.onclose = function (evt) {
            console.log("I'm sorry. Bye!");
        };
        socket.onmessage = function (evt) {
            onNativeMessage(evt.data);
        };
        socket.onerror = function (evt) {
            console.log("ERR: " + evt);
			setTimeout(function(){
			connect(); }, 2000);
        };
    } catch (e) {
		console.error('===> WebSocket creation error :: ', e);
    }
}

//TODO: use connection based communication 
function sendRequestToPage(message) {
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        var tab = tabs[0];
        chrome.tabs.sendMessage(tab.id, message, sendNativeMessage);
    });
}

function execExtensionCommand(command, callback) {
    try {
        switch (command) {
            case "GetActiveTabTitle":
                getActiveTabTitle(function (title) {
                    callback({ result: title });
                });
                break;
            case "GetActiveTabUrl":
                getActiveTabUrl(function (url) {
                    callback({ result: url });
                });
                break;
            default:
                callback({ error: "Undefined extension command: " + command });
        }
    } catch (e) {
        callback({ error: "Extension command (" + command + ") failed: " + e.message });
    }
}

function getActiveTabTitle(callback) {
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        var tab = tabs[0];
        callback(tab.title);
    });

}

function getActiveTabUrl(callback) {
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        var tab = tabs[0];
        callback(tab.url);
    });
}

var console_debug = function () { };
var console_info = function () { };
function ToggleLogging() {
    var temp = console.debug;
    console.debug = console_debug;
    console_debug = temp;
    temp = console.info;
    console.info = console_info;
    console_info = temp;
}

ToggleLogging();
connect();
