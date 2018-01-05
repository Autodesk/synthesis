!include MUI2.nsh
;!include LogicLib.nsh

Name "Synthesis"

Icon "logo-outline.ico"

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
    MessageBox MB_YESNO "You appear to have Synthesis installed, would you like to reinstall it?" IDYES true IDNO false
      ; Remove registry keys
      true:
        DeleteRegKey HKLM SOFTWARE\Synthesis

        RMDir /r /REBOOTOK $INSTDIR
        Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDRobotExporter.inventor.addin"
        Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDFieldExporter.inventor.addin"
        Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDRobotExporter.inventor.addin"
        Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDFieldExporter.inventor.addin"
        RMDIR /r /REBOOTOK $APPDATA\RobotViewer
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

  SetOutPath $INSTDIR
  CreateShortCut "$SMPROGRAMS\Synthesis.lnk" "$INSTDIR\SynthesisLauncher.exe"
  CreateShortCut "$DESKTOP\Synthesis.lnk" "$INSTDIR\SynthesisLauncher.exe"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayName" "Autodesk Synthesis"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayIcon" "plantlogo(NoBack).ico"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "Publisher" "Autodesk"
  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "URLInfoAbout" "BXD.Autodesk.com/tutorials"
  ; Update this on release
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "DisplayVersion" "4.1.0.0"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "UninstallString" "$\"$INSTDIR\uninstall.exe$\""


  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
  WriteUninstaller "uninstall.exe"

SectionEnd

Section "MixAndMatch Files"

SetOutPath $APPDATA\Synthesis\MixAndMatch

File /r "MixAndMatch\*"

SectionEnd

Section

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR

  File "SynthesisLauncher.exe"
  File "Apache2.rtf"

SectionEnd

Section /o "Standalone Robot Exporter (legacy)"

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR\RobotExporter

  File /r "RobotExporter\*"

SectionEnd

Section /o "Standalone Field Exporter (legacy)"

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR\FieldExporter

  File /r "FieldExporter\SimulatorAPI.dll"
  File /r "FieldExporter\FieldExporter.exe"
  File /r "FieldExporter\ConvexLibraryWrapper.dll"
	
SectionEnd

Section "Robot Exporter Plugin (reccommended)"

  ; Set output path to plugin directory
  SetOutPath "$INSTDIR"
  File /r "RobotExporter\RobotDistrib.bat"
  File /r "RobotExporter\autodesk.BxDRobotExporter.inventor.addin"
  ExecShell open "$INSTDIR\RobotDistrib.bat" SW_HIDE

  SetOutPath "C:\Program Files (x86)\Autodesk\Synthesis"
  File /r "RobotExporter\BxDRobotExporter.dll"
 
  SetOutPath "$APPDATA\RobotViewer"
  File /r "RobotExporter\Viewer\RobotViewer.exe"
  File /r "RobotExporter\Viewer\OpenTK.dll"
  File /r "RobotExporter\Viewer\OpenTK.GLControl.dll"
  File /r "RobotExporter\Viewer\OGLViewer.dll"
  File /r "RobotExporter\Viewer\SimulatorAPI.dll"

SectionEnd

Section "Field Exporter Plugin (reccommended)"
	
  ; Set output path to plugin directory
  SetOutPath "$INSTDIR"
  File /r "FieldExporter\FieldDistrib.bat"
  File /r "FieldExporter\autodesk.BxDFieldExporter.inventor.addin"
  ExecShell open "$INSTDIR\FieldDistrib.bat" SW_HIDE

  SetOutPath "C:\Program Files (x86)\Autodesk\Synthesis"
  File /r "FieldExporter\BxDFieldExporter.dll"

SectionEnd

Section "Code Emulator (optional)"

  SetOutPath $INSTDIR\SynthesisDrive

  File /r "SynthesisDrive\*"

  SetOutPath $INSTDIR\cygscripts
  File /r "cygscripts\*"

  ; installs cygwin
  ExecWait "$INSTDIR\cygscripts\cygpac.bat"
  
SectionEnd

Section "Robot Files"

SetOutPath $APPDATA\Synthesis\Robots

File /r "Robots\*"

SectionEnd

Section "Field Files"

SetOutPath $APPDATA\Synthesis\Fields

File /r "Fields\*"

SectionEnd

Section "Uninstall"

  ; Remove registry keys
  DeleteRegKey HKLM SOFTWARE\Synthesis

  RMDir /r /REBOOTOK $INSTDIR
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDFieldExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDFieldExporter.inventor.addin"
  ; Remove files and uninstaller
  Delete $INSTDIR\Synthesis.nsi
  Delete $INSTDIR\uninstall.exe
  Delete $INSTDIR\*
  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\Synthesis.lnk"
  Delete "$DESKTOP\Synthesis.lnk"
  Delete "$DESKTOP\BXD Synthesis.lnk"

  ; Remove Installshield shortcuts
  Delete "$SMPROGRAMS\Autodesk Synthesis.lnk"
  Delete "$DESKTOP\Autodesk Synthesis.lnk"
  Delete "$SMPROGRAMS\BXD Synthesis.lnk"
  ; Remove directories used
  RMDir $INSTDIR

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis"

SectionEnd

Section

MessageBox MB_YESNO "Thank you for installing Synthesis, would you like to view our Readme?" IDNO NoReadme
      ExecShell "open" "https://github.com/Autodesk/synthesis/blob/master/README.md"
    NoReadme:


    Exec "$INSTDIR\SynthesisLauncher.exe"

    MessageBox MB_OK "Synthesis has been installed succsessfully!"

    MessageBox MB_OK "In order to improve this product and understand how it is used, we collect non-personal product usage information. This usage information may consist of custom events like Replay Mode, Driver Practice Mode, Tutorial Link Clicked, etc. $\r$\nThis information is not used to identify or contact you. $\r$\nYou can turn data collection off from the Control Panel within the simulation."

    Quit
SectionEnd
