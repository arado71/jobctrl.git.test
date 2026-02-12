(function () {
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
})()