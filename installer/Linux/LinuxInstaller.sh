#!/bin/bash
#sed -i -e 's/\r$//' LinuxInstaller.sh

VERSION="5.0.0B"
source ./ProgressBar.sh
sudo echo "Installing Autodesk Synthesis $VERSION For Linux"
echo "This software is licensed under the terms and conditions of the Apache 2.0 License."

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
echo "Copying Environment Files..."
sudo rsync -a --info=progress2 Environments $HOME/.config/Autodesk/Synthesis
draw_progress_bar 40
echo "Copying Program Files..."
sudo rsync -a --info=progress2 Synthesis $ROOT/usr/share/Autodesk
draw_progress_bar 50
sudo rsync -a --info=progress2 synthesis.png $ROOT/usr/share/pixmaps
draw_progress_bar 60
sudo rsync -a --info=progress2 Synthesis.desktop $ROOT/usr/share/applications
draw_progress_bar 70
sudo rsync -a --info=progress2 SynthesisUninstaller.sh $HOME/.config/Autodesk/Synthesis
chmod +x $HOME/.config/Autodesk/Synthesis/SynthesisUninstaller.sh
draw_progress_bar 80
sudo chmod +x $ROOT/usr/share/Autodesk/Synthesis/Synthesis.x86_64
block_progress_bar 90

read -r -p "Would you like to install the Controller for robot code as well? [ Y / N ] " response
if [[ "$response" =~ ^([yY][eE][sS]|[yY])+$ ]]
then
	echo "Copying Controller Files"
	sudo rsync -a --info=progress2 Controller $HOME/.config/Autodesk/Synthesis/Modules
	draw_progress_bar 100
fi

destroy_scroll_area
echo "Autodesk Synthesis $VERSION For Linux Installation Complete"
