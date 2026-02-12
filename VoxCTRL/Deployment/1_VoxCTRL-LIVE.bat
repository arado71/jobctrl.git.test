@ECHO OFF
echo Publishing LIVE version of VoxCTRL (Rev: 0)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version:
)
call "%~dp0\VoxCTRLClientPublish.bat" %publishVersion% "%~dp0\%~n0_v%publishVersion%\\" Default
