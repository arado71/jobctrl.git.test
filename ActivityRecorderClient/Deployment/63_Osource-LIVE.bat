@ECHO OFF
echo Publishing LIVE version for Osource Global (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.osourceglobal.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" Osource d657476f-cebc-4d8e-b408-72742cf6fa85 perUser 1 NoSync NoOcr SnkAndSign NoObfuscar
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
