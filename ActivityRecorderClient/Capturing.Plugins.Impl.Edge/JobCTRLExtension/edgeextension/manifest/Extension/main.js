var gmail;

function refresh(f) {
    if ((/in/.test(document.readyState)) || (typeof Gmail === "undefined")) {
        setTimeout('refresh(' + f + ')', 10);
    } else {
        f();
    }
}


var main = function () {
    // NOTE: Always use the latest version of gmail.js from
    // https://github.com/KartikTalwar/gmail.js
    gmail = new Gmail();
    if (gmail.get.user_email() && !gmail.get.user_email().endsWith("gmail.com")) {
        gmail.observe.before('send_message', addJcIdIfNeeded);
        gmail.observe.before("open_email",
            function (id, url, body, xhr) {
                if (localStorage.getItem("AddJCID") == "true") {

                    var email = new gmail.dom.email(id);
                    var body = email.body();
                    var emData = gmail.get.email_data(id);
                    var res = /\[[*]([^*]+)[*]\]/.exec(emData.subject);

                    if (res != null && res.length > 1) return;
                    res = /\[[*]([^*]+)[*]\]/.exec(body);
                    if (res != null && res.length > 1) return;

                    var id = Encode(convertToInt(id));
                    id = "[*" + id + "*]";
                    email.body(body + "\r\n <p class=\"JC-ID\">" + id + "</p>");
                }
            });
    }
}

function addJcIdIfNeeded(url, body, data, xhr) {
    if (localStorage.getItem("AddJCID") == "true") {

        var res = /\[[*]([^*]+)[*]\]/.exec(data.subject);
        if (res != null && res.length > 1) return;
        res = /\[[*]([^*]+)[*]\]/.exec(data.body);
        if (res != null && res.length > 1) return;
        res = /\[[*]([^*]+)[*]\]/.exec(body);
        if (res != null && res.length > 1) return;
        res = /\[[*]([^*]+)[*]\]/.exec(gmail.dom.email_body());
        if (res != null && res.length > 1) return;
        var body_params = xhr.xhrParams.body_params;
        res = /\[[*]([^*]+)[*]\]/.exec(body_params.uet);
        if (res != null && res.length > 1) return;


        var id = "";
        var idLen = gmail.get.email_ids().length;
        console.log(idLen);
        if (idLen > 1 && data.threads != undefined && data.threads[1].reply_to_id != "") {
            console.log("generating jcid from reply_to_id");
            id = Encode(data.threads[1].reply_to_id);
        }
        else if (data.thread_id != undefined && data.thread_id != "") {
            console.log("generating jcid from thread");
            id = Encode(data.thread_id);
        }
        else if (data.rm != undefined && data.rm != "undefined" && data.rm != null && data.rm != "") {
            console.log("generating jcid from previous mail id");
            id = Encode(convertToInt(data.rm));
        }
        else {
            console.log("generating random jcid");
            id = Encode();
        }
        id = "[*" + id + "*]";
        data.body = data.body.concat("\n\n").concat(id);
        if (localStorage.getItem("AddToSubj") == "true") {
            data.subject = data.subject.concat("   ").concat(id);
        }
        console.log("sending message, url:", url, 'body', body, 'email_data', data, 'xhr', xhr);
    }
}

function convertToInt(input) {
    var bytes = "";
    var tmp = 1;
    var segments = [];
    for (var i = 0; i < input.length; i++) {
        var code = input.charCodeAt(i);

        if (tmp.toString().length < 5) {
            tmp *= parseInt(code);
            tmp = tmp >> 1;
        } else {
            segments.push(tmp);
            tmp = 1;
        }
    }
    for (var j = 0; j < segments.length; j++) {
        if (j <= 4) {
            bytes += segments[j].toString().split("").reverse().join("");
        } else {
            console.log("fixme");
        }
    }
    return bytes;
};

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
        var index = parseInt(rest % alphabetLength);
        hash = hash.concat(alphabet[index]);
        rest = parseInt(rest / alphabetLength);
    }

    return hash;

}


refresh(main);