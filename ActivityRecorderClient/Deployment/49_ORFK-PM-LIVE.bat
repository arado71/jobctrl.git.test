@ECHO OFF
echo Publishing LIVE version for ORFK (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.police.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" ORFK 471E8A6E-9F99-4958-9F6A-BAD0CD104CEF perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
