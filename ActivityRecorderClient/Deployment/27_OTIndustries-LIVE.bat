@ECHO OFF
echo Publishing OTIndustries version for jobctrl.olajterv.hu
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.olajterv.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" OTIndustries 8fefbd46-75c0-4b48-a7a4-725fcc2ddca3 perUser
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
