#!/bin/bash
#sed -i -e 's/\r$//' LinuxInstaller.sh

VERSION="4.3.0"
source ./ProgressBar.sh
sudo echo "Installing Autodesk Synthesis $VERSION For Linux"

enable_trapping
setup_scroll_area
draw_progress_bar 0
echo "Creating Directories..."
sudo mkdir -p $HOME/.config/Autodesk/Synthesis
draw_progress_bar 5
sudo mkdir -p $ROOT/usr/share/Autodesk
draw_progress_bar 10
echo "Initializing Directory Permissions..."
sudo chown -R $USER: $HOME/.config/Autodesk
draw_progress_bar 15
sudo chown -R $USER: $HOME/.config/Autodesk/Synthesis
draw_progress_bar 20
echo "Copying Robot Files..."
sudo rsync -a --info=progress2 Robots $HOME/.config/Autodesk/Synthesis
draw_progress_bar 30
echo "Copying MixAndMatch Files..."
sudo rsync -a --info=progress2 MixAndMatch $HOME/.config/Autodesk/Synthesis
draw_progress_bar 40
echo "Copying Field Files..."
sudo rsync -a --info=progress2 Fields $HOME/.config/Autodesk/Synthesis
draw_progress_bar 50
echo "Copying Program Files..."
sudo rsync -a --info=progress2 Synthesis $ROOT/usr/share/Autodesk
draw_progress_bar 60
sudo rsync -a --info=progress2 synthesis.png $ROOT/usr/share/pixmaps
draw_progress_bar 65
sudo rsync -a --info=progress2 Synthesis.desktop $ROOT/usr/share/applications
draw_progress_bar 70
sudo rsync -a --info=progress2 SynthesisUninstaller.sh $HOME/.config/Autodesk/Synthesis
chmod +x $HOME/.config/Autodesk/Synthesis/SynthesisUninstaller.sh
draw_progress_bar 75
sudo chmod +x $ROOT/usr/share/Autodesk/Synthesis/Synthesis.x86_64
block_progress_bar 80

read -r -p "Would you like to install the Code Emulator as well? [ Y / N ] " response
if [[ "$response" =~ ^([yY][eE][sS]|[yY])+$ ]]
then
	echo "Copying Emulator Files..."
	rsync --info=progress2 Emulator $HOME/.config/Autodesk/Synthesis
	draw_progress_bar 90

	echo "Installing QEMU..."

	APT_GET_CMD=$(which apt-get)
	YUM_CMD=$(which yum)
	PACMAN_CMD=$(which pacman)
	ZYPPER_CMD=$(which zypper)
	DNF_CMD=$(which dnf)

	if [[ ! -z $APT_GET_CMD ]]; then
		sudo apt-get install qemu
	elif [[ ! -z $YUM_CMD ]]; then
		sudo yum install qemu-kvm
	elif [[ ! -z $PACMAN_CMD ]]; then
		sudo pacman -S qemu
	elif [[ ! -z $ZYPPER_CMD ]]; then
		sudo zypper install qemu
	elif [[ ! -z $DNF_CMD ]]; then
		sudo dnf install @virtualization
	else
    echo "Failed to install QEMU. Install it manually from: https://www.qemu.org/download/"
    exit 1;
	fi
fi

draw_progress_bar 100
destroy_scroll_area
echo "Autodesk Synthesis $VERSION For Linux Installation Complete"