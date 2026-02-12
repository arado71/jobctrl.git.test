@ECHO OFF
echo Publishing UAT version for Telenor (Rev: 15000)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL-UAT %publishVersion% http://172.27.18.61/JobCTRL/Install/Test/ "%~dp0\%~n0_v%publishVersion%\\" Default 6fcdff0e-6561-4855-93ff-dbfd9d0d78e3
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
