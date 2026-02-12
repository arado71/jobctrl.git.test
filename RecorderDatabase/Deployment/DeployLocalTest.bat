@echo on

REM Ensure environment variables are properly set
call "%~dp0\..\..\Deployment\Environment.bat"

set SOURCEDACPAC="%~1"

"%SQLPACKAGE%" /Action:Publish /sf:%SOURCEDACPAC% /tcs:"%JC_TEST_CONNECTIONSTRING%" /p:CreateNewDatabase=True

exit /B 0