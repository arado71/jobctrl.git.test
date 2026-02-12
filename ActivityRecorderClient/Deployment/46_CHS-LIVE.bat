@ECHO OFF
echo Publishing LIVE version for CHS (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.chs.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" CHS F106ABDA-F53D-4C77-A1CB-A3157B41AA5F perUser 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
