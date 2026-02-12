@echo off

set HG_TOOL=%1
set HG_REPO=%2
set HG_BRANCH=%3
set HG_REVISION=%4
set WORKDIR=%5
set JCVER=%6
set BASEDIR=%CD%

pushd %WORKDIR%
%HG_TOOL% clone %HG_REPO% .\
%HG_TOOL% pull 
%HG_TOOL% update -C %HG_BRANCH%
%HG_TOOL% update -r %HG_REVISION%
copy %BASEDIR%\..\ActivityRecorderClient\ActivityRecorderClient.csproj %WORKDIR%\ActivityRecorderClient\
copy %BASEDIR%\..\ActivityRecorderClient\Properties\AssemblyInfo.cs %WORKDIR%\ActivityRecorderClient\Properties\
copy %BASEDIR%\..\ActivityRecorderService\Properties\AssemblyInfo.cs %WORKDIR%\ActivityRecorderService\Properties\
%HG_TOOL% update -m -y -t internal:merge-local %HG_BRANCH%
%HG_TOOL% commit -m "Release number updated" -u SYSTEM
if errorlevel 1 goto do_push
%HG_TOOL% tag %JCVER% -u SYSTEM
:do_push
rem clear errorlevel!
%HG_TOOL% push
if not errorlevel 2 if errorlevel 1 ver >null
popd
