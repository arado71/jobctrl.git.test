@ECHO OFF
echo Publishing LIVE version for MÁK (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.tcs.allamkincstar.gov.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Mak 890e04bc-7b1a-465f-bdce-e6e4133adc83 perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
