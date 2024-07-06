REM Edit the values of these variables as required
set KXVERSION=2.1.4.0
set KXCOMPANYNAME="Tim Brogden"
set KXPRODUCTNAME="Keysticks"
set KXYEARNAME=2024
REM signtool sign /t http://timestamp.comodoca.com/authenticode /d Keysticks /a "Release\KeysticksSetup.msi"
REM signtool sign /t http://timestamp.comodoca.com/authenticode /a "Release\Setup.exe"
REM copy "Release\KeysticksSetup.msi" .\KeysticksSetup-%KXVERSION%.msi
"C:\Program Files (x86)\NSIS\makensis.exe" /DVERSION=%KXVERSION% /DCOMPANYNAME=%KXCOMPANYNAME% /DPRODUCTNAME=%KXPRODUCTNAME% /DYEARNAME=%KXYEARNAME% KeysticksSetup.nsi
pause
REM signtool sign /t http://timestamp.comodoca.com/authenticode /a "KeysticksSetup-%KXVERSION%.exe"
REM pause
