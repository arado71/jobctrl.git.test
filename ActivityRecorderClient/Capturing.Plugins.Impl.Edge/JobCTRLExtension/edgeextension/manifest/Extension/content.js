// is there a script tag in the <head> whose src contains jquery file
function hasJquery(){
	var regx = /\/jquery(-\d+\.\d+\.\d+)?\.(slim.)?(min.)?js/;
	var scripts = document.head.querySelectorAll("script");
	for(var i =0; i<scripts.length; i++ ){
		if (regx.test(scripts[i].src)){
			return true
		}
	}
	return false;
}

function loadFiles(){
	if(!hasJquery()){
		var j = document.createElement('script');
		j.src = chrome.extension.getURL('jquery-1.10.2.min.js');
		(document.head || document.documentElement).appendChild(j);
	}
	
	var g = document.createElement('script');
	g.src = chrome.extension.getURL('gmail.js');
	(document.head || document.documentElement).appendChild(g);

	var h = document.createElement('script');
	h.src = chrome.extension.getURL('main.js');
	(document.head || document.documentElement).appendChild(h);
}

window.addEventListener("load", loadFiles);