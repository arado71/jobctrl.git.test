@ECHO OFF
echo Publishing TEST version for MÁV (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360-Test %publishVersion% https://jc360test.mav.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" MAVTest 1b137cd2-e017-42b4-9ab9-4ae66472f0ab perUser 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
