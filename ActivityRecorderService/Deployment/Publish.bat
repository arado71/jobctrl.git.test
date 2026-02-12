 @echo OFF
 REM Must be absolute path!
 if "%TARGETDIR%" == "" (
	SET "TARGETDIR=%CD%"
 )
 
 if exist "%TARGETDIR%\JcService.zip" del "%TARGETDIR%\JcService.zip"
 pushd ..\bin\Release
 ..\..\..\Utilities\7za.exe a -mx9 -y "%TARGETDIR%\JcService.zip" *
 pushd ..\..\..\RecorderDatabase\
 ..\Utilities\7za.exe a -mx9 -y "%TARGETDIR%\JcService.zip" "Change Scripts\change*.sql"
 popd
 popd
 
 