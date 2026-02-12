@echo off
setlocal enabledelayedexpansion
set ProjectDir=%~1
set ConfigurationName=%2
set NotDoSnk=%~3
set DoSign=%~4
for /f "delims=" %%a in ('dir /b/s "c:\Program Files (x86)\Windows Kits\signtool.exe"') do set SIGNTOOL="%%a"
pushd %ProjectDir%..\..\ActivityRecorderClient\bin\%ConfigurationName%
cd
for /f "delims=" %%a in ('dir /b "jobctrl*.exe" "jc360*.exe"') do set JC=%%a
set JC=%JC:~0,-4%
set TARGET=%CD%
mkdir %TEMP%\il 2>nul
cd %TEMP%\il
FOR /D %%f IN (
%JC%.exe
es-MX\%JC%.resources.dll
hu-HU\%JC%.resources.dll
ja-JP\%JC%.resources.dll
ko-KR\%JC%.resources.dll
pt-BR\%JC%.resources.dll
BitmapManipulation.dll 
ChromeInterop\JC.Chrome.exe 
FirefoxInterop\JC.FF.exe
bootstrap.exe 
JabApiLib.dll 
..\..\..\JcSend\bin\Release\JcSend.exe 
..\..\..\JcSend\bin\Release\hu-HU\JcSend.resources.dll 
LotusNotesSync\LotusNotesMeetingCaptureService.dll 
LotusNotesSync\JC.Meeting.exe 
OutlookSync\JC.Mail.exe 
OutlookSync\JC.Mail64.exe 
OutlookSync\JC.Meeting.exe 
OutlookSync\JC.Meeting64.exe 
OutlookSync\OutlookInteropService.dll 
OutlookSync\OutlookMeetingCaptureService.dll
TctEncoder.dll
Tesseract.dll
sapfewse.dll
saprotwr.net.dll
) DO (
For %%A in ("%%f") do (
    Set Folder=%%~dpA
    Set Name=%%~nxA
)
if not "%NotDoSnk%"=="1" (
"%ProjectDir%..\..\Utilities\ildasm" "%TARGET%\%%f" /out:!name!.il /nobar || exit /b 255
"%ProjectDir%..\..\Utilities\PubKeyTokenIns.exe" !name!.il BitmapManipulation,TctEncoder,OutlookMeetingCaptureService,OutlookInteropService,LotusNotesMeetingCaptureService,Tesseract,sapfewse,saprotwr.net,JabApiLib 462a9292f6070282
del "%TARGET%\%%f" || exit /b 255
echo !name!
set pe64=
if !name:~-6!==64.exe set pe64=/pe64 /x64
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\ilasm.exe !name!.il /res=!name!.res /!name:~-3! !pe64! /output="%TARGET%\%%f" /key="%ProjectDir%..\..\Keys\JobCTRL.snk" /QUIET || exit /b 255
)
if "%DoSign%"=="1" if not !%TOKENSIGN% == ! %SIGNTOOL% sign /a /f "%ProjectDir%..\..\Keys\TcT_CS_Public.cer" /csp "eToken Base Cryptographic Provider" /k "[{{Jobctrl789$}}]=Sectigo_20231010110831" /fd sha256 /tr http://timestamp.comodoca.com /td sha256 /as "%TARGET%\%%f" || exit /b 255
if "%DoSign%"=="1" if !%TOKENSIGN% == ! %SIGNTOOL% sign /a /f "%ProjectDir%..\..\Keys\tct_self.pfx" /p jobctrl789 /fd sha256 /tr http://timestamp.comodoca.com /td sha256 /as "%TARGET%\%%f" || exit /b 255
)
del /s/q %TEMP%\il
popd
