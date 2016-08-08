!include MUI2.nsh
;!include LogicLib.nsh

Name "SynthesisInstaller"

Icon "C:\Users\t_hics\Desktop\LogoStuff\plantlogo(NoBack).ico"

OutFile "SynthesisInstaller.exe"

InstallDir $PROGRAMFILES\Autodesk\Synthesis

InstallDirRegKey HKLM "Software\Synthesis" "Install_Dir"

RequestExecutionLevel admin

;Var INVDIR5
;Var INVDIR6
;Var INVDIR7
;PageEx directory
 ; DirVar $INVDIR5
 ; DirVar $INVDIR6
 ; DirVar $INVDIR7
;PageExEnd

Page components
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

Section
 
    # read the value from the registry into the $0 register
    ReadRegStr $0 HKLM "SOFTWARE\Autodesk\Inventor" CurrentVersion
 
    # print the results in a popup message box
    MessageBox MB_OK "version: $0"
 
# default section end
SectionEnd

Section "Synthesis (required)"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  ;File "installer.nsi"
  File /r "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Synthesis\*"
  File "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Apache2.rtf"
  File "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\README.rtf"
  
SectionEnd

Section "Exporter Plugin (optional)"
  
  ; Set output path to the installation directory.
  IfFileExists "$APPDATA\Roaming\Autodesk\ApplicationPlugins\SynthesisExporter" 0 +2
  SetOutPath "$INSTDIR\Exporter"
  File /r "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Exporter\*"
  SetOutPath "$APPDATA\Roaming\Autodesk\Inventor 2016\Addins"
  ;SetOutPath $EXPDIR

  MessageBox MB_OK "Inventor is installed"
  ;SetOutPath $APPDATA\Roaming\Autodesk\Inventor 2017\Addins
  
  ; Put file there
  ;File "example2.nsi"
  File /r "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Exporter\*"
 
  
SectionEnd

Section "Code Emulator (optional)"
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR\SynthesisDrive
  
  ; Put file there
  ;File "example2.nsi"
  File /r "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Code\*"
  
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

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis"

SectionEnd

Function .onInstSuccess
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "DisplayName" "Autodesk Synthesis -- Robot Simulator"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "UninstallString" "$\"$INSTDIR\uninstall.exe$\""

WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
WriteUninstaller "uninstall.exe"

    MessageBox MB_YESNO "Congrats, it worked. View readme?" IDNO NoReadme
      Exec notepad.exe ; view readme or whatever, if you want.
    NoReadme:
FunctionEnd

