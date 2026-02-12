@ECHO OFF
echo Publishing Vizmuvek version of VoxCTRL (Rev: 250)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version:
)
call "%~dp0\VoxCTRLClientPublish.bat" %publishVersion% "%~dp0\%~n0_v%publishVersion%\\" Vizmu
