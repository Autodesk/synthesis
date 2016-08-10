!include MUI2.nsh
;!include LogicLib.nsh

Name "Synthesis"

Icon "C:\Users\t_hics\Documents\GitHub\synthesis\installer\plantlogo(NoBack).ico"

OutFile "Synthesis Installer.exe"

InstallDir $PROGRAMFILES\Autodesk\Synthesis

InstallDirRegKey HKLM "Software\Synthesis" "Install_Dir"

RequestExecutionLevel admin

Page components
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

Section

;Where we can read registry data if we need it
IfFileExists "$INSTDIR" +1 +27
    MessageBox MB_YESNO "You appear to have synthesis installed, would you like to reinstall it?" IDYES true IDNO false
      ; Remove registry keys
      true:
        DeleteRegKey HKLM SOFTWARE\Synthesis

        RMDir /r /REBOOTOK $INSTDIR
        RMDir /r /REBOOTOK $APPDATA\Autodesk\ApplicationPlugins\Synthesis
        ; Remove files and uninstaller
        Delete $INSTDIR\Synthesis.nsi
        Delete $INSTDIR\uninstall.exe
        Delete $INSTDIR\*

        ; Remove shortcuts, if any
        Delete "$SMPROGRAMS\Synthesis.lnk"
        Delete "$DESKTOP\Synthesis.lnk"
        Delete "$DESKTOP\BXD Synthesis.lnk"
        ; Remove directories used
        RMDir $INSTDIR

        DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis"

        Goto next
        
      false:
        Quit

      next:
      



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

CreateShortCut "$DESKTOP\Synthesis.lnk" "$INSTDIR\Synthesis.exe" ""

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayName" "Autodesk Synthesis" 

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayIcon" "C:\Users\t_hics\Documents\GitHub\synthesis\installer\plantlogo(NoBack).ico"

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "Publisher" "Autodesk" 

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "Readme" "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\README.rtf"

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "URLInfoAbout" "BXD.Autodesk.com/tutorials" 

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "DisplayVersion" "3.0.1.0"

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "UninstallString" "$\"$INSTDIR\uninstall.exe$\""

createShortCut "$SMPROGRAMS\Synthesis.lnk" "$INSTDIR\Synthesis.exe"

WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
WriteUninstaller "uninstall.exe"
  
SectionEnd

Section "Exporter Plugin (optional)"

  ; Set output path to the installation directory.
  IfFileExists "$APPDATA\Autodesk\ApplicationPlugins" +1 +5
    ;MessageBox MB_OK "Inventor is installed"
    SetOutPath "$APPDATA\Autodesk\ApplicationPlugins\Synthesis"
    File /r "C:\Users\t_hics\Downloads\3.0.1.0\3.0.1.0\Exporter\*"
  ;MessageBox MB_OK "Inventor is installed"
    Goto +2
    SetOutPath "$INSTDIR\Exporter"
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
  DeleteRegKey HKLM SOFTWARE\Synthesis

  RMDir /r /REBOOTOK $INSTDIR
  RMDir /r /REBOOTOK $APPDATA\Autodesk\ApplicationPlugins\Synthesis
  ; Remove files and uninstaller
  Delete $INSTDIR\Synthesis.nsi
  Delete $INSTDIR\uninstall.exe
  Delete $INSTDIR\*

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\Synthesis.lnk"
  Delete "$DESKTOP\Synthesis.lnk"
  Delete "$DESKTOP\BXD Synthesis.lnk"
  ; Remove directories used
  RMDir $INSTDIR

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis"

SectionEnd

Function .onInstSuccess
    MessageBox MB_YESNO "Thank you for installing Synthesis, would you like to view our Readme?" IDNO NoReadme
      ExecShell "" "$instdir\readme.rtf"
    NoReadme:
FunctionEnd

