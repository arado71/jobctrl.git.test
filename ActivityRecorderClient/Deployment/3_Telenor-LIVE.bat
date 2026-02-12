@ECHO OFF
echo Publishing LIVE version for Telenor (Rev: 14000)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://172.27.18.61/JobCTRL/Install/ "%~dp0\%~n0_v%publishVersion%\\" Default 724f6d76-f823-4347-aea2-79b92cc72aa3
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
