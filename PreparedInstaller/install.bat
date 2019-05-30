echo F | xcopy %~dp0\files\WoADialerHelper.exe C:\Windows\System32\WoADialerHelper.exe /O /X /H /K /F
regedit /S %~dp0\files\regkeys.reg
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process PowerShell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File ""%~dp0\files\woadialer\Add-AppDevPackage.ps1""' -Verb RunAs}"
schtasks.exe /Create -tn "WoADialerTask" /XML files\WoADialerTask.xml
pause