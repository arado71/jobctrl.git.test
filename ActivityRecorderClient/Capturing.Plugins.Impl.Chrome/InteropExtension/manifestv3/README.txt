Extension build és publish folyamat:
1.) nodejs telepítése
2.) parancssor ablak ActivityRecorderClient\Capturing.Plugins.Impl.Chrome\InteropExtension\manifestv3\ mappában
3.) "npm install"
4.) "npm run build"
5.) dist mappába létrehozza a bővítmény fájljait, Chrome-ba be lehet tölteni, lehet tesztelni

Közzététel:
1.) A dist mappa tartalmát tömöríteni kell egy zip fájlba
2.) https://chrome.google.com/webstore/devconsole/
3.) Publisher kiválasztása
4.) Extension kiválasztása
5.) Package -> Upload new package -> az első pontban tömörített zip-et kell feltölteni
6.) Érdemes jóváhagyni publish nélkül Upload for review -> "Publish after successful review" checkbox kivétele
7.) Sikeres review után 30 nap van közzétenni
