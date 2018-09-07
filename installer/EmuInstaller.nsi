  !include LogicLib.nsh
  !include x64.nsh
  
  Name "Synthesis Emulator Installation"
  
  OutFile "SynthesisEmuInstaller4.2.1.exe"

  ;Default installation folder
  InstallDir $APPDATA\Autodesk\Synthesis\Emulator

  ;Get installation folder from registry if available
  InstallDirRegKey HKLM "Software\Synthesis\Emulator" "Install_Dir"

  ;Request application privileges
  RequestExecutionLevel admin

Section

  ${If} ${RunningX64}
	goto install_stuff
  ${Else}
	MessageBox MB_OK "Sorry, but feature is compatible with 64-bit systems only."
	Quit
  ${EndIf}

  install_stuff:

  IfFileExists "$PROGRAMFILES64\Autodesk\Synthesis\Synthesis\Synthesis.exe" file_found file_not_found

  file_found: goto perform_install
  
  file_not_found:
  
	MessageBox MB_YESNO "It appears that you do not have Synthesis installed. Would you like to download it now?" IDNO NoDownload
      ExecShell "open" "http://synthesis.autodesk.com/download.html"
	  ExecShell "open" "http://synthesis.autodesk.com/Downloadables/Synthesis%20Installer.exe"
	  
	  Quit
	  
    NoDownload:
  
	perform_install:
	
	  SetOutPath $INSTDIR
  
	  File /r "Emulator\rootfs.ext4"
	  File /r "Emulator\zImage"
	  File /r "Emulator\zynq-zed.dtb"
	  File /r "Emulator\qemu-w64-setup-20180519.exe"
	  exec '"$APPDATA\Autodesk\Synthesis\Emulator\qemu-w64-setup-20180519.exe" \s'
	
	  Quit

SectionEnd