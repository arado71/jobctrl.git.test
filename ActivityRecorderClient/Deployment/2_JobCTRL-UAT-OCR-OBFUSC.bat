@ECHO OFF
echo Publishing UAT version for jobctrl.com (Rev: 10000)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL-UAT %publishVersion% http://jobctrl.com/Install/UAT/ "%~dp0\%~n0_v%publishVersion%\\" Default aaca0011-c83e-40a1-9acb-47d156610025 perUser 1 NoSync GoOcr NoSign GoObfuscar
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
REM c:\Z\ProjectsInstall\ActivityRecorder\ClickOnceUAT\
