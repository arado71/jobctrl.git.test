(function () {
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
    var subj = document.querySelector('div[role="main"] h2');
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
})()