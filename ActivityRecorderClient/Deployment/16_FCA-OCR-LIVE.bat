@ECHO OFF
echo Publishing LIVE version for FCA (Rev: 60)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL     %publishVersion% http://jobctrl.fcaservices.com.br/Install/ "%~dp0\%~n0_v%publishVersion%\\" FCA     BABC3630-F7E0-4037-BB2D-985E50649BE8 perUser "" NoSync GoOCR
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
