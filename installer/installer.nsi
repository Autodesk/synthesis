Name "SynthesisInstaller"

OutFile "SynthesisInstaller.exe"

InstallDir $PROGRAMFILES\Autodesk\Synthesis

InstallDirRegKey HKLM "Software\Synthesis" "Install_Dir"

RequestExecutionLevel admin

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

Section "Synthesis (required)"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  ;File "installer.nsi"
  File /r "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Synthesis\*"
  File "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Apache2.rtf"
  File "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\README.rtf"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\Synthesis "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "Synthesis" "Autodesk Synthesis"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd

Section "Exporter Plugin (optional)"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  ;File "example2.nsi"
  File /r "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Exporter\*"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\Synthesis "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "Exporter" "Autodesk Synthesis"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd

Section "Code Emulator (optional)"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  ;File "example2.nsi"
  File /r "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Code\*"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\Synthesis "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "Code Emulation" "Autodesk Synthesis"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd 

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis"
  DeleteRegKey HKLM SOFTWARE\Synthesis

  ; Remove files and uninstaller
  Delete $INSTDIR\Synthesis.nsi
  Delete $INSTDIR\uninstall.exe

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\Synthesis\*.*"

  ; Remove directories used
  RMDir "$SMPROGRAMS\Synthesis"
  RMDir "$INSTDIR"

SectionEnd

