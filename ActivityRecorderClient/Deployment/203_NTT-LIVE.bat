@ECHO OFF
echo Publishing NTT version for jobctrl.nttcom.co.jp (Rev: 250)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.nttcom.co.jp/Install/ "%~dp0\%~n0_v%publishVersion%\\" NTT 1DBA7A30-A91B-4434-AE2D-FC9B6FB1E4D4
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
