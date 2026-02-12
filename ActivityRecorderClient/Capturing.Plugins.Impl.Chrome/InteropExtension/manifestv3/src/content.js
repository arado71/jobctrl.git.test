"use strict";

import * as InboxSDK from '@inboxsdk/core';

function addJcIdIfNeededComposeView() {

    if (localStorage.getItem("AddJCID") !== "true") {
        return null;
    }
    var body = this.getTextContent();
    console.log(body);
    var subject = this.getSubject();
    console.log("step 1: JCID injection enabled");
    var res = /\[[*]([^*]+)[*]\]/.exec(subject);
    console.log("step 2: JCID extracted from subject");
    if (res != null && res.length > 1) return null;
    res = /\[[*]([^*]+)[*]\]/.exec(body);
    console.log("step 3: JCID extracted from body");
    if (res != null && res.length > 1) return null;

    if (this.isInlineReplyForm()) {
        if (jcid)
            this.setBodyHTML(this.getHTMLContent() + "\r\n<p class=\"JC-ID\">" + jcid + "</p>");
        return;
    }
    var id = Encode();
    id = "[*" + id + "*]";
    if (localStorage.getItem("AddToSubj") === "true") {
        this.setSubject(this.getSubject.concat("   ").concat(id));
    }
    this.setBodyHTML(this.getHTMLContent() + "\r\n<p class=\"JC-ID\">" + id + "</p>");
}

var jcid = null;

function addJcIdIfNeededMessageView(messageView) {
    console.log("step 0: addJcIdIfNeededMessageView");
    if (localStorage.getItem("AddJCID") !== "true") {
        return null;
    }
    if (!messageView.isLoaded()) {
        setTimeout(addJcIdIfNeededMessageView(messageView), 10);
        return;
    }
    var threadView = messageView.getThreadView();

    var body = "";
    for (var bod in threadView.getMessageViews()) {
        body = body.concat(bod);
    }

    var subject = threadView.getSubject();
    var id;
    console.log("step 1: JCID injection enabled");
    var res = /\[[*]([^*]+)[*]\]/.exec(subject);
    console.log("step 2: JCID extracted from subject");
    if (res != null && res.length > 1) return null;
    res = /\[[*]([^*]+)[*]\]/.exec(body);
    console.log("step 3: JCID extracted from data.body");
    if (res != null && res.length > 1) return null;

    messageView.getMessageIDAsync().then(function (messageId) {
        if (messageId != undefined && messageId !== "") {
            console.log("generating jcid from Message-ID: " + messageId);
            id = sha256(messageId);
        } else {
            console.log("generating random jcid");
            id = Encode();
        }

        id = "[*" + id + "*]";
        jcid = id;
        if (localStorage.getItem("AddToSubj") === "true") {
            threadView.setSubject(threadView.getSubject().concat("   ").concat(id));
        }
        var bodyHTML = messageView.getBodyElement();
        var p = document.createElement("P");
        var br = document.createElement("br");
        var t = document.createTextNode(id);
        p.appendChild(t);
        bodyHTML.appendChild(br);
        bodyHTML.appendChild(p);
        return;
    });
    return;
}

var main = function () {
    InboxSDK.load(2, 'sdk_JobCTRL_619a7adc70').then(function (sdk) {
        if (!sdk.User.getEmailAddress().endsWith("gmail.com")) {
            sdk.Compose.registerComposeViewHandler(function (composeView) {
                if (!composeView.getFromContact().emailAddress.endsWith("gmail.com")) {
                    composeView.on('presending', addJcIdIfNeededComposeView);
                }
            });
            sdk.Conversations.registerMessageViewHandler(addJcIdIfNeededMessageView);
            sdk.Conversations.registerThreadViewHandler(function (threadView) {
                threadView.on('destroy', function () {
                    jcid = null;
                });
            });
        }
    });
}

// http://geraintluff.github.io/sha256/
// some modification to fit for Unicode encoding
var sha256 = function getHashCode(input) {

    var ccodes = []; // char codes
    var i, j; // Used as a counter across the whole file

    for (i = 0; i < input.length; ++i) {
        var code = input.charCodeAt(i);
        ccodes = ccodes.concat([code & 0xff, code / 256 >>> 0]);
    }
    var ascii = String.fromCharCode.apply(null, ccodes);

    function rightRotate(value, amount) {
        return (value >>> amount) | (value << (32 - amount));
    };

    var mathPow = Math.pow;
    var maxWord = mathPow(2, 32);
    var lengthProperty = 'length';
    var result = '';

    var words = [];
    var asciiBitLength = ascii[lengthProperty] * 8;

    //* caching results is optional - remove/add slash from front of this line to toggle
    // Initial hash value: first 32 bits of the fractional parts of the square roots of the first 8 primes
    // (we actually calculate the first 64, but extra values are just ignored)
    var hash = sha256.h = sha256.h || [];
    // Round constants: first 32 bits of the fractional parts of the cube roots of the first 64 primes
    var k = sha256.k = sha256.k || [];
    var primeCounter = k[lengthProperty];
    /*/
    var hash = [], k = [];
    var primeCounter = 0;
    //*/

    var isComposite = {};
    for (var candidate = 2; primeCounter < 64; candidate++) {
        if (!isComposite[candidate]) {
            for (i = 0; i < 313; i += candidate) {
                isComposite[i] = candidate;
            }
            hash[primeCounter] = (mathPow(candidate, .5) * maxWord) | 0;
            k[primeCounter++] = (mathPow(candidate, 1 / 3) * maxWord) | 0;
        }
    }

    ascii += '\x80' // Append Ƈ' bit (plus zero padding)
    while (ascii[lengthProperty] % 64 - 56) ascii += '\x00' // More zero padding
    for (i = 0; i < ascii[lengthProperty]; i++) {
        j = ascii.charCodeAt(i);
        if (j >> 8) return; // ASCII check: only accept characters in range 0-255
        words[i >> 2] |= j << ((3 - i) % 4) * 8;
    }
    words[words[lengthProperty]] = ((asciiBitLength / maxWord) | 0);
    words[words[lengthProperty]] = (asciiBitLength)

    // process each chunk
    for (j = 0; j < words[lengthProperty];) {
        var w = words.slice(j, j += 16); // The message is expanded into 64 words as part of the iteration
        var oldHash = hash;
        // This is now the undefinedworking hash", often labelled as variables a...g
        // (we have to truncate as well, otherwise extra entries at the end accumulate
        hash = hash.slice(0, 8);

        for (i = 0; i < 64; i++) {
            var i2 = i + j;
            // Expand the message into 64 words
            // Used below if
            var w15 = w[i - 15],
                w2 = w[i - 2];

            // Iterate
            var a = hash[0],
                e = hash[4];
            var temp1 = hash[7] +
                (rightRotate(e, 6) ^ rightRotate(e, 11) ^ rightRotate(e, 25)) // S1
                +
                ((e & hash[5]) ^ ((~e) & hash[6])) // ch
                +
                k[i]
                // Expand the message schedule if needed
                +
                (w[i] = (i < 16) ? w[i] : (
                    w[i - 16] +
                    (rightRotate(w15, 7) ^ rightRotate(w15, 18) ^ (w15 >>> 3)) // s0
                    +
                    w[i - 7] +
                    (rightRotate(w2, 17) ^ rightRotate(w2, 19) ^ (w2 >>> 10)) // s1
                ) | 0);
            // This is only used once, so *could* be moved below, but it only saves 4 bytes and makes things unreadble
            var temp2 = (rightRotate(a, 2) ^ rightRotate(a, 13) ^ rightRotate(a, 22)) // S0
                +
                ((a & hash[1]) ^ (a & hash[2]) ^ (hash[1] & hash[2])); // maj

            hash = [(temp1 + temp2) | 0].concat(hash); // We don't bother trimming off the extra ones, they're harmless as long as we're truncating when we do the slice()
            hash[4] = (hash[4] + temp1) | 0;
        }

        for (i = 0; i < 8; i++) {
            hash[i] = (hash[i] + oldHash[i]) | 0;
        }
    }

    for (i = 0; i < 8; i++) {
        for (j = 3; j + 1; j--) {
            var b = (hash[i] >> (j * 8)) & 255;
            result += ((b < 16) ? 0 : '') + b.toString(16);
        }
    }
    var res = "";
    for (i = 8; i > 0; i--) {
        for (j = 2; j > 0; j--) {
            var r1 = parseInt(result.slice(2 * i - j, 2 * i - j + 1), 16);
            var r2 = parseInt(result.slice(2 * i - j + 16, 2 * i - j + 17), 16);
            var r3 = parseInt(result.slice(2 * i - j + 48, 2 * i - j + 49), 16);
            res += (r1 ^ r2 ^ r3).toString(16);
        }
    }

    return Encode(res);
}

function Encode(input) {

    var hash = "";
    var alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    var alphabetLength = alphabet.length;

    var rest = input;

    if (input == null) {
        for (var i = 0; i < 11; i++)
            hash += alphabet.charAt(Math.floor(Math.random() * alphabetLength));
        return hash;
    }

    while (hash.length < 11) {
        var div = divide(rest, alphabetLength);
        var index = parseInt(div.modulo);
        hash = hash.concat(alphabet[index]);
        rest = div.result;
    }

    return hash;

}

function divide(a, b) {
    var mod = 0;
    var res = "";
    for (var i = 0; i < a.length; i++) {
        var nextNum = parseInt(a[i], 16);
        mod = mod * 16 + nextNum;

        var temp = Math.floor(mod / b);
        res += temp.toString(16);
        mod = mod - temp * b;
    }
    return {
        result: res,
        modulo: mod
    };
}

InboxSDK.load(2, 'sdk_JobCTRL_619a7adc70').then(function (sdk) {
    console.log(sdk.User.getEmailAddress());
    if (!sdk.User.getEmailAddress().endsWith("gmail.com")) {
        sdk.Compose.registerComposeViewHandler(function (composeView) {
            if (!composeView.getFromContact().emailAddress.endsWith("gmail.com")) {
                composeView.on('presending', addJcIdIfNeededComposeView);
            }
        });
        sdk.Conversations.registerMessageViewHandler(addJcIdIfNeededMessageView);
        sdk.Conversations.registerThreadViewHandler(function (threadView) {
            threadView.on('destroy', function () {
                jcid = null;
            });
        });
    }
});
