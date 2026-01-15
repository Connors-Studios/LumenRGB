; ---------------------------------------
; Modern UI 2
; ---------------------------------------
!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"

Var RelaunchAfterInstall

!define MUI_ICON "LumenRGB_arc_256.ico"
!define MUI_UNICON "LumenRGB_arc_256.ico"

!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_TEXT "Launch LumenRGB"
!define MUI_FINISHPAGE_RUN_FUNCTION LaunchLumenRGB

; ---------------------------------------
; Detect /L flag (silent relaunch)
; ---------------------------------------
Function .onInit
    ; Read full command line
    StrCpy $R0 "$CMDLINE"

    ; Default: no relaunch
    StrCpy $RelaunchAfterInstall "0"

    ; Get length of command line
    StrLen $R2 $R0

    ; Start at index 0
    StrCpy $R3 0

    loop:
        ; If index == length, stop
        StrCmp $R3 $R2 done

        ; Read 2 characters starting at index R3
        StrCpy $R4 $R0 2 $R3

        ; Compare to "/L"
        StrCmp $R4 "/L" found

        ; Move to next character
        IntOp $R3 $R3 + 1
        Goto loop

    found:
        StrCpy $RelaunchAfterInstall "1"

    done:
FunctionEnd

; ---------------------------------------
; Installer Metadata
; ---------------------------------------
OutFile "LumenRGB-Setup-${VERSION}.exe"
Name "LumenRGB"
InstallDir "$PROGRAMFILES\LumenRGB"
InstallDirRegKey HKLM "Software\LumenRGB" "InstallLocation"

; Version + Company Info
VIAddVersionKey "ProductName" "LumenRGB"
VIAddVersionKey "CompanyName" "Connor Studios"
VIAddVersionKey "FileDescription" "LumenRGB Lighting Control"
VIAddVersionKey "ProductVersion" "${VERSION}"
VIAddVersionKey "LegalCopyright" "© Connor Studios"
VIProductVersion "${VERSION_NUMERIC}"

; Embed installer + uninstaller icon
Icon "LumenRGB_arc_256.ico"
UninstallIcon "LumenRGB_arc_256.ico"

; ---------------------------------------
; Pages
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
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\LumenRGB" "DisplayVersion" "${VERSION}"
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

; ---------------------------------------
; Launch function (Finish page)
; ---------------------------------------
Function LaunchLumenRGB
  Exec "$INSTDIR\LumenRGB.exe"
FunctionEnd

; ---------------------------------------
; Auto-launch after silent install (/L)
; ---------------------------------------
Function .onInstSuccess
    ${If} $RelaunchAfterInstall == "1"
        Exec "$INSTDIR\LumenRGB.exe"
    ${EndIf}
FunctionEnd
