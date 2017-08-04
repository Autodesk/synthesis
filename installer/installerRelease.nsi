!include MUI2.nsh
;!include LogicLib.nsh

Name "Synthesis"

Icon "plantlogo(NoBack).ico"

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
IfFileExists "$INSTDIR" +1 +28
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
        Delete $APPDATA\Autodesk\ApplicationPlugins\*

        ; Remove shortcuts, if any
        Delete "$SMPROGRAMS\Synthesis.lnk"
        Delete "$DESKTOP\Synthesis.lnk"
        Delete "$DESKTOP\BXD Synthesis.lnk"

        ;Remvoe Installshield shortcuts
        Delete "$SMPROGRAMS\Autodesk Synthesis.lnk"
        Delete "$DESKTOP\Autodesk Synthesis.lnk"
        Delete "$DESKTOP\BXD Synthesis.lnk"
        Delete "$SMPROGRAMS\BXD Synthesis.lnk"
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
  SetOutPath $INSTDIR\Synthesis
  
  ; Put file there
  ;File "installer.nsi"
  File /r "Synthesis\*"

CreateShortCut "$DESKTOP\Synthesis.lnk" "$INSTDIR\SynthesisLauncher.exe" ""

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayName" "Autodesk Synthesis" 

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayIcon" "plantlogo(NoBack).ico"

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

createShortCut "$SMPROGRAMS\Synthesis.lnk" "$INSTDIR\SynthesisLauncher.exe"

WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
WriteUninstaller "uninstall.exe"
  
SectionEnd

Section

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR

  File "SynthesisLauncher.exe"
  File "Apache2.rtf"
  File "README.pdf"

SectionEnd

Section "Robot Exporter (optional)"

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR\RobotExporter

  File /r "RobotExporter\*"

SectionEnd

Section "Field Exporter (optional)"

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR\FieldExporter

  File /r "FieldExporter\*"

SectionEnd

Section "Code Emulator (optional)"
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR\SynthesisDrive
  
  ; Put file there
  ;File "example2.nsi"
  File /r "SynthesisDrive\*"

  SetOutPath $INSTDIR\cygscripts
  File /r "cygscripts\*"

  ExecShellWait "open" "$INSTDIR\cygscripts\cygpac.bat" "mingw64-x86_64-gcc-g++" SW_HIDE
  
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
  Delete $APPDATA\Autodesk\ApplicationPlugins\*

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\Synthesis.lnk"
  Delete "$DESKTOP\Synthesis.lnk"
  Delete "$DESKTOP\BXD Synthesis.lnk"

        ;Remvoe Installshield shortcuts
  Delete "$SMPROGRAMS\Autodesk Synthesis.lnk"
  Delete "$DESKTOP\Autodesk Synthesis.lnk"
  Delete "$SMPROGRAMS\BXD Synthesis.lnk"
  ; Remove directories used
  RMDir $INSTDIR

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis"

SectionEnd

Section

MessageBox MB_YESNO "Thank you for installing Synthesis, would you like to view our Readme?" IDNO NoReadme
      ExecShell "" "$instdir\README.pdf"
    NoReadme:


    Exec "$INSTDIR\SynthesisLauncher.exe"

    MessageBox MB_OK "Synthesis has been installed succsessfully!"

    Quit

SectionEnd

