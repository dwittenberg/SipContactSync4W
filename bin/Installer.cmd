if not exist "%APPDATA%\PhonerLiteContactSync" (mkdir "%APPDATA%\PhonerLiteContactSync")
copy * "%APPDATA%\PhonerLiteContactSync" /y
del "%APPDATA%\PhonerLiteContactSync\Installer.cmd"