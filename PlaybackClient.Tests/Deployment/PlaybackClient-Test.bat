@ECHO OFF
echo Building Service TEST version (Rev: 0)
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\community\MSBuild\15.0\Bin\msbuild.exe" "%~dp0\..\PlaybackClient.Tests.csproj" /target:rebuild;test /p:Configuration=Debug /p:Platform=AnyCPU /p:VisualStudioVersion=15.0
REM c:\Z\ProjectsInstall\ActivityRecorder\ClickOnce\
exit /B %ERRORLEVEL%
@pause