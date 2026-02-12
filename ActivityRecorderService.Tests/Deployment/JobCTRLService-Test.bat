@ECHO ON

if not "%~1" == "" (
	set "JC_TEST_CONNECTIONSTRING=%~1"
)

REM Ensure environment variables are properly set
call ..\..\Environment.bat

if "%JC_TEST_SKIPDATABASEDEPLOY%" == "" (
	set "%JC_TEST_SKIPDATABASEDEPLOY%=true"
)

if "%JC_TEST_OVERWRITEDATABASE%" == "" (
	set "%JC_TEST_OVERWRITEDATABASE%=false"
)

if "%JC_TEST_CONNECTIONSTRING%" == "" (
	set /p testConn=Enter connection string used for testing:%=%
	if not "%testConn%" == "" (
		set "JC_TEST_CONNECTIONSTRING=%testConn%"
	)
)



echo ==Building and running unit tests==
"%MSBUILD%" "%~dp0\..\ActivityRecorderService.Tests.csproj" /target:rebuild;test /p:Configuration=Debug /p:Platform=AnyCPU /p:VisualStudioVersion=12.0
REM c:\Z\ProjectsInstall\ActivityRecorder\ClickOnce\
exit /B %ERRORLEVEL%
@pause