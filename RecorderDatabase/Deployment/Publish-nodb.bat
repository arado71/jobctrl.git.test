@echo off

REM Ensure environment variables are properly set
call "%~dp0\..\..\Deployment\Environment.bat"

set LOCALDBNAME=recorder
set SOURCEDACPAC="..\bin\Release\RecorderDatabase.dacpac"
set TARGETDACPAC="..\Create Scripts\RecorderDatabase_last.dacpac"
set PROJECTFILE="..\RecorderDatabase.sqlproj"
set REPORTFILE="DeployReport.xml"

echo [Step] Building database project...
"%MSBUILD%" %PROJECTFILE% /target:rebuild /property:Configuration=Release;Platform=AnyCPU;VisualStudioVersion=15.0
if ERRORLEVEL 1 goto error

echo [Step] Checking for changes...
"%SQLPACKAGE%" /a:deployreport /sf:%SOURCEDACPAC% /tf:%TARGETDACPAC% /tdn:%LOCALDBNAME% /op:%REPORTFILE%
if ERRORLEVEL 1 goto error

findstr /I /R /C:"\<Operation[ \>/]" %REPORTFILE% >nul
set RESULT=%ERRORLEVEL%
del %REPORTFILE%
if %RESULT% neq 0 (
	goto noChange
)

echo [Info] Change detected

REM Calculate new schema version
echo [Step] Looking up version information...
cscript -nologo GetSchemaVersion.vbs "..\dbo\Stored Procedures\GetSchemaVersion.sql" > version.tmp
if ERRORLEVEL 1 goto error
set /p version=<version.tmp
del version.tmp
echo Old Version is: %version%
set /a newVersion=%version%+1
echo New Version is: %newVersion%

echo [Step] Writing version information into sql...
echo -- Don't modify this file, it's used in the build process!> "..\dbo\Stored Procedures\GetSchemaVersion.sql"
echo CREATE PROCEDURE [dbo].[GetSchemaVersion]>> "..\dbo\Stored Procedures\GetSchemaVersion.sql"
echo AS RETURN %newVersion% >> "..\dbo\Stored Procedures\GetSchemaVersion.sql"

echo [Step] Rebuilding project...
"%MSBUILD%" %PROJECTFILE% /target:rebuild /property:Configuration=Release;Platform=AnyCPU;VisualStudioVersion=15.0

echo [Step] Creating change script...
REM Do diff
"%SQLPACKAGE%" /a:Script /sf:%SOURCEDACPAC% /tf:%TARGETDACPAC% /tdn:%LOCALDBNAME% /op:"..\Change Scripts\Change_%version%_%newVersion%.sql" /p:CommentOutSetVarDeclarations=True /p:ScriptDatabaseOptions=False
if ERRORLEVEL 1 goto error

echo [Step] Overwriting old file...
copy /B /Y /V %SOURCEDACPAC% %TARGETDACPAC%

goto success

:error
echo [Error] Error while generating change script
exit /B %ERRORLEVEL%
:noChange
echo [Info] No change detected
:success
echo [Info] Done.
exit /B 0