@ECHO OFF
echo Publishing Pilot version of JobCTRL (Rev: 50)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://pilot.jobctrl.intra/Install/ "%~dp0\%~n0_v%publishVersion%\\" Pilot abe1805d-a532-4aac-8ada-7048b59f9089
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
