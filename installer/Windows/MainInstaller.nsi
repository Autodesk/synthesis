!include MUI2.nsh
!include x64.nsh
!define PRODUCT_VERSION "5.0.0"

Name "Synthesis"

Icon "W16_SYN_launch.ico"

Caption "Synthesis ${PRODUCT_VERSION} Setup"

OutFile "SynthesisWin${PRODUCT_VERSION}.exe"

InstallDir $PROGRAMFILES64\Autodesk\Synthesis

InstallDirRegKey HKLM "Software\Synthesis" "Install_Dir"

RequestExecutionLevel admin

  Section
  ${If} ${RunningX64}
	goto install_stuff
  ${Else}
	MessageBox MB_OK "ERROR: This install requires a 64 bit system."
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
  !define MUI_FINISHPAGE_LINK_LOCATION "https://synthesis.autodesk.com/tutorials.html"
  
;--------------------------------

  ; Installer GUI Pages
  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "..\..\LICENSE.txt"
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

IfFileExists "$APPDATA\Autodesk\Synthesis" +1 +28
    MessageBox MB_YESNO "You appear to have Synthesis installed; would you like to reinstall it?" IDYES true IDNO false
      true:
        DeleteRegKey HKLM SOFTWARE\Synthesis
		
		; Remove fusion plugins
		RMDir /r "$APPDATA\Autodesk\Autodesk Fusion 360\API\AddIns\FusionRobotExporter"
		RMDir /r "$APPDATA\Autodesk\Autodesk Fusion 360\API\AddIns\Synthesis"
		RMDir /r "$APPDATA\Autodesk\Autodesk Fusion 360\API\AddIns\FusionExporter"
		RMDir /r "$APPDATA\Autodesk\ApplicationPlugins\FusionRobotExporter.bundle"
		RMDir /r "$APPDATA\Autodesk\ApplicationPlugins\FusionSynth.bundle"

	    ; Remove inventor plugins
		Delete "$APPDATA\Autodesk\Inventor 2021\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
	    Delete "$APPDATA\Autodesk\Inventor 2020\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
	    Delete "$APPDATA\Autodesk\Inventor 2019\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
	    Delete "$APPDATA\Autodesk\Inventor 2018\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
	    Delete "$APPDATA\Autodesk\Inventor 2017\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
	    Delete "$APPDATA\Autodesk\ApplicationPlugins\Autodesk.InventorRobotExporter.Inventor.addin"
		RMDir /r "$APPDATA\Autodesk\ApplicationPlugins\InventorRobotExporter"
  
		; Remove deprecated bxd inventor plugins
		Delete "$APPDATA\Autodesk\Inventor 2020\Addins\autodesk.BxDRobotExporter.inventor.addin"
        Delete "$APPDATA\Autodesk\Inventor 2019\Addins\autodesk.BxDRobotExporter.inventor.addin"
		Delete "$APPDATA\Autodesk\Inventor 2019\Addins\autodesk.BxDFieldExporter.inventor.addin"
		Delete "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDRobotExporter.inventor.addin"
		Delete "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDFieldExporter.inventor.addin"
        Delete "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDRobotExporter.inventor.addin"
        Delete "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDFieldExporter.inventor.addin"
		Delete "$APPDATA\Autodesk\ApplicationPlugins\Autodesk.BxDRobotExporter.Inventor.addin"
		RMDir /r "$APPDATA\Autodesk\ApplicationPlugins\BxDRobotExporter"
        RMDIR /r $APPDATA\RobotViewer

        ; Remove excess shortcuts
        Delete "$SMPROGRAMS\Synthesis.lnk"
        Delete "$DESKTOP\Synthesis.lnk"
		Delete "$SMPROGRAMS\BXD Synthesis.lnk"
        Delete "$DESKTOP\BXD Synthesis.lnk"
		Delete "$SMPROGRAMS\Autodesk Synthesis.lnk"
        Delete "$DESKTOP\Autodesk Synthesis.lnk"
		Delete "$DESKTOP\FieldExporter.lnk"
        
        ; Remove obsolete directories
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

Section "Synthesis (required)" Synthesis

  SectionIn RO

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR

  File /r "Synthesis\*"
  
  CreateShortCut "$SMPROGRAMS\Synthesis.lnk" "$INSTDIR\Synthesis.exe"
  CreateShortCut "$DESKTOP\Synthesis.lnk" "$INSTDIR\Synthesis.exe"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayName" "Autodesk Synthesis"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "DisplayIcon" "$INSTDIR\uninstall.exe"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "Publisher" "Autodesk"
  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                "URLInfoAbout" "synthesis.autodesk.com/tutorials"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "DisplayVersion" "${PRODUCT_VERSION}"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis" \
                 "UninstallString" "$\"$INSTDIR\uninstall.exe$\""


  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Synthesis" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
	; Set extraction path for field files
	SetOutPath $APPDATA\Autodesk\Synthesis\Fields
	File /r "Fields\*"

SectionEnd

/*
Section "Inventor Exporter Plugin" iExporter

  ; Set extraction path to Inventor plugin directory
  SetOutPath $INSTDIR\Exporter
  File /r "InventorExporter\*"
  
  SetOutPath $APPDATA\Autodesk\ApplicationPlugins
  File /r "InventorExporter\Autodesk.InventorRobotExporter.Inventor.addin"

SectionEnd
*/

Section "Fusion Exporter Plugin" fExporter

  ; Set extraction path to Fusion plugin directories
  SetOutPath "$APPDATA\Autodesk\Autodesk Fusion 360\API\AddIns\Synthesis"
  File /r "..\..\exporter\SynthesisFusionAddin\*"
  
  ; SetOutPath "$APPDATA\Autodesk\ApplicationPlugins\FusionRobotExporter.bundle\Contents\"
  ; File /r "FusionExporter\FusionRobotExporter.dll"

SectionEnd

Section "Robots and Fields" RobotFiles

  ; Set extraction path for preloaded robot files
  SetOutPath $APPDATA\Autodesk\Synthesis\Mira
  File /r "Robots\*"

  SetOutPath $APPDATA\Autodesk\Synthesis\Mira\Fields
  File /r "Fields\*"

SectionEnd

/*
Section "Code Emulator" Emulator

	; INetC.dll must be installed to proper NSIS Plugins x86 directories
	inetc::get "https://qemu.weilnetz.de/w64/2019/qemu-w64-setup-20190724.exe" "$PLUGINSDIR\qemu-w64-setup-20190724.exe"
	Pop $R0 ;Get the return value
	
	${If} $R0 == "OK"  ;Return value should be "OK"
	SetOutPath $APPDATA\Autodesk\Synthesis\Emulator
	  File /r "Emulator\*"
	  HideWindow
	  ExecWait '"$PLUGINSDIR\qemu-w64-setup-20190724.exe" /SILENT'
	  ShowWindow hwnd show_state
	${Else}
	  MessageBox MB_ICONSTOP "Error: $R0" ;Show cancel/error message
	${EndIf}
		
SectionEnd
*/

;--------------------------------
;Component Descriptions

  LangString DESC_Synthesis ${LANG_ENGLISH} "The Simulator Engine is the real-time physics environment which simulates the robots and fields."
  ; LangString DESC_iExporter ${LANG_ENGLISH} "The Inventor Exporter Plugin is an Inventor addin used to export Autodesk Inventor Assemblies directly into the simulator"
  LangString DESC_fExporter ${LANG_ENGLISH} "The Fusion360 Exporter Plugin is a Fusion addin used to export Autodesk Fusion Assemblies directly into the simulator"
  LangString DESC_RobotFiles ${LANG_ENGLISH} "A library of sample robots and fields pre-loaded into the simulator"
  ; LangString DESC_Emulator ${LANG_ENGLISH} "The Robot Code Emulator allows you to emulate your C++ & JAVA robot code in the simulator"

  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${Synthesis} $(DESC_Synthesis)
  ; !insertmacro MUI_DESCRIPTION_TEXT ${iExporter} $(DESC_iExporter)
  !insertmacro MUI_DESCRIPTION_TEXT ${fExporter} $(DESC_fExporter)
  !insertmacro MUI_DESCRIPTION_TEXT ${RobotFiles} $(DESC_RobotFiles)
  ; !insertmacro MUI_DESCRIPTION_TEXT ${Emulator} $(DESC_Emulator)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END
  
;--------------------------------
  
Section "Uninstall"

  MessageBox MB_YESNO "Would you like to remove your robot files?" IDNO NawFam
  RMDir /r /REBOOTOK $APPDATA\Synthesis
  RMDir /r /REBOOTOK $APPDATA\Autodesk\Synthesis
  
  NawFam:
  ; Remove registry keys
  DeleteRegKey HKLM SOFTWARE\Synthesis
  DeleteRegKey HKCU SOFTWARE\Autodesk\Synthesis
  DeleteRegKey HKCU "SOFTWARE\Autodesk\BXD Synthesis"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Autodesk Synthesis"

  ; Remove installation directories
  RMDir /r /REBOOTOK $INSTDIR
  RMDir /r /REBOOTOK $PROGRAMFILES\Autodesk\Synthesis
  RMDir /r /REBOOTOK $APPDATA\BXD_Aardvark
  RMDir /r /REBOOTOK $APPDATA\SynthesisTEMP
  
  ; Remove fusion plugins
  RMDir /r "$APPDATA\Autodesk\Autodesk Fusion 360\API\AddIns\FusionRobotExporter"
  RMDir /r "$APPDATA\Autodesk\Autodesk Fusion 360\API\AddIns\Synthesis"
  RMDir /r "$APPDATA\Autodesk\Autodesk Fusion 360\API\AddIns\FusionExporter"
  RMDir /r "$APPDATA\Autodesk\ApplicationPlugins\FusionRobotExporter.bundle"
  RMDir /r "$APPDATA\Autodesk\ApplicationPlugins\FusionSynth.bundle"
  
  ; Remove inventor plugins
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2022\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2021\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2020\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2019\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\Autodesk.InventorRobotExporter.Inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\ApplicationPlugins\Autodesk.InventorRobotExporter.Inventor.addin"
  
  ; Remove deprecated bxd inventor plugins
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2020\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2019\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2019\Addins\autodesk.BxDFieldExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2018\Addins\autodesk.BxDFieldExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDRobotExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\Inventor 2017\Addins\autodesk.BxDFieldExporter.inventor.addin"
  Delete /REBOOTOK "$APPDATA\Autodesk\ApplicationPlugins\Autodesk.BxDRobotExporter.Inventor.addin"
  RMDir /REBOOTOK "$APPDATA\Autodesk\ApplicationPlugins\BxDRobotExporter"
  
  ; Remove excess shortcuts
  Delete "$SMPROGRAMS\Synthesis.lnk"
  Delete "$DESKTOP\Synthesis.lnk"
  Delete "$SMPROGRAMS\BXD Synthesis.lnk"
  Delete "$DESKTOP\BXD Synthesis.lnk"
  Delete "$SMPROGRAMS\Autodesk Synthesis.lnk"
  Delete "$DESKTOP\Autodesk Synthesis.lnk"
  Delete "$DESKTOP\FieldExporter.lnk"
  
  /*
  ; Execute QEMU uninstaller
  IfFileExists "$PROGRAMFILES64\qemu" file_found uninstall_complete
  
	file_found:
	MessageBox MB_YESNO "Would you like to uninstall QEMU as well?" IDNO uninstall_complete
	Exec '"$PROGRAMFILES64\qemu\qemu-uninstall.exe"'
	Quit
	
	uninstall_complete:
  */

SectionEnd

Function .OnInstSuccess
  Exec "$INSTDIR\Synthesis.exe"
FunctionEnd
