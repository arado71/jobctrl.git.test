@ECHO OFF
echo Publishing LIVE version for Bekomold (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jc360.bekomold.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Bekomold 01a9e271-8573-42d7-ab7f-eb0f782db292 perUser 1 NoSync NoOcr
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
