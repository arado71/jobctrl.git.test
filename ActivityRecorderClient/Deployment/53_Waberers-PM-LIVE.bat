@ECHO OFF
echo Publishing LIVE version for Waberer's on jobctrl.com
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Waberers EF6A6684-4598-4270-9AEA-081CC0577BAE perMachine 1
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
