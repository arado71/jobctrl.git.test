@ECHO OFF
echo Publishing LIVE version for NISZ (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://prod.jobctrl.nisz.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" NISZProd 33f46d40-b0d1-4877-9150-afa7ea445b85 perUser 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
