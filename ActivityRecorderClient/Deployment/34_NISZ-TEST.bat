@ECHO OFF
echo Publishing TEST version for NISZ (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://test.jobctrl.nisz.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" NISZTest 20aaa4fc-d7f9-4f42-a7a1-99f794aa8cd2 perUser 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
