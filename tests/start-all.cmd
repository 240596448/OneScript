@echo on
chcp 866 > nul

setlocal
set pathdir=%~dp0
set success=1

set logfile=failtests.log
del /Q /F %logfile%
echo . > %logfile%

for %%I in (*.os) do (
if NOT "%%~nI"=="start" (
if NOT "%%~nI"=="testrunner" (
	echo ---
	echo - 䠩� ��� -   %%I
	echo ---

	call %1 "%pathdir%\start.os" -run %%I %2 %3 %4 %5

	rem @call "%pathdir%\..\install\built\oscript.exe" "%pathdir%\start.os" -run %%I %1 %2 %3 %4 %5
	rem @call "%ProgramFiles(x86)%\OneScript\oscript.exe" "%pathdir%\start.os" -run %%I %1 %2 %3 %4 %5
	
	
	if NOT %ERRORLEVEL%==0 (
		set success=%ERRORLEVEL%
		echo        ���� ��� "%%~nI" >> %logfile%
	)
)
)
)

if NOT "%success%"=="0" GOTO bad_exit


:success_exit

exit /B 0

:bad_exit
if "-1"=="%success%" GOTO success_exit
echo .
echo ��᪮�쪮 ��⮢ 㯠��
echo ��㤠�.  ��᭠� �����
echo    ����騥 ����:
type %logfile%

if ".%1"=="." pause
exit /B 1