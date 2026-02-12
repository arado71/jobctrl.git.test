@ECHO OFF
echo Publishing Raiffeisen version for jobctrl.raiffeisen.hu (Rev: 50)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.raiffeisen.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Raiffeisen b269ab47-7672-4cc1-a162-dcc97e2271fe perMachine
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
