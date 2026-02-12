@ECHO OFF
echo Warning: You should run this tool under the same account on which the service runs
echo Encoding configuration...
echo ---------------------------------------------------
set "BASEPATH=%~dp0"
move "%~dp0\JobCTRLService.exe.config" "%~dp0\web.config"
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis -pef "appSettings" "%BASEPATH:~0,-1%"
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis -pef "connectionStrings" "%BASEPATH:~0,-1%"
move "%~dp0\web.config" "%~dp0\JobCTRLService.exe.config"
echo ---------------------------------------------------
echo Done.
@pause