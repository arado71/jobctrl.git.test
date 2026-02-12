@ECHO OFF
echo Publishing LIVE version for CHS (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.chs.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" CHS 596105A8-BBCE-42FF-8B55-2AA0A8FB1F8A perMachine 1 NoSync NoOcr SnkAndSign
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
