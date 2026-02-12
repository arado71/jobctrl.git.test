@ECHO OFF
echo Publishing LIVE version for Dijbeszedo (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jobctrl.dbrt.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Dijbeszedo 5C196919-4D54-406B-9EA3-26ADA1E6A22F perMachine
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
