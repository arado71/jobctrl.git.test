@ECHO OFF
echo Uninstalling JobCTRL Service...
echo ---------------------------------------------------
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil /u JobCTRLService.exe
echo ---------------------------------------------------
echo Done.
@pause