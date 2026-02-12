@ECHO OFF
if "%~2"=="" (
    echo Usage %~nx0 Version PublishDir
    echo e.g.:
    echo %~nx0 2.1.55.10001 c:\voxctrl
    goto :eof
)

set ApplicationVersion=%1

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
for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  set MSBUILDCMD="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
)

if %MSBUILDCMD%=="" (
  echo MSBUILD v15 not found!
  exit /b 255
)

%MSBUILDCMD% "%~dp0\..\VoxCTRL.csproj" /target:clean;publish /property:Configuration=Release;Platform=x86;%versionpart%;PublishDir=%2;PublishProfile=%3
echo Done.
:eof
