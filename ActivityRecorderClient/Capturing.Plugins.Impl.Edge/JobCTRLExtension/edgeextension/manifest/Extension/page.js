function handleRequest(request, sender, sendResponse) {
    console.debug("Received message from extension (" + sender.id + "): " + JSON.stringify(request));
    var result;
    try {
        if (request.evalString) {
            result = { result: getDomElementPropertyWithEval(request.evalString) };
        } else {
            result = { result: getDomElementProperty(request.selector, request.propertyName) };
        }
    } catch (ex) {
        result = { error: "Error occured while executing injected script: " + ex.message };
    }
    console.debug("Send response to extension (" + sender.id + "): " + JSON.stringify(result));
    sendResponse(result);
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

ToggleLogging();
chrome.runtime.onMessage.addListener(handleRequest);

