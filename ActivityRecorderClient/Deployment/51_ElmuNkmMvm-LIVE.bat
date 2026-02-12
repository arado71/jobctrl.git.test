@ECHO OFF
echo Publishing LIVE version for ELMU/NKM/MVM (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jc360.mvmee.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" ElmuNkmMvm 939117d8-5643-4d54-954a-7f27efe0451c perUser
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
