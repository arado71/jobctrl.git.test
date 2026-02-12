@ECHO OFF
echo Publishing Japanase version for jobconductor.jp
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobconductor.jp/Install/ "%~dp0\%~n0_v%publishVersion%\\" JapanPM D8357E4A-CCB0-42BA-97F0-232CB6852CAD perMachine 1
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
