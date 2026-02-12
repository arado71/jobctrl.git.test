REM Use this tool to package extension into a crx file if any of the containing files changed in InteropExtension folder

"c:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --pack-extension="%~dp0\InteropExtension" --pack-extension-key="%~dp0\chromeinterop@jobctrl.com.pem"
IF EXIST "%~dp0\chromeinterop@jobctrl.com.crx" del "%~dp0\chromeinterop@jobctrl.com.crx"
rename "%~dp0\InteropExtension.crx" "chromeinterop@jobctrl.com.crx"
move "chromeinterop@jobctrl.com.crx" "..\ChromeInterop\chromeinterop@jobctrl.com.crx"