@ECHO OFF
echo Publishing Delta version for jobctrl.delta.hu (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.delta.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Delta 237EB7E4-DEC1-48CC-8618-CA03236B7CBA perMachine
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
