@echo off

REM Ensure environment variables are properly set
call "%~dp0\..\..\Deployment\Environment.bat"

set SQLCMD="sqlcmd"
set TEMPDBNAME=recorder_temp
set CREATESCRIPTFILE="..\Create Scripts\GenerateAll.sql"
set LOCALDBNAME=recorder
set SOURCEDACPAC="..\bin\Release\RecorderDatabase.dacpac"
set PROJECTFILE="..\RecorderDatabase.sqlproj"
set REPORTFILE="DeployReport.xml"
set GETSCHEMAVERSIONSPFILE="..\dbo\Stored Procedures\GetSchemaVersion.sql"

echo Building database project.
"%MSBUILD%" %PROJECTFILE% /target:build /property:Configuration=Release;Platform=AnyCPU;VisualStudioVersion=15.0
if ERRORLEVEL 1 goto error

echo Creating temporary database from previous GenerateAll.sql
%SQLCMD% -b -Q "IF EXISTS(SELECT * FROM sys.databases WHERE name='%TEMPDBNAME%') BEGIN ALTER DATABASE %TEMPDBNAME% SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE %TEMPDBNAME%; END CREATE DATABASE %TEMPDBNAME% COLLATE SQL_Latin1_General_CP1_CI_AS;"
if ERRORLEVEL 1 goto error
%SQLCMD% -b -d %TEMPDBNAME% -i %CREATESCRIPTFILE%
if ERRORLEVEL 1 goto error

REM Calculate new schema version
set TMPFILE=%TMP%\mytempfile-%RANDOM%.tmp
%SQLCMD% -b -d %TEMPDBNAME% -Q "DECLARE @res int; EXEC @res=[dbo].[GetSchemaVersion]; PRINT @res;" > %TMPFILE%
if ERRORLEVEL 1 goto error
set /P CURRSCHEMAVERSION=<%TMPFILE%
set /A NEWSCHEMAVERSION=%CURRSCHEMAVERSION% + 1
set CHANGESCRIPTFILE="..\Change Scripts\Change_%CURRSCHEMAVERSION%_%NEWSCHEMAVERSION%.sql"
del %TMPFILE%

echo Create report about changes
"%SQLPACKAGE%" /a:deployreport /sf:%SOURCEDACPAC% /tcs:"Data Source=.;Initial Catalog=%TEMPDBNAME%;Integrated Security=True" /op:%REPORTFILE%
if ERRORLEVEL 1 goto error

findstr /I /R /C:"\<Operation[ \>/]" %REPORTFILE% >nul
set RESULT=%ERRORLEVEL%
del %REPORTFILE%

if %RESULT% neq 0 (
	echo Source and target schemas are equal.
) else (
	echo Changing schema version form %CURRSCHEMAVERSION% to %NEWSCHEMAVERSION% in project and local db.
	@echo CREATE PROCEDURE [dbo].[GetSchemaVersion] AS RETURN %NEWSCHEMAVERSION% > %GETSCHEMAVERSIONSPFILE%
	REM Change schema version in local db
	%SQLCMD% -d %LOCALDBNAME% -Q "ALTER PROCEDURE [dbo].[GetSchemaVersion] AS RETURN %NEWSCHEMAVERSION%"
	
	echo Generating change script
	"%MSBUILD%" %PROJECTFILE% /target:build /property:Configuration=Release;Platform=AnyCPU;VisualStudioVersion=15.0
	if ERRORLEVEL 1 goto error
	"%SQLPACKAGE%" /a:script /sf:%SOURCEDACPAC% /tcs:"Data Source=.;Initial Catalog=%TEMPDBNAME%;Integrated Security=True" /op:%CHANGESCRIPTFILE% /p:CommentOutSetVarDeclarations=True /p:ScriptDatabaseOptions=False
	if ERRORLEVEL 1 goto error
	REM %SQLPACKAGE% /a:publish /sf:%SOURCEDACPAC% /tcs:"Data Source=.;Initial Catalog=%TEMPDBNAME%;Integrated Security=True"
	%SQLCMD% -b -d %TEMPDBNAME% -i %CHANGESCRIPTFILE% -v DatabaseName=%TEMPDBNAME%
	if ERRORLEVEL 1 goto error
	
	echo Generating new create script
	"%SQLPUBWIZ%" script -C "Data Source=.;Initial Catalog=%TEMPDBNAME%;Integrated Security=True" %CREATESCRIPTFILE% -f -schemaonly -targetserver "2008"
	if ERRORLEVEL 1 goto error
	SqlScriptCleaner.exe %CREATESCRIPTFILE%
)

%SQLCMD% -Q "IF EXISTS(SELECT * FROM sys.databases WHERE name='%TEMPDBNAME%') BEGIN ALTER DATABASE %TEMPDBNAME% SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE %TEMPDBNAME%; END"

echo Done.
goto end
:error
echo Error generating change script.
:end


