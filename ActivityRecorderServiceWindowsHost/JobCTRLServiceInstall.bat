@ECHO OFF
echo Installing JobCTRL Service...
echo ---------------------------------------------------
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil /i JobCTRLService.exe
echo ---------------------------------------------------
echo Done.
@pause