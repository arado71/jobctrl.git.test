@ECHO OFF
echo Building Client TEST version (Rev: 0)
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "%~dp0\..\ActivityRecorderClient.Tests.csproj" /target:rebuild;test /p:Configuration=Debug /p:Platform=AnyCPU /p:ProductName=JobCTRL-DEV /p:ClientAssemblyName=JobCTRL-DEV /p:VisualStudioVersion=12.0
REM c:\Z\ProjectsInstall\ActivityRecorder\ClickOnce\
exit /B %ERRORLEVEL%
@pause