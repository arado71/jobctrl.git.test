@ECHO OFF
echo Publishing LIVE version for Dijbeszedo (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jobctrl.dbrt.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Dijbeszedo 44EA2DC4-D42A-4E58-BE82-E6EE9AD4357A perUser
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
