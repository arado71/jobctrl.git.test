@ECHO OFF
echo Publishing T-Systems version for jobctrl.t-systems.hu
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.t-systems.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" tsystems 0d9e3cfb-b475-41ea-83e1-d66b0191577c perMachine
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
