@ECHO OFF
echo Publishing CpSol version for http://jobctrl.cpsol.test.com (Rev: 0)
SET publishVersion=%1
if "%~1" == "" (
	SET /P publishVersion=Please enter the version: 
)
call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jobctrl.cpsol.test.com/Install/ "%~dp0\%~n0_v%publishVersion%\\" CpSol AC8A76F9-838F-4FBD-B4D1-8A58EEDC98E1 perUser 1
exit /B %ERRORLEVEL%
@pause