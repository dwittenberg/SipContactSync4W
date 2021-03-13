@echo off
if not exist "%APPDATA%\PhonerLiteContactSync" (mkdir "%APPDATA%\PhonerLiteContactSync")
copy * "%APPDATA%\PhonerLiteContactSync" /y
del "%APPDATA%\PhonerLiteContactSync\Installer.cmd"

taskkill /IM "PhonerLite.exe"
Timeout /T 3 /Nobreak

:LOOP
tasklist | find /i "PhonerLite" >nul 2>&1
IF ERRORLEVEL 1 (
  GOTO CONTINUE
) ELSE (
  Timeout /T 1 /Nobreak
  GOTO LOOP
)

:CONTINUE

powershell -Command "(gc $env:APPDATA/PhonerLite/PhonerLite.ini) -replace 'terminated=(.)*', 'terminated=%appdata%\PhonerLiteContactSync\syncstart.bat' | Out-File -encoding ASCII $env:APPDATA/PhonerLite/PhonerLite.ini"

start "PhonerLite" "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\PhonerLite\PhonerLite.lnk"