@ECHO OFF
echo Publishing LIVE version for ÁSZ (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.asz.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" ASZ 77cee1a6-73b2-46a1-99a7-2242d1d0b2eb perMachineSvc 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
