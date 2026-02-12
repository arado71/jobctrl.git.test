@ECHO OFF
echo Publishing LIVE version for MÁV (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.mav.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" MAV 3474004a-72b0-454c-be76-054b1c3db09f perUser 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
