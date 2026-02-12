@ECHO OFF
echo Publishing TEST version for Magyar Posta (perMachine)
SET publishVersion=%1
if "%~1" == "" (
  SET /P publishVersion=Please enter the version: 
)

call "%~dp0\JobCTRLClientPublish.bat" JobCTRL %publishVersion% http://jc360.tst.vftk.posta.hu/Install/ "%~dp0\%~n0_v%publishVersion%\\" PostaTest b6f86395-2fdc-4324-b28f-961aeda90ee0 perMachine 1 NoSync NoOcr SnkAndSign NoObfuscar
if "%~1" == "" (
  @pause
)
exit /B %ERRORLEVEL%
