@echo off

setlocal
set pathdir=%~dp0
set success=1

rem echo ᠬ� ���� %CD%
rem echo �ਯ�� ���஢���� %pathdir%
rem echo "%pathdir%\..\install\built\oscript.exe" %pathdir%\start.os -run %1 %2 %3 %4 %5

for %%I in (*.os) do (
if NOT "%%~nI"=="start" (
if NOT "%%~nI"=="testrunner" (
	echo ---
	echo - 䠩� ��� -   %%I
	echo ---

	@call "%pathdir%\..\install\built\oscript.exe" %pathdir%\start.os -run %%I %1 %2 %3 %4 %5

	rem echo %ERRORLEVEL%
	if NOT %ERRORLEVEL%==0 set success=%ERRORLEVEL%
	rem set success=%ERRORLEVEL%

	rem echo success = %success%
	rem GOTO bad_exit
)
)
)

if NOT "%success%"=="0" GOTO bad_exit


:success_exit
rem echo �ᯥ譮
rem pause

exit /B 0

:bad_exit
if "-1"=="%success%" GOTO success_exit
echo .
echo ��᪮�쪮 ��⮢ 㯠��
echo ��㤠�.  ��᭠� �����

if ".%1"=="." pause
exit /B 1