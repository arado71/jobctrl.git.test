@ECHO OFF
echo Publishing LIVE version for Civil (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.civil.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Civil 01a9e271-8573-42d7-ab7f-eb0f782db2dc perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
