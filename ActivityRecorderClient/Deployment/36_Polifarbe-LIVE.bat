@ECHO OFF
echo Publishing LIVE version for Polifarbe (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.polifarbe.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Polifarbe a420c18b-9b00-4038-923e-0203f907f9f2 perUser
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
