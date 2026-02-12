REM === Connection strings ===

REM ActivityRecorderService Unit testing database
if "%JC_TEST_CONNECTIONSTRING%" == "" (
	set "JC_TEST_CONNECTIONSTRING=Data Source=.;Initial Catalog=recorder_test;Integrated Security=True;Encrypt=False"
)

REM === Executable paths ===

if "%SQLPACKAGE%" == "" (
	set "SQLPACKAGE=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe"
)

if "%MSBUILD%" == "" (
	set "MSBUILD=c:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
)

if "%SQLPUBWIZ%" == "" (
	set "SQLPUBWIZ=c:\Program Files (x86)\Microsoft SQL Server\90\Tools\Publishing\1.4\SqlPubWiz.exe"
)

if "%SQLDBExtensionsRefPath%" == "" (
	set "SQLDBExtensionsRefPath=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Microsoft\VisualStudio\v17.0\SSDT"
)

if "%VsIdePath%" == "" (
	set "VsIdePath=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE"
)
