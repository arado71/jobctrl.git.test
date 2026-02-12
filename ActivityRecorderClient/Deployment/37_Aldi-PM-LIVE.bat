@ECHO OFF
echo Publishing LIVE version for Aldi (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.test.aldi-sued.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Aldi 41aaa4fc-d7f9-4f42-a7a1-99f794cc7ce8 perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
