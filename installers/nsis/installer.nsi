; ---------------------------------------
; Modern UI 2
; ---------------------------------------
!include "MUI2.nsh"

!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_TEXT "Launch LumenRGB"
!define MUI_FINISHPAGE_RUN_FUNCTION LaunchLumenRGB

; ---------------------------------------
; Installer Metadata
; ---------------------------------------
OutFile "LumenRGB-Setup.exe"
Name "LumenRGB"
InstallDir "$PROGRAMFILES\LumenRGB"
InstallDirRegKey HKLM "Software\LumenRGB" "InstallLocation"

; Version + Company Info
VIAddVersionKey "ProductName" "LumenRGB"
VIAddVersionKey "CompanyName" "Connor Studios"
VIAddVersionKey "FileDescription" "LumenRGB Lighting Control"
VIAddVersionKey "ProductVersion" "1.0.0"
VIAddVersionKey "LegalCopyright" "© Connor Studios"
VIProductVersion "1.0.0.0"

; Embed installer + uninstaller icon
Icon "LumenRGB_arc_256.ico"
UninstallIcon "LumenRGB_arc_256.ico"

; ---------------------------------------
; Pages (Welcome → License → Directory → Install → Finish)
; ---------------------------------------
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

; ---------------------------------------
; Install Section
; ---------------------------------------
Section "Install"
  SetOutPath "$INSTDIR"

  ; Install published files
  File /r "../../publish/*.*"

  ; Write uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ; Desktop shortcut
  CreateShortcut "$DESKTOP\LumenRGB.lnk" "$INSTDIR\LumenRGB.exe"

  ; Start Menu folder + shortcuts
  CreateDirectory "$SMPROGRAMS\LumenRGB"
  CreateShortcut "$SMPROGRAMS\LumenRGB\LumenRGB.lnk" "$INSTDIR\LumenRGB.exe"
  CreateShortcut "$SMPROGRAMS\LumenRGB\Uninstall LumenRGB.lnk" "$INSTDIR\Uninstall.exe"

  ; Register in Windows Apps & Features
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "DisplayName" "LumenRGB"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "Publisher" "Connor Studios"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "DisplayVersion" "1.0.0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "InstallLocation" "$INSTDIR"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "DisplayIcon" "$INSTDIR\LumenRGB.exe"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "NoRepair" 1
SectionEnd

; ---------------------------------------
; Uninstall Section
; ---------------------------------------
Section "Uninstall"
  Delete "$DESKTOP\LumenRGB.lnk"
  Delete "$SMPROGRAMS\LumenRGB\LumenRGB.lnk"
  Delete "$SMPROGRAMS\LumenRGB\Uninstall LumenRGB.lnk"
  RMDir "$SMPROGRAMS\LumenRGB"

  RMDir /r "$INSTDIR"

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB"
SectionEnd

Function LaunchLumenRGB
  Exec "$INSTDIR\LumenRGB.exe"
FunctionEnd
