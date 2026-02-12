@ECHO OFF
echo Publishing Delta version for jobctrl.delta.hu
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.delta.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Delta 9E783EEC-CD08-4ADF-A131-0FD1203A6E7E perUser
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
