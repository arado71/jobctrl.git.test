@echo off
setlocal enabledelayedexpansion

set TARGETVERSION=%1
set DEPLOYSCRIPTFILE="Deploy.sql"
if "%CHANGESCRIPTSPATH%" == "" (
	set CHANGESCRIPTSPATH=..\Change Scripts
)
set SQLCMD="sqlcmd"

if "%TARGETSERVER%" == "" (
	set TARGETSERVER=.
)

if "%USER%" == "" (
	set USER=
)

if "%PASSWORD%" == "" (
	set PASSWORD=
)

if "%TARGETDBNAME%" == "" (
	set TARGETDBNAME=recorder
)

if "%USER%" == "" ( 
	set SQLCMDCONNECTIONPARAMS=-S %TARGETSERVER% -E -d %TARGETDBNAME%
) else (
	set SQLCMDCONNECTIONPARAMS=-S %TARGETSERVER% -U %USER% -P %PASSWORD% -d %TARGETDBNAME%
)
%SQLCMD% -b %SQLCMDCONNECTIONPARAMS% -Q ""
if ERRORLEVEL 1 (
	echo Connection failed.
	exit /B 1
)


set TMPFILE=%TMP%\mytempfile-%RANDOM%.tmp
%SQLCMD% -b %SQLCMDCONNECTIONPARAMS% -Q "DECLARE @res int; EXEC @res=[dbo].[GetSchemaVersion]; PRINT @res;" > %TMPFILE%
if ERRORLEVEL 1 (
	echo Retrieving current schema version from the given database failed.
	exit /B 1
)
set /P SOURCEVERSION=<%TMPFILE%
del %TMPFILE%

@echo :setvar DatabaseName %TARGETDBNAME% > %DEPLOYSCRIPTFILE%
@echo :on error exit >> %DEPLOYSCRIPTFILE%
@echo SET XACT_ABORT ON; >> %DEPLOYSCRIPTFILE%
@echo BEGIN TRANSACTION; >> %DEPLOYSCRIPTFILE%

set CURRENTVERSION=%SOURCEVERSION%
set EMPTY=1
:loop
if "%TARGETVERSION%" NEQ "" if %CURRENTVERSION% GEQ %TARGETVERSION% goto loopend
set /a COUNT=0
for %%f in ("%CHANGESCRIPTSPATH%\Change_%CURRENTVERSION%_*.sql") do (
	set /a COUNT+=1
	set CURRENTSCRIPTFILEPATH=%%~ff
	set CURRENTSCRIPTFILENAME=%%~nxf
)
if %COUNT% GTR 1 (
	echo More than one possible scripts to continue with.
	goto end
)
if %COUNT% == 0 goto loopend
for /f "tokens=2,3 delims=_." %%a in ("%CURRENTSCRIPTFILENAME%") do (
	@echo PRINT^('%CURRENTSCRIPTFILEPATH%'^) >> %DEPLOYSCRIPTFILE%
	@echo :r "%CURRENTSCRIPTFILEPATH%" >> %DEPLOYSCRIPTFILE%
	@echo DECLARE @currVer INT; EXEC @currVer = [dbo].[GetSchemaVersion]; IF @currVer ^<^> %%b RAISERROR^('Wrong version.',16,1^) >> %DEPLOYSCRIPTFILE%
	@echo GO >> %DEPLOYSCRIPTFILE%
	set CURRENTVERSION=%%b
	set EMPTY=0
)
goto loop
:loopend

@echo COMMIT TRANSACTION; >> %DEPLOYSCRIPTFILE%

if "%TARGETVERSION%" NEQ "" (
	if "%CURRENTVERSION%" NEQ "%TARGETVERSION%" (
		echo There is no possible chain of change scripts to the given target version.
		if exist %DEPLOYSCRIPTFILE% del %DEPLOYSCRIPTFILE%
		exit /B 1
	)
)
if %EMPTY% == 1 (
	echo No changes found.
	goto end
)

%SQLCMD% -b %SQLCMDCONNECTIONPARAMS% -i %DEPLOYSCRIPTFILE%
if ERRORLEVEL 1 (
	echo Run of change scripts failed.
	if exist %DEPLOYSCRIPTFILE% del %DEPLOYSCRIPTFILE%
	exit /B 1
) else (
	echo Run of change scripts finished succesfully.
)

:end
if exist %DEPLOYSCRIPTFILE% del %DEPLOYSCRIPTFILE%
exit /B 0