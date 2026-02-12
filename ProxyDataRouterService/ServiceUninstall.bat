@ECHO OFF
echo Uninstalling ProxyDataRouter Service...
echo ---------------------------------------------------
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil /u ProxyDataRouterService.exe
echo ---------------------------------------------------
echo Done.
@pause