!include MUI2.nsh
!include x64.nsh
;!include LogicLib.nsh

Name "Synthesis"

Icon "W16_SYN_launch.ico"

Caption "Synthesis 4.2.3 Setup [Snapshot Release]"

OutFile "SynthesisWinBeta4.2.3.exe"

InstallDir $PROGRAMFILES64\Autodesk\Synthesis

InstallDirRegKey HKLM "Software\Synthesis" "Install_Dir"

RequestExecutionLevel admin

  Section
  ${If} ${RunningX64}
	goto install_stuff
  ${Else}
	MessageBox MB_OK "Whoa There! This install requires a 64 bit system. Sorry about that."
	Quit
  ${EndIf}
    install_stuff:
  SectionEnd

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
  !define MUI_FINISHPAGE_LINK "Synthesis Tutorials Website"
  !define MUI_FINISHPAGE_LINK_LOCATION "http://bxd.autodesk.com/tutorials.html"
  
;--------------------------------

  ; Installer GUI Pages
  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "Apache2.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  ; Uninstaller GUI Pages
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH
  
;--------------------------------

  ; Default Language
  !insertmacro MUI_LANGUAGE "English"

Section

;Where we can read registry data if we need it
IfFileExists "$APPDATA\Autodesk\Synthesis" +1 +28
    MessageBox MB_YESNO "You appear to have Synthesis installed; would you like to reinstall it?" IDYES true IDNO false
      true:
        DeleteRegKey HKLM SOFTWARE\Synthesis

		; Remove outdated bxd inventor plugins
		Delete "$APPDATA\Autodesk\Inventor 2020\Addins\autodesk.BxDRobotExporter.inventor.addin"
        Delete "$APPDATA\Autodesk\Inventor 2019\Addins\autodesk.BxDRobotExporter.inventor.addin"
		Delete "$APPDATA\Autodesk\Inventor 2019\Addins\autodesk.BxDFieldExporter.inventor.addin"
		Delete "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDRobotExporter.inventor.addin"
		Delete "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDFieldExporter.inventor.addin"
        Delete "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDRobotExporter.inventor.addin"
        Delete "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDFieldExporter.inventor.addin"
		Delete "$APPDATA\Autodesk\ApplicationPlugins\Autodesk.BxDRobotExporter.Inventor.addin"
        RMDIR /r $APPDATA\RobotViewer

        ; Remove excess shortcuts
        Delete "$SMPROGRAMS\Synthesis.lnk"
        Delete "$DESKTOP\Synthesis.lnk"
		Delete "$SMPROGRAMS\BXD Synthesis.lnk"
        Delete "$DESKTOP\BXD Synthesis.lnk"
		Delete "$SMPROGRAMS\Autodesk Synthesis.lnk"
        Delete "$DESKTOP\Autodesk Synthesis.lnk"
        
        ; Remove expired directories
		RMDir /r $INSTDIR
		RMDir /r $APPDATA\Synthesis
		RMDir /r $APPDATA\BXD_Aardvark
		RMDir /r $PROGRAMFILES\Autodesk\Synthesis

        DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis"
		DeleteRegKey HKCU "SOFTWARE\Autodesk\Synthesis"
		;DeleteRegKey HKCU "SOFTWARE\Autodesk\BXD Synthesis"

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

  File /r "Synthesis\*"

  SetOutPath $INSTDIR
  
  CreateShortCut "$SMPROGRAMS\Synthesis.lnk" "$INSTDIR\Synthesis\Synthesis.exe"
  CreateShortCut "$DESKTOP\Synthesis.lnk" "$INSTDIR\Synthesis\Synthesis.exe"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayName" "Autodesk Synthesis"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayIcon" "$INSTDIR\uninstall.exe"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "Publisher" "Autodesk"
  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "URLInfoAbout" "BXD.Autodesk.com/tutorials"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "DisplayVersion" "4.2.3" ;UPDATE ON RELEASE

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "UninstallString" "$\"$INSTDIR\uninstall.exe$\""


  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
	; Extract field files
	SetOutPath $APPDATA\Autodesk\Synthesis\Fields
	File /r "Fields\*"

SectionEnd

Section "MixAndMatch Files" MixMatch

  ; Set extraction path for Mix&Match files
  SetOutPath $APPDATA\Autodesk\Synthesis\MixAndMatch

  File /r "MixAndMatch\*"

SectionEnd

Section "Inventor Exporter Plugin" iExporter

  ; Set extraction path Inventor plugin directory
  SetOutPath $INSTDIR
  File /r "Exporter"
  
  SetOutPath $APPDATA\Autodesk\ApplicationPlugins
  File /r "Exporter\Autodesk.BxDRobotExporter.Inventor.addin"

SectionEnd

Section "Fusion Exporter Plugin" fExporter

  ; Set extraction path to Fusion plugin directories
  SetOutPath "$APPDATA\Autodesk\Autodesk Fusion 360\API\AddIns"
  File /r "FusionExporter"
  
  SetOutPath "$APPDATA\Autodesk\ApplicationPlugins\FusionSynth.bundle\Contents\"
  File /r "FusionExporter\FusionSynth.dll"

SectionEnd

Section "Robot Files" RobotFiles

  ; Set extraction path for preloaded robot files
  SetOutPath $APPDATA\Autodesk\Synthesis\Robots

  File /r "Robots\*"

SectionEnd

Section "Code Emulator" Emulator

	; INetC.dll must be installed to proper NSIS Plugins x86 directories
	inetc::get "https://github.com/docker/toolbox/releases/download/v18.09.3/DockerToolbox-18.09.3.exe" "$PLUGINSDIR\DockerToolbox-18.09.3.exe"
	Pop $R0 ;Get the return value
	
	${If} $R0 == "OK"  ;Return value should be "OK"
	  HideWindow
	  ExecWait '"$PLUGINSDIR\DockerToolbox-18.09.3.exe" /SILENT'
	  ExecWait '"$PROGRAMFILES64\Docker Toolbox\docker.exe" run -d -p 50051:50051 -p 10022:10022 -p 10023:10023 hel'
	  ShowWindow hwnd show_state
	${Else}
	  MessageBox mb_iconstop "Error: $R0" ;Show cancel/error message
	${EndIf}
		
SectionEnd

;--------------------------------
;Component Descriptions

  LangString DESC_SynthesisRequired ${LANG_ENGLISH} "The Unity5 Simulator Engine is what the exported fields and robots are loaded into. In real-time, it simulates a real world physics environment for robots to interact with fields or other robots"
  LangString DESC_MixMatch ${LANG_ENGLISH} "Mix and Match will allow the user to quickly choose from pre-configured robot parts such as wheels, drive bases and manipulators within the simulator"
  LangString DESC_iExporter ${LANG_ENGLISH} "The Robot Exporter Plugin is an Inventor addin used to export Autodesk Inventor Assemblies directly into the simulator"
  LangString DESC_fExporter ${LANG_ENGLISH} "The Fusion Exporter Plugin is a Fusion addin used to export Autodesk Fusion Assemblies directly into the simulator"
  LangString DESC_RobotFiles ${LANG_ENGLISH} "A library of sample robots pre-loaded into the simulator"
  LangString DESC_Emulator ${LANG_ENGLISH} "The Robot Code Emulator allows you to emulate your C++ & JAVA robot code in the simulator (Installs Docker Toolbox)"

  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SynthesisRequired} $(DESC_SynthesisRequired)
  !insertmacro MUI_DESCRIPTION_TEXT ${MixMatch} $(DESC_MixMatch)
  !insertmacro MUI_DESCRIPTION_TEXT ${iExporter} $(DESC_iExporter)
  !insertmacro MUI_DESCRIPTION_TEXT ${fExporter} $(DESC_fExporter)
  !insertmacro MUI_DESCRIPTION_TEXT ${RobotFiles} $(DESC_RobotFiles)
  !insertmacro MUI_DESCRIPTION_TEXT ${Emulator} $(DESC_Emulator)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END
  
;--------------------------------
  
Section "Uninstall"

  MessageBox MB_YESNO "Would you like to remove your robot/replay files?" IDNO NawFam
  RMDir /r /REBOOTOK $APPDATA\Synthesis
  RMDir /r /REBOOTOK $APPDATA\Autodesk\Synthesis
  
  NawFam:
  ; Remove registry keys
  DeleteRegKey HKLM SOFTWARE\Synthesis
  DeleteRegKey HKCU SOFTWARE\Autodesk\Synthesis
  DeleteRegKey HKCU "SOFTWARE\Autodesk\BXD Synthesis"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis"

  RMDir /r /REBOOTOK $INSTDIR
  RMDir /r /REBOOTOK $PROGRAMFILES\Autodesk\Synthesis
  RMDir /r /REBOOTOK $APPDATA\BXD_Aardvark
  RMDir /r /REBOOTOK $APPDATA\SynthesisTEMP
  
  ; Remove outdated bxd inventor plugins
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2020\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2019\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2019\Addins\autodesk.BxDFieldExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDFieldExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDFieldExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\ApplicationPlugins\Autodesk.BxDRobotExporter.Inventor.addin"
  
  ; Remove excess shortcuts
  Delete "$SMPROGRAMS\Synthesis.lnk"
  Delete "$DESKTOP\Synthesis.lnk"
  Delete "$SMPROGRAMS\BXD Synthesis.lnk"
  Delete "$DESKTOP\BXD Synthesis.lnk"
  Delete "$SMPROGRAMS\Autodesk Synthesis.lnk"
  Delete "$DESKTOP\Autodesk Synthesis.lnk"
  
  IfFileExists "$PROGRAMFILES64\qemu" file_found uninstall_complete
  
	file_found:
	MessageBox MB_YESNO "Would you like to uninstall QEMU as well?" IDNO uninstall_complete
	exec '"$PROGRAMFILES64\qemu\qemu-uninstall.exe" \s'
	Quit
	
	uninstall_complete:

SectionEnd