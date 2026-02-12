@ECHO OFF
echo Publishing LIVE version for Ulyssys2 (Rev: 60)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jc360.si.net/Install/ "%~dp0\%~n0_v%publishVersion%\\" Uly c0c69988-35f9-4d56-bcb1-14bb4531fac3
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
