@ECHO OFF
echo Publishing Telekom version for jobctrl.telekom.intra (Rev: 50)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.telekom.intra/Install/ "%~dp0\%~n0_v%publishVersion%\\" Telekom 65c44286-1e5d-4e69-a3f1-d4d90d81dcb8 perMachineSvc
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
