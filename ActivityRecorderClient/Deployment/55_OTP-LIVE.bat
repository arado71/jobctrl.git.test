@ECHO OFF
echo Publishing LIVE version for OTP (perUser)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JC360 %publishVersion% https://jc360.otpbank.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" OTP b6f86395-2fdc-4324-b28f-961aeda90ee0 perUser 1 NoSync NoOcr SnkAndSign NoObfuscar
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
