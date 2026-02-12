@ECHO OFF
echo Publishing LIVE version for Bradesco (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.bradesco.com.br/Install/ "%~dp0\%~n0_v%publishVersion%\\" Bradesco e789226e-5c50-42ee-bd4e-e30b2facf6b9 perMachine 0 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
