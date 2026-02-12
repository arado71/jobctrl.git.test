@ECHO OFF
echo Publishing LIVE version for Takinfo (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jobctrl.takarekinfo.local/Install/ "%~dp0\%~n0_v%publishVersion%\\" Takinfo bb407b4d-9c19-40cc-b987-3fbe1d06f125 perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
