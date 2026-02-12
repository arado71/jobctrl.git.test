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
    if (evalString.startsWith("(function(){\r\n\tvar myTo = new Set();")) return getGmailTo();
    if (evalString.startsWith("(function () {\r\n    var subject = \"\";")) return getGmailJcId();
    if (evalString.startsWith("(function () {\r\n    var res2 = /\\?ui=2&view=btop/i.exec(document.location.search);\r\n    if (res2 != null && res2.length > 0) {\r\n        var e = document.querySelector(\'input[name=\"f")) return getGmailFrom();
    if (evalString.startsWith("(function () {\r\n    var res2 = /\\?ui=2&view=btop/i.exec(document.location.search);\r\n    if (res2 != null && res2.length > 0) {\r\n        var e = document.querySelector(\'input[name=\"s")) return getGmailSubject();
    if (evalString.startsWith("(function () {\r\n    var res2 = /\\?ui=2&view=btop/i.exec(document.location.search)\r\n")) return getGmailId();
    if (evalString.startsWith("(function () {localStorage.setItem(\"AddJCID\"")) return setGetJCIdSettings(evalString); 
    return '';
    //return eval(evalString);
}

function setGetJCIdSettings(evalString) {
    if(evalString[47] == 't') {
        localStorage.setItem("AddJCID",true);
    } else {
        localStorage.setItem("AddJCID",false);
    }
    if(evalString[87] == 't' || evalString[88] == 't') {
        localStorage.setItem("AddToSubj", true);
    } else {
        localStorage.setItem("AddToSubj", false);
    }
    return localStorage.getItem("AddJCID")+ " " + localStorage.getItem("AddToSubj");
}

function getGmailFrom() {

    var res2 = /\?ui=2&view=btop/i.exec(document.location.search);
    if (res2 != null && res2.length > 0) {
        var e = document.querySelector('input[name="from"]');
        if (e == null) return "";
        return e.value;
    }
    var res = /\?compose=([0-9abcdef]+)/i.exec(document.location.hash);
    if (res != null && res.length > 1) {
        return '';
    }
    var eFrom = document.querySelectorAll('div[role="main"] div[class~="h7"] div[class~="G3"][class~="G2"] span[email][class~="gD"]');
    if (eFrom.length == 0) return '';
    return eFrom[eFrom.length - 1].attributes['email'].nodeValue;
}

function getGmailTo() {

    var myTo = new Set();
    var res2 = /\?ui=2&view=btop/i.exec(document.location.search);
    if (res2 != null && res2.length > 0) {
        [].forEach.call(
            document.querySelectorAll('div[role="main"] td[class="eV"] span[email]'),
            function (el) {
                myTo.add(el.attributes["email"].nodeValue);
            }
        );
    } else {
        var res = /\?compose=([0-Z]+)/i.exec(document.location.hash);
        if (res != null && res.length > 1) {
            document.querySelectorAll('div[class="nH nn"] span[email]').forEach(
                function (el) {
                    myTo.add(el.attributes["email"].nodeValue);
                }
            );
        } else {
            var e = document.querySelector('div[class="amn"]');
            if (e == null) {
                [].forEach.call(
                    document.querySelectorAll('div[role="main"] td[class="Iy"] span[email]'),
                    function (el) {
                        myTo.add(el.attributes["email"].nodeValue);
                    }
                );
            } else {
                var prt = document.querySelectorAll('div[role="main"] div[class~="h7"] div[class~="G3"][class~="G2"]');
                if (prt.length == 0) return '';
                prt = prt[prt.length - 1];
                if (prt == null) return '';
                [].forEach.call(
                    prt.querySelectorAll('span[email][class="g2"]'),
                    function (el) {
                        myTo.add(el.attributes['email'].nodeValue);
                    }
                );
            }
        }
    }
    var arr = new Array();
    myTo.forEach(function (e) { arr.push(e); });
    return arr.join(';');
}

function getGmailSubject() {
    var res2 = /\?ui=2&view=btop/i.exec(document.location.search);
    if (res2 != null && res2.length > 0) {
        var e = document.querySelector('input[name="subjectbox"]');
        if (e == null) return "";
        return e.value;
    }
    var res = /\?compose=([0-Z]+)/i.exec(document.location.hash);
    if (res != null && res.length > 1) {
        var e = document.querySelector('div[class="nH"] input[name="subjectbox"]');
        if (e == null) return "";
        return e.value;
    }
    var subj = document.querySelector('h2.hP')
    if (subj) return subj.textContent || subj.innerText;
    return null;
}

function getGmailId() {

    var res2 = /\?ui=2&view=btop/i.exec(document.location.search)
    if (res2 != null && res2.length > 0) {
        var e = document.querySelector('div[class="M9"] form input[name="composeid"]');
        if (e == null) return "";
        return e.value;
    }
    var res = /\?compose=([0-9abcdef]+)/i.exec(document.location.hash);
    if (res != null && res.length > 1) return res[1];
    res = '';
    [].forEach.call(
        document.querySelectorAll('.adn'),
        function (el) {
            res = el.getAttribute("data-legacy-message-id");
        }
    );
    return res;
}

function getGmailJcId() {

    var subject = "";
    var res2 = /\?ui=2&view=btop/i.exec(document.location.search);
    if (res2 != null && res2.length > 0) {
        var e = document.querySelector('input[name="subjectbox"]');
        if (e == null) subject = "";
        subject = e.value;
    }
    var res = /\?compose=([0-9abcdef]+)/i.exec(document.location.hash);
    if (res != null && res.length > 1) {
        var e = document.querySelector('div[class="nH Hd"] input[name="subjectbox"]');
        if (e == null) subject = "";
        subject = e.value;
    }
    var subj = document.querySelector('h2.hP');
    if (subj)
        subject = subj.textContent || subj.innerText;

    var res3 = /\[[*]([^*]+)[*]\]/.exec(subject);
    if (res3 != null && res3.length > 1)
        return res3[res3.length - 1];

    var body = document.querySelector('div[class="nH aHU"]');
    if (body == null)
        body = document.querySelector('div[class="ii gt"] div');
    if (body != null) {
        var res = /\[[*]([^*]+)[*]\]/.exec(body.textContent);
        if (res != null && res.length > 1) return res[res.length - 1];
    }
    var jcidp = document.querySelector('p[class="JC-ID"]');
    if (jcidp == null) return '';
    var res = /\[[*]([^*]+)[*]\]/.exec(jcidp.textContent);
    if (res != null && res.length > 1) return res[res.length - 1];

    return '';
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
port = chrome.runtime.connect({ name: "jobctrl-" + guid() });
port.onMessage.addListener(handleRequest);
window.unload += cleanup;
