@echo off
echo WoADialer created by Simone Franco @simizfo
echo for the WoALumia Project.
echo ------
echo This setup will install the WoADialer UWP App
echo the background app and the required files.
echo Confirm now, or close this console window.
pause 

net session >nul 2>&1
if %errorLevel% == 0 (
    echo Administrator permissions found...
) else (
    echo You need to run this tool as administrator.
    pause
    exit
)

echo Copying needed files...
echo F | xcopy %~dp0\files\WoADialerHelper.exe C:\Windows\System32\WoADialerHelper.exe /O /X /H /K /F
echo Adding registry keys...
regedit /S %~dp0\files\regkeys.reg
echo Installing WoADialer UWP App...
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process PowerShell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File ""%~dp0\files\woadialer\Add-AppDevPackage.ps1""' -Verb RunAs}"
echo Creating the background task...
schtasks.exe /Create -tn "WoADialerTask" /XML %~dp0\files\WoADialerTask.xml
echo Completed! Enjoy WoADialer!
pause