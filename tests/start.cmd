@echo off

setlocal
set pathdir=%~dp0

rem echo ᠬ� ���� %CD%
rem echo �ਯ�� ���஢���� %pathdir%
echo "%pathdir%\..\install\built\oscript.exe" %pathdir%\start.os -run %1 %2 %3 %4 %5

"%pathdir%\..\install\built\oscript.exe" %pathdir%\start.os -run %1 %2 %3 %4 %5

rem echo %ERRORLEVEL%
if NOT %ERRORLEVEL%==0 GOTO bad_exit

:success_exit
rem echo �ᯥ譮
rem pause

exit /B 0

:bad_exit
if %ERRORLEVEL%==-1 GOTO success_exit
echo Tests failed

pause
exit /B 1