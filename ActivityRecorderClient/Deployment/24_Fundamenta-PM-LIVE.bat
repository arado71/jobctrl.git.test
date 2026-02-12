@ECHO OFF
echo Publishing Fundamenta version for jobctrl.fundamenta.local
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.fundamenta.local/Install/ "%~dp0\%~n0_v%publishVersion%\\" Fundamenta 74410a0e-a980-489e-a04d-5ae12a1c5aa7 perMachine
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
