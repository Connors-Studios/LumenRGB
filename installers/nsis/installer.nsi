OutFile "LumenRGB-Setup.exe"
InstallDir "$PROGRAMFILES\LumenRGB"

Section "Install"
  SetOutPath "$INSTDIR"
  File /r "publish/*.*"
  CreateShortcut "$DESKTOP\LumenRGB.lnk" "$INSTDIR\LumenRGB.exe"
SectionEnd

Section "Uninstall"
  Delete "$DESKTOP\LumenRGB.lnk"
  RMDir /r "$INSTDIR"
SectionEnd
