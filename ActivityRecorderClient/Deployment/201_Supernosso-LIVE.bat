@ECHO OFF
echo Publishing Supernosso version for jobctrl.supernosso.intra (Rev: 250)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.supernosso.intra/Install/ "%~dp0\%~n0_v%publishVersion%\\" Supernosso 7f3652d5-22e7-400b-ac9a-c073b18ccde2 perUser
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
