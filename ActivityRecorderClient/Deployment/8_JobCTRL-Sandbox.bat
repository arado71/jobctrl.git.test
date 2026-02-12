@ECHO OFF
echo Publishing Sandbox version for sandbox.jobctrl.com (Rev: 30000)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL-TEST %publishVersion% http://sandbox.jobctrl.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Sandbox AB332FA9-5B52-4B45-84B8-40CB7C56D4E9
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
