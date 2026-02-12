@ECHO OFF
echo Publishing Frigelar version for jobctrl.frigelar.local (Rev: 250)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.frigelar.local/Install/ "%~dp0\%~n0_v%publishVersion%\\" Frigelar da7e5a2a-639e-467d-8cef-fb21b4b8d3e5
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
