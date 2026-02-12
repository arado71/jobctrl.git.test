@ECHO OFF
echo Publishing Brasil LIVE version for br.jobctrl.com (Rev: 200)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://br.jobctrl.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Brasil 3b7c60b8-eee2-4717-87f0-c3d9cb11f50d perUser 1 1 GoOcr
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
