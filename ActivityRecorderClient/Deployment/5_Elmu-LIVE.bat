@ECHO OFF
echo Publishing LIVE version for ELMU (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.elmu.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Elmu 939117d8-5643-4d54-954a-7f27efe0451c perUser
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
