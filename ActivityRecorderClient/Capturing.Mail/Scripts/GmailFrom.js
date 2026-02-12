(function () {
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
})()