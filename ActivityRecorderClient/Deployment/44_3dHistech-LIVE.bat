@ECHO OFF
echo Publishing LIVE version for 3dhistech (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.3dhistech.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" 3dHistech 01a9e271-8573-42d7-ab7f-eb0f782db292 perUser 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
