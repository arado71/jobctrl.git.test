@echo OFF
echo Self update BETA
REM  === Configuration ===
SET "FILESOURCE=\\jcapp2\rls\JcService\DEV"
SET SERVICENAME=JobCTRL Server

REM --- Database ---
set TARGETSERVER=tctsql2012\ps
set TARGETDBNAME=recorder_test
REM Leave empty for integrated authentication
set USER=sa
set PASSWORD=Password01

REM --- Defaults ---
SET "TARGETDIR=%~dp0"
SET SOURCENAME=JcService.zip

REM  === Logic ===
SET STARTSERVICE=true
sc query | findstr /I /C:"SERVICE_NAME: %SERVICENAME%"
if ERRORLEVEL 1 (
	echo [WARN] Service not found, service won't be started at the end
	set STARTSERVICE=false
)
else
(
	for /F "tokens=3 delims=: " %%H in ('sc query "MyServiceName" ^| findstr "        STATE"') do (
  if /I "%%H" NEQ "RUNNING" (
   REM Put your code you want to execute here
   REM For example, the following line
   echo [WARN] Service not running, service won't be started at the end
   set STARTSERVICE=false
  )
)

net stop "%SERVICENAME%"
if ERRORLEVEL 1 (
	echo [WARN] Error while stopping service, continuing anyway...
)

pushd "%FILESOURCE%"
xcopy "%SOURCENAME%" "%TARGETDIR%" -y
"%~dp0\7za.exe" x "JcService.zip" -o"%TARGETDIR%" -y
popd
if ERRORLEVEL 1 (
	echo [ERROR] Error while copying files
	goto restartService
)

pushd "%TARGETDIR%"
set "CHANGESCRIPTSPATH=.\Change Scripts"
call "upgrade.bat"
if ERRORLEVEL 1 (
	echo [WARN] Database update failed, restarting service anyway...
)
popd

:restartService
if "%STARTSERVICE%" == "true" (
	echo [INFO] Starting service...
	net start "%SERVICENAME%"
	if ERRORLEVEL 1 (
		echo [ERROR] Error while starting service
		exit /B 1
	)
)
else
(
)


echo Update completed successfully
exit /B 0