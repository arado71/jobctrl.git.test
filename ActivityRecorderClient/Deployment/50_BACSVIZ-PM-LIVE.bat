@ECHO OFF
echo Publishing LIVE version for BÁCSVÍZ (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.bacsviz.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" BACSVIZ 768793BA-62A5-4440-A491-2D8D2E3653FE perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
