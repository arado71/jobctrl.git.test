@ECHO OFF
echo Publishing Inovabra version for jobctrl.inovabra (Rev: 250)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.inovabra/Install/ "%~dp0\%~n0_v%publishVersion%\\" Inovabra da7e5a2a-639e-467d-8cef-fb21b4b8d3e5 perUser 1
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
