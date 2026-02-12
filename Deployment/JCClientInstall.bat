@echo off
IF EXIST "%LOCALAPPDATA%\Apps\JobCTRL\JobCTRL.exe" GOTO end
msiexec /qn /i "https://jobctrl.com/install/jobctrl.msi" RUNAFTERINSTALL=1
:end