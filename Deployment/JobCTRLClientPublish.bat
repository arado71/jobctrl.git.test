@ECHO OFF
if "%~6"=="" (
    echo Usage %~nx0 ProductName Version InstallUrl PublishDir PublishProfile UpgradeCode [MsiInstallScope] [EnableWelcome] [MsProjSyncFeature] [OcrPlugin]
    echo e.g.:
    echo %~nx0 JobCTRL-UAT 2.1.55.10001 http://jobctrl.com/Install/UAT/ c:\Z\ProjectsInstall\ActivityRecorder\ClickOnceUAT\ Default aaca0011-c83e-40a1-9acb-47d156610025 perMachine "" 1
    goto :eof
)

set MsiInstallScope=%~7
set IncludeService=

if "%MsiInstallScope%" == "perMachineSvc" (
	set MsiInstallScope=perMachine
	set IncludeService=1
)
if "%MsiInstallScope%" NEQ "perMachine" (
	set MsiInstallScope=perUser
)

set EnableWelcome=%8
set ProjectSync=%9
set OcrPlugin=%10

REM this version check is not working for all cases... but msbuild will check it properly
set "xResult=valid"
for /f "tokens=1-5 delims=." %%A in ("%2") do (
    if "%%A" == "" set "xResult=invalid"
    if "%%B" == "" set "xResult=invalid"
    if "%%C" == "" set "xResult=invalid"
    if "%%D" == "" set "xResult=invalid"
    if not "%%E" == "" set "xResult=invalid"
    for /f "tokens=1 delims=1234567890" %%n in ("%%A") do set "xResult=invalid"
    for /f "tokens=1 delims=1234567890" %%n in ("%%B") do set "xResult=invalid"
    for /f "tokens=1 delims=1234567890" %%n in ("%%C") do set "xResult=invalid"
    for /f "tokens=1 delims=1234567890" %%n in ("%%D") do set "xResult=invalid"
)
if "%xResult%" == "invalid" (
    echo Version format %2% is invalid
    goto :eof
)

"%ProgramFiles(x86)%\MSBuild\12.0\Bin\msbuild.exe" "%~dp0\..\ActivityRecorderClient.csproj" /target:clean;publish /property:Configuration=Release;Platform=MixedCo;ProductName=%1;ClientAssemblyName=%1;ApplicationVersion=%2;InstallUrl=%3;UpdateUrl=%3;PublishDir=%4;PublishProfile=%5;UpgradeCode=%6;MsiInstallScope=%MsiInstallScope%;IncludeService=%IncludeService%;ProjectSync=%ProjectSync%;OcrPlugin=%OcrPlugin%;VisualStudioVersion=12.0
exit /B %ERRORLEVEL%
echo Done.
:eof
