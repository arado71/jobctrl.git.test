@ECHO OFF
echo Publishing LIVE version for E.On on jobctrl.com (Rev: 50)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Eon 78fb14c5-0405-4f2b-a14b-1e938c117869 perUser 1
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
