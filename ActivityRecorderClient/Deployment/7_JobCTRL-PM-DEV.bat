@ECHO OFF
echo Publishing DEV version for dev.jobctrl.net (Rev: 20000)
SET publishVersion=%1
if "%~1" == "" (
	SET /P publishVersion=Please enter the version: 
)
call "%~dp0\JobCTRLClientPublish.bat" JobCTRL-PM-DEV %publishVersion% http://dev.jobctrl.net/Install/ "%~dp0\%~n0_v%publishVersion%\\" Dev 2B330CBD-D4D3-4be7-9B03-F94DD2267FCF perMachine 1
exit /B %ERRORLEVEL%
@pause