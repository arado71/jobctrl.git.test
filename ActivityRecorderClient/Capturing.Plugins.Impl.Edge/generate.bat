@echo off
for /f "delims=" %%a in ('dir /b/s "c:\Program Files (x86)\Windows Kits\signtool.exe"') do set SIGNTOOL="%%a"
call manifoldjs -l debug -p edgeextension package JobCTRLExtension\edgeextension\manifest\
%SIGNTOOL% sign /fd SHA256 /tr http://timestamp.comodoca.com /td sha256 /a /f ..\..\Keys\jobctrlkft_cs_sha256.p12 /p jobctrl789 JobCTRLExtension\edgeextension\package\edgeExtension.appx
move JobCTRLExtension\edgeextension\package\edgeExtension.appx ..\EdgeInterop\edgeExtension.appx
