@ECHO OFF
echo Publishing JC360 Lite for jobctrl.com (Rev: 10000)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360-HomeOffice %publishVersion% http://jobctrl.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" JC360Lite aaca0011-c83e-40a1-9acb-47d156611136 perUser "" NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
REM c:\Z\ProjectsInstall\ActivityRecorder\ClickOnceUAT\
