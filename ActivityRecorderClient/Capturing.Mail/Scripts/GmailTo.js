(function(){
	var myTo = new Set();
	var res2 = /\?ui=2&view=btop/i.exec(document.location.search);
	if (res2 != null && res2.length > 0) {
		[].forEach.call(
			  document.querySelectorAll('div[role="main"] td[class="eV"] span[email]'),
			  function(el){
				myTo.add(el.attributes["email"].nodeValue);
			  }
			);
	} else {
	var res = /\?compose=([0-Z]+)/i.exec(document.location.hash);
		if (res != null && res.length > 1) {		
            document.querySelectorAll('div[class="nH nn"] span[email]').forEach(
			  function(el){
				myTo.add(el.attributes["email"].nodeValue);
			  }
			);
		}else {
			var e = document.querySelector('div[class="amn"]');
			if (e == null) {
				[].forEach.call(
				  document.querySelectorAll('div[role="main"] td[class="Iy"] span[email]'),
				  function(el){
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
					function(el){
						myTo.add(el.attributes['email'].nodeValue);
					}
				);
			}
		}
	}
	var arr = new Array();
	myTo.forEach(function(e){arr.push(e);});
	return arr.join(';');
})()