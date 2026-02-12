@ECHO OFF
echo Publishing LIVE version for Allianz (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobctrl.ahbrt.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" Allianz a420c18b-9b00-4038-923e-0203f907f9f2 perUser "" NoSync NoOcr NoSign NoObfuscar CreateZip
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
