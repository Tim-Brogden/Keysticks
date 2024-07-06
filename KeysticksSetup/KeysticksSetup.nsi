!ifndef VERSION
  !define VERSION "1.0.0.0"
!endif
!ifndef COMPANYNAME
  !define COMPANYNAME "Company Name"
!endif
!ifndef PRODUCTNAME
  !define PRODUCTNAME "Keysticks"
!endif
!ifndef YEAR
  !define YEAR 2024
!endif

Unicode true

VIProductVersion              "${VERSION}"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "ProductVersion" "${VERSION}"
VIAddVersionKey "ProductName" "${PRODUCTNAME}"
VIAddVersionKey "CompanyName" "${COMPANYNAME}"
VIAddVersionKey "Comments" "Installs ${PRODUCTNAME} on your computer"
VIAddVersionKey "LegalCopyright" "Copyright © ${YEAR} ${COMPANYNAME}"
VIAddVersionKey "FileDescription" "${PRODUCTNAME} Installer"
VIAddVersionKey "OriginalFilename" "${PRODUCTNAME}Setup-${VERSION}.exe"
OutFile "${PRODUCTNAME}Setup-${VERSION}.exe"

SilentInstall silent

Section Main    
    SetOutPath "$TEMP\${PRODUCTNAME}.tmp"
    SetOverwrite on
    File /r Release
    Exec '"$OUTDIR\Release\Setup.exe"'
SectionEnd