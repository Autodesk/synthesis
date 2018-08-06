!include MUI2.nsh
;!include LogicLib.nsh

Name "Synthesis"

Icon "W16_SYN_launch.ico"

OutFile "SynthesisInstaller.exe"

InstallDir $PROGRAMFILES\Autodesk\Synthesis

InstallDirRegKey HKLM "Software\Synthesis" "Install_Dir"

RequestExecutionLevel admin

;--------------------------------
;Interface Settings

  !define MUI_WELCOMEFINISHPAGE_BITMAP "W21_SYN_sidebar.bmp"
  !define MUI_UNWELCOMEFINISHPAGE_BITMAP "W21_SYN_sidebar.bmp"
  !define MUI_ICON "W16_SYN_launch.ico"
  !define MUI_UNICON "W16_SYN_launch.ico"
  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_BITMAP "orange-r.bmp"
  !define MUI_HEADERIMAGE_RIGHT
  !define MUI_ABORTWARNING
  !define MUI_FINISHPAGE_TEXT 'Synthesis has been successfully installed on your system. $\r$\n $\r$\nIn order to improve this product and understand how it is used, we collect non-personal product usage information. This usage information may consist of custom events like Replay Mode, Driver Practice Mode, Tutorial Link Clicked, etc. $\r$\nThis information is not used to identify or contact you. $\r$\nYou can turn data collection off from the Control Panel within the simulator. $\r$\n $\r$\nBy clicking Finish, you agree that you have read the terms of service agreement and data collection statement above.'
  !define MUI_FINISHPAGE_LINK "Synthesis GitHub Wiki"
  !define MUI_FINISHPAGE_LINK_LOCATION "https://github.com/Autodesk/synthesis/wiki"
  
;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "Apache2.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"

;UninstPage uninstConfirm
;UninstPage instfiles

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

Section "Synthesis (required)" SynthesisRequired

  SectionIn RO

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR\Synthesis

  ; Put file there
  ;File "installer.nsi"
  File /r "Synthesis\*"

  SetOutPath $INSTDIR
  CreateShortCut "$SMPROGRAMS\Synthesis.lnk" "$INSTDIR\Synthesis\Synthesis.exe"
  CreateShortCut "$DESKTOP\Synthesis.lnk" "$INSTDIR\Synthesis\Synthesis.exe"

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
  
	;Install Fields
	SetOutPath $APPDATA\Synthesis\Fields
	File /r "Fields\*"

SectionEnd

Section "MixAndMatch Files" MixMatch

SetOutPath $APPDATA\Synthesis\MixAndMatch

File /r "MixAndMatch\*"

SectionEnd

Section /o "Standalone Robot Exporter (legacy)" LegacyExporter

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR\RobotExporter

  File /r "RobotExporter\*"

SectionEnd

Section "Robot Exporter Plugin (reccommended)" PluginExporter

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

Section "Robot Files" RoboFiles

SetOutPath $APPDATA\Synthesis\Robots

File /r "Robots\*"

SectionEnd

;--------------------------------
;Component Descriptions

  LangString DESC_SynthesisRequired ${LANG_ENGLISH} "The Unity5 Simulator Engine is what the exported fields and robots are loaded into. In real-time, it simulates a real world physics environment for robots to interact with fields or other robots"
  LangString DESC_MixMatch ${LANG_ENGLISH} "Mix and Match will allow the user to quickly choose from pre-configured robot parts such as wheels, drive bases and manipulators within the simulator"
  LangString DESC_LegacyExporter ${LANG_ENGLISH} "The Legacy Robot Exporter is a standalone application used to convert an Autodesk Inventor Assembly file into a format that can be read and loaded by the simulator"
  LangString DESC_PluginExporter ${LANG_ENGLISH} "The Robot Exporter Plugin is an Inventor Addin used to import Autodesk Inventor Assemblies directly into the simulator"
  LangString DESC_RoboFiles ${LANG_ENGLISH} "A library of sample robots pre-loaded into the simulator"

  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SynthesisRequired} $(DESC_SynthesisRequired)
  !insertmacro MUI_DESCRIPTION_TEXT ${MixMatch} $(DESC_MixMatch)
  !insertmacro MUI_DESCRIPTION_TEXT ${LegacyExporter} $(DESC_LegacyExporter)
  !insertmacro MUI_DESCRIPTION_TEXT ${PluginExporter} $(DESC_PluginExporter)
  !insertmacro MUI_DESCRIPTION_TEXT ${RoboFiles} $(DESC_RoboFiles)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END
  
;--------------------------------
  
Section "Uninstall"

  ; Remove registry keys
  DeleteRegKey HKLM SOFTWARE\Synthesis

  RMDir /r /REBOOTOK $INSTDIR
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDRobotExporter.inventor.addin"
  
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
