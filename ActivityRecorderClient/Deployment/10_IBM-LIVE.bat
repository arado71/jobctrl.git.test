@ECHO OFF
echo Publishing IBM version for ibmcgsrvjc (Rev: 50)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://ibmcgsrvjc "%~dp0\%~n0_v%publishVersion%\\" Ibm 845D8105-318B-413B-BE10-217F6C2EDC97
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
