@ECHO OFF
echo Publishing HungaroControl version for jobctrl.hc.hu
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.hc.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" HungaroControl 0e4d51a3-749b-49da-9f8c-144f90b61808 perUser
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
