@ECHO OFF
echo Publishing Japanase version for jobconductor.jp
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% https://jobconductor.jp/Install/ "%~dp0\%~n0_v%publishVersion%\\" Japan 033FF553-33B4-47BE-A2DF-D24993E6149C perUser 1
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
