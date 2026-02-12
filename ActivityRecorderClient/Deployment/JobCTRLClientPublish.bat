@ECHO OFF
if "%~6"=="" (
    echo Usage %~nx0 ProductName Version InstallUrl PublishDir PublishProfile UpgradeCode [MsiInstallScope] [EnableWelcome] [MsProjSyncFeature] [OcrPlugin] [SnkAndSign] [GoObfuscar]
    echo e.g.:
    echo "%~nx0 (%%1)JobCTRL-UAT (%%2)2.1.55.10001 (%%3)http://jobctrl.com/Install/UAT/ (%%4)c:\Z\ProjectsInstall\ActivityRecorder\ClickOnceUAT\ "
	echo "(%%5)Default (%%6)aaca0011-c83e-40a1-9acb-47d156610025 (%%7)perMachine (%%8)"" (%%9)1 (%%10)GoOcr (%%11)NoSign (%%12)GoObfuscar (%%13)NoZip"
	echo "(%7)MsiInstallScope := {perMachine,perUser,perMachineSvc}"
	echo "(%8)EnableWelcome = "" (placeholder if additional parameters coming)"
	echo "(%9)ProjectSync := {Sync means yes do sync (practicaly 1)}"
	echo "(%10)OcrPlugin := {GoOcr means yes do OCR}"
	echo "(%11)AsmSnkAndSign := {SnkAndSign means to add strong name and sign assembly}"
	echo "(%12)GoObfuscation := {GoObfuscar means to obfuscate assemblies}"
	echo "(%13)ZipPackage := {CreateZip means to create zip package}"
	
    goto :eof
)

set ProductName=%1
set ClientAssemblyName=%1
set ApplicationVersion=%2
set InstallUrl=%3
set UpdateUrl=%3
set PublishDir=%~4
set PublishProfile=%5
set UpgradeCode=%6
set MsiInstallScope=%~7
set IncludeService=
if "%MsiInstallScope%" == "perMachineSvc" (
	set MsiInstallScope=perMachine
	set IncludeService=1
)
if "%MsiInstallScope%" NEQ "perMachine" (
	set MsiInstallScope=perUser
)
if "%8" == "1" (
set EnableWelcome=1
) else (
set EnableWelcome=
)
if "%9" == "Sync" (
set ProjectSync=1
) else (
set ProjectSync=
)
shift
if "%9" == "GoOcr" (
set OcrPlugin=1
) else (
set OcrPlugin=
)
shift
if "%9" == "SnkAndSign" (
set AsmSnkAndSign=1
) else (
set AsmSnkAndSign=
)
shift /1
if "%9" == "GoObfuscar" (
set GoObfuscation=1
) else (
set GoObfuscation=
)
shift /1
if "%9" == "CreateZip" (
set ZipPackage=1
) else (
set ZipPackage=
)
rem goto :eof
REM this version check is not working for all cases... but msbuild will check it properly
set "xResult=valid"
for /f "tokens=1-5 delims=." %%A in ("%ApplicationVersion%") do (
	if NOT "%%A" == "" ( 
		if "%%B%%C%%D" == "" goto :revonly 
	)
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
    echo Version format %ApplicationVersion% is invalid
    goto :eof
)
set versionpart=ApplicationVersion=%ApplicationVersion%
goto :dobuild

:revonly
set versionpart=ApplicationRevision=%ApplicationVersion%

:dobuild
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" set MSBUILDCMD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\msbuild.exe" set MSBUILDCMD=%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\msbuild.exe
if "%MSBUILDCMD%"=="" (
  echo MSBUILD v17 not found!
  exit /b 255
)
"%MSBUILDCMD%" "%~dp0\..\ActivityRecorderClient.csproj" /target:clean;publish "/property:Configuration=Release;Platform=MixedCo;ProductName=%ProductName%;ClientAssemblyName=%ProductName%;%versionpart%;InstallUrl=%InstallUrl%;UpdateUrl=%InstallUrl%;PublishDir=%PublishDir%;PublishProfile=%PublishProfile%;UpgradeCode=%UpgradeCode%;MsiInstallScope=%MsiInstallScope%;IncludeService=%IncludeService%;ProjectSync=%ProjectSync%;OcrPlugin=%OcrPlugin%;AsmSnkAndSign=%AsmSnkAndSign%;GoObfuscation=%GoObfuscation%;ZipPackage=%ZipPackage%;VisualStudioVersion=15.0;SolutionDir=%~dp0\..\..\;AllowedReferenceRelatedFileExtensions=none"
exit /B %ERRORLEVEL%
echo Done.
:eof
