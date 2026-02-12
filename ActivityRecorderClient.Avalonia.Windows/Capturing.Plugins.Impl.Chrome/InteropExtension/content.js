function refresh() {
    if (document.getElementById('loading').style.display === "none") {
        insertscripts();
    } else {
        setTimeout(insertscripts, 1000);
    }
}

function insertscripts() {
    var k = document.createElement('script');
    k.src = chrome.extension.getURL('inboxsdk.js');
    (document.head || document.documentElement).appendChild(k);

    var h = document.createElement('script');
    h.src = chrome.extension.getURL('main.js');
    (document.head || document.documentElement).appendChild(h);
}

refresh();