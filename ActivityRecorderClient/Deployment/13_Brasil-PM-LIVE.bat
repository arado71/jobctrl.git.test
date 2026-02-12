@ECHO OFF
echo Publishing Brasil per Machine LIVE version for br.jobctrl.com (Rev: 200)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://br.jobctrl.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Brasil 880e0fea-399c-4684-a2fd-407c63b093ff perMachine 1 1
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
