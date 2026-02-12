var port;
var timeout;
var tabPorts = [];

function connected(p) {
    tabPorts[p.sender.tab.id] = p;
    tabPorts[p.sender.tab.id].onMessage.addListener(sendNativeMessage);
}

function disconnected(p) {
    tabPorts[p.sender.tab.id].onMessage.removeListener(sendNativeMessage);
    tabPorts[p.sender.tab.id] = null;
}

function sendNativeMessage(message) {
    if (message === null) {
        message = {
             error: "Error occured while connecting to active tab: " + chrome.runtime.lastError.message
        };
    }
    console.debug("Send message to native messaging host: " + JSON.stringify(message));
    port.postMessage(message);
}

function onNativeMessage(message) {
    console.debug("Received message from native messaging host: " + JSON.stringify(message));

    if (message.extensionCommand) {
        execExtensionCommand(message.extensionCommand, function (result) {
            sendNativeMessage(result);
        });
    } else {
        sendRequestToPage(message);
    }
}

function onDisconnected() {
    console.info("Disconnected from native messaging host: " + chrome.runtime.lastError.message);
    port.onMessage.removeListener(onNativeMessage);
    port.onDisconnect.removeListener(onDisconnected);
    port = null;
    if (timeout != null)
        clearTimeout(timeout);
    timeout = setTimeout(connect, 1000);
}

function connect() {
    var hostName = "com.tct.jobctrl";
    console.info("Connecting to native messaging host: " + hostName);
    port = chrome.runtime.connectNative(hostName);
    port.onMessage.addListener(onNativeMessage);
    port.onDisconnect.addListener(onDisconnected);
}

//TODO: use connection based communication 
function sendRequestToPage(message) {
    if (message.everyTab) {
        if (message.everyTab.url === null) {
            sendNativeMessage({ error: "Error, url wasn't provided." });
        }
        if (message.everyTab.title === null) {
            sendNativeMessage({ error: "Error, title wasn't provided." });
        }
        chrome.tabs.query({
            url: message.everyTab.url,
            title: message.everyTab.title
        },function(tabs) {
                if (!tabs || tabs.length === 0) {
                    sendNativeMessage({ result: "" });
                    return;
                }
                if (tabs.length === 1) {
                    if (tabPorts[tabs[0].id])
                        tabPorts[tabs[0].id].postMessage(message);
                    else
                        sendNativeMessage({
                            error: "Tab not found in the ported tabs."
                        });
                    return;
                }
                iterator = 0;
                browserTabs = tabs;
                tabPorts[tabs[iterator].id].onMessage.removeListener(sendNativeMessage);
                tabPorts[tabs[iterator].id].onMessage.addListener(returnOrIterate);
                tabPorts[tabs[iterator].id].postMessage(message);
            });
    } else {
        chrome.tabs.query({ active: true, lastFocusedWindow: true },
            function(tabs) {
                if (tabs && tabs[0] && tabPorts[tabs[0].id])
                    tabPorts[tabs[0].id].postMessage(message);
            });
    }
}

function returnOrIterate(message) {
    tabPorts[browserTabs[iterator].id].onMessage.addListener(sendNativeMessage);
    if (message) {
        browserTabs = null;
        sendNativeMessage(message);
    } else {
        if (tabs.length > iterator + 1) {
            iterator = iterator + 1;
            tabPorts[browserTabs[iterator].id].onMessage.removeListener(sendNativeMessage);
            tabPorts[browserTabs[iterator].id].onMessage.addListener(returnOrIterate);
            tabPorts[browserTabs[iterator].id].postMessage(message);
        } else {
            browserTabs = null;
            sendNativeMessage(message);
        }
    }
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
	callback(currentTitle);
}

function getActiveTabUrl(callback) {
	callback(currentUrl);
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

function cleanup() {
    if (timeout != null)
        clearTimeout(timeout);
    if (port != null) {
        port.onMessage.removeListener(onNativeMessage);
        port.onDisconnect.removeListener(onDisconnected);
        port = null;
    }
}
//ToggleLogging();
chrome.runtime.onConnect.addListener(connected);
connect();

var currentUrl = "";
var currentTitle = "";

function onUrlUpdated(tabId, changeInfo, tabInfo) {
	if ((changeInfo.title || changeInfo.url) && tabInfo.active) {
		updateUrlAndTitle(tabInfo.url, tabInfo.title);
	}
}

function onActiveTabChanged(activeInfo) {
	chrome.tabs.get(activeInfo.tabId, function(x) {
		updateUrlAndTitle(x.url, x.title);
	});
}

function updateUrlAndTitle(url, title) {
	currentUrl = url;
	currentTitle = title;
	console.debug("Url changed to:", url, " ; title changed to: ", title);
}

function onWindowFocusChanged(windowId) {
    chrome.tabs.query({ active: true, lastFocusedWindow: true }, tabs => {
        if (tabs && tabs[0]) {
            currentUrl = tabs[0].url;
            currentTitle = tabs[0].title;
        }
    });
}

chrome.tabs.onUpdated.addListener(onUrlUpdated);
chrome.tabs.onActivated.addListener(onActiveTabChanged);
chrome.windows.onFocusChanged.addListener(onWindowFocusChanged);

/* InboxSDK */
chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
    if (message.type === 'inboxsdk__injectPageWorld' && sender.tab) {
      if (chrome.scripting) {
        // MV3
        let documentIds;
        let frameIds;
        if (sender.documentId) {
          // Protect against https://github.com/w3c/webextensions/issues/8 in
          // browsers (Chrome 106+) that support the documentId property.
          // Protections for other browsers happen inside the injected script.
          documentIds = [sender.documentId];
        } else {
          frameIds = [sender.frameId];
        }
        chrome.scripting.executeScript({
          target: { tabId: sender.tab.id, documentIds, frameIds },
          world: 'MAIN',
          files: ['pageWorld.js'],
        });
        sendResponse(true);
      } else {
        // MV2 fallback. Tell content script it needs to figure things out.
        sendResponse(false);
      }
    }
  });
  