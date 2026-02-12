@ECHO OFF
echo Publishing Vizmuvek Timesheet version for jobctrl.vizmuvek.hu (Rev: 50)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.vizmuvek.hu/Install/timesheet/ "%~dp0\%~n0_v%publishVersion%\\" VizmuTS a13ac509-ed2e-47ec-9e81-4225673d6367 perMachine
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
