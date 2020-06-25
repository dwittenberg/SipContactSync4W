set name = "%APPDATA%\PhonerLiteContactSync"

if not exist name mkdir name
copy * name /y
remove (name+"\Installer.com")