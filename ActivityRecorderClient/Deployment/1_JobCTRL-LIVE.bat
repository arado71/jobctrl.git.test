@ECHO OFF
echo Publishing LIVE version for jobctrl.com (Rev: 0)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Default d657476f-cebc-4d8e-b408-72742cf6fa85 perUser 1
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
