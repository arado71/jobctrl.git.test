@ECHO OFF
echo Publishing BME version for jobctrl.piholding.hu (Rev: 70)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.piholding.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Bme D30E2098-B56E-4913-AB06-E56DF40A5ADE
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
