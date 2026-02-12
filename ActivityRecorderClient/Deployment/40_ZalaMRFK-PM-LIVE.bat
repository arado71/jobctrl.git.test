@ECHO OFF
echo Publishing LIVE version for ZalaMRFK (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.zala.police.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" ZalaMRFK 4e63973f-2564-4eda-8481-0d3825b7f410 perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
