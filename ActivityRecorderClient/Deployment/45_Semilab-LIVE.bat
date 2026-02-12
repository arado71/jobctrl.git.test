@ECHO OFF
echo Publishing LIVE version for Semilab (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.semilab.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Semilab D4E25BE9-0446-45FB-995E-59B60CB9F00C perUser 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
