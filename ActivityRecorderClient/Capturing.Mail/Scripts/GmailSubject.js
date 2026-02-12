(function () {
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
    var subj = document.querySelector('div[role="main"] table h2');
    if (subj) return subj.textContent || subj.innerText;
    return null;
})()