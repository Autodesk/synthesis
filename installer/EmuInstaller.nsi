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

;--------------------------------
;Installer Sections

Section

  IfFileExists "$PROGRAMFILES\Autodesk\Synthesis\Synthesis\Synthesis.exe" file_found file_not_found

  file_found: goto perform_install
  
  file_not_found:
  
	MessageBox MB_YESNO "It appears that you do not have Synthesis installed. Would you like to download it now?" IDNO NoDownload
      ExecShell "open" "http://bxd.autodesk.com/download.html"
	  ExecShell "open" "http://synthesis.autodesk.com/Downloadables/Synthesis%20Installer.exe"
	  
	  Quit
	  
    NoDownload:
  
	perform_install:
	
	  SetOutPath $INSTDIR
  
	  File /r "Emulator\rootfs.ext4"
	  File /r "Emulator\zImage"
	  File /r "Emulator\zynq-zed.dtb"
	  
	  ;NSISdl::download "https://qemu.weilnetz.de/w64/qemu-w64-setup-20180725.exe" "$INSTDIR\qemu-w64-setup-20180725.exe"
	  ${If} ${RunningX64}
		File /r "Emulator\qemu-w64-setup-20180519.exe"
		exec '"$APPDATA\Autodesk\Synthesis\Emulator\qemu-w64-setup-20180519.exe" \s'
	  ${Else}
		File /r "Emulator\qemu-w32-setup-20180519.exe"
		exec '"$APPDATA\Autodesk\Synthesis\Emulator\qemu-w32-setup-20180519.exe" \s'
	  ${EndIf}
	
	  Quit

SectionEnd