var port;

function handleRequest(request) {
    console.debug("Received message from extension: " + JSON.stringify(request));
    var result;
    try {
        if (request.evalString) {
            result = { result: getDomElementPropertyWithEval(request.evalString) };
        } else if (request.everyTab && request.everyTab.eval) {
            result = { result: getDomElementPropertyWithEval(request.everyTab.eval) };
        } else {
            result = { result: getDomElementProperty(request.selector, request.propertyName) };
        }
    } catch (ex) {
        result = { error: "Error occured while executing injected script: " + ex.message };
    }
    console.debug("Send response to extension: " + JSON.stringify(result));
    port.postMessage(result);
}

function getDomElementProperty(selector, propertyName) {
    if (!document.querySelector) return null;
    var element = document.querySelector(selector);
    if (element) return element[propertyName];
    else return null;
}

function getDomElementPropertyWithEval(evalString) {
    return eval(evalString);
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

function guid() {
    function s4() {
      return Math.floor((1 + Math.random()) * 0x10000)
        .toString(16)
        .substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
  }

function cleanup() {
	console_debug = null;
    console_info = null;
    port.onMessage.removeListener(handleRequest);
    port.disconnect();
    port = null;
}

ToggleLogging();
port = chrome.runtime.connect({name:"jobctrl-" + guid()});
port.onMessage.addListener(handleRequest);
window.unload += cleanup;
