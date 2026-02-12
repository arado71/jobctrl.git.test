 @ECHO OFF
 setlocal enabledelayedexpansion
if "%~1"=="" (
    echo Usage %~nx0 Version
    echo e.g.:
    echo %~nx0 2.1.55.10001 
    goto :eof
)
if "%~2" == "GoObfuscar" (
set GoObfuscation=1
) else (
set GoObfuscation=
)

set "xResult=valid"
for /f "tokens=1-5 delims=." %%A in ("%1") do (
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
    echo Version format %1% is invalid
    goto :eof
)
set version=%1
goto :dobuild

:revonly
for /f delims^=^"^ tokens^=2 %%a in ('find "AssemblyVersion" ..\Properties\AssemblyInfo.cs') do set asmver=%%a

for /f "tokens=1-5 delims=." %%A in ("%asmver%") do (
    if not "%%D" == "" set asmver=%%A.%%B.%%C
)

set version=%asmver%.%1

:dobuild

if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" set MSBUILDCMD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\msbuild.exe" set MSBUILDCMD=%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\msbuild.exe
if "%MSBUILDCMD%"=="" (
  echo MSBUILD v17 not found!
  exit /b 255
)
for /f "delims=" %%a in ('dir /b/s "c:\Program Files (x86)\Windows Kits\signtool.exe"') do set SIGNTOOL="%%a"
"%MSBUILDCMD%" "%~dp0\..\ActivityRecorderService.csproj" /target:rebuild /property:Configuration=Release;Platform=AnyCPU;ApplicationVersion=%version%;GoObfuscation=%GoObfuscation%;VisualStudioVersion=15.0 /p:SignAssembly=true /p:AssemblyOriginatorKeyFile="%CD%\..\..\Keys\JobCTRL.snk" || exit /b 255
"%MSBUILDCMD%" "%~dp0\..\..\ActivityRecorderServiceWindowsHost\ActivityRecorderServiceWindowsHost.csproj" /target:rebuild /property:Configuration=Release;Platform=AnyCPU;ApplicationVersion=%version%;GoObfuscation=%GoObfuscation%;VisualStudioVersion=15.0 /p:SignAssembly=true /p:AssemblyOriginatorKeyFile="%CD%\..\..\Keys\JobCTRL.snk" || exit /b 255
set TARGET=%~dp0\..\bin\Release
copy "%~dp0\..\..\ActivityRecorderServiceWindowsHost\bin\Release\JobCTRLService.exe" "%TARGET%" || exit /b 255
copy "%~dp0\..\..\ActivityRecorderServiceWindowsHost\bin\Release\JobCTRLService*install.bat" "%TARGET%" || exit /b 255

mkdir "%TEMP%\il" 2>nul
FOR  /D %%f IN (
	Tesseract.dll
	Ocr.dll
	ActivityRecorderService.dll
) DO (
For %%A in ("%%f") do (
    Set Folder=%%~dpA
    Set Name=%%~nxA
)
%ProjectDir%..\..\Utilities\ildasm "%TARGET%\!name!" /out:"%TEMP%\il\!name!.il" /nobar || exit /b 255
%ProjectDir%..\..\Utilities\PubKeyTokenIns.exe "%TEMP%\il\!name!.il" Tesseract 462a9292f6070282
del "%TARGET%\!name!" || exit /b 255
echo !name!
set pe64=
if !name:~-6!==64.exe set pe64=/pe64 /x64
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\ilasm.exe "%TEMP%\il\!name!.il" /res="%TEMP%\il\!name!.res" /!name:~-3! !pe64! /output="%TARGET%\!name!" /key=%ProjectDir%..\..\Keys\JobCTRL.snk /QUIET || exit /b 255
del /s/q "%TEMP%\il"
)

FOR /R "%TARGET%" %%f IN (
	*.dll
	JobCTRLService.ex?
) DO (
if not !%TOKENSIGN% == ! %SIGNTOOL% sign /a /f "%ProjectDir%..\..\Keys\TcT_CS_Public.cer" /csp "eToken Base Cryptographic Provider" /k "[{{Jobctrl789$}}]=Sectigo_20231010110831" /fd sha256 /tr http://timestamp.comodoca.com /td sha256 /as "%%f" || exit /b 255
if !%TOKENSIGN% == ! %SIGNTOOL% sign /a /f "%ProjectDir%..\..\Keys\tct_self.pfx" /p jobctrl789 /fd sha256 /tr http://timestamp.comodoca.com /td sha256 /as "%%f" || exit /b 255
)
echo ##teamcity[buildNumber '%version%']
exit /B %ERRORLEVEL%
echo Done.
:eof