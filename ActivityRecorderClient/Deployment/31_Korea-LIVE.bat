@ECHO OFF
echo Publishing Korean version for kr.jobctrl.com
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://kr.jobctrl.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Korea f8638ccf-c0ac-4722-9c64-dd43125d8d58 perUser 1
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
