@ECHO OFF
echo Publishing LIVE version for Semilab (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.semilab.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Semilab DD1FD72A-D80D-4308-AFB5-F052DECEA965 perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
