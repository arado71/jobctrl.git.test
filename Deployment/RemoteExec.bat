@echo OFF
set SERVER=%1
set USER=%2
set PASSWORD=%3
set REMOTEBAT=%4

cmdkey.exe /add:%SERVER% /user:%SERVER%\%USER% /pass:%PASSWORD%
..\Utilities\PsExec.exe \\%SERVER% -u .\%USER% -p %PASSWORD% -accepteula cmd /c %REMOTEBAT%
cmdkey.exe /delete:%SERVER%