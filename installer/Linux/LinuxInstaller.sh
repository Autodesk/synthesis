#!/bin/bash
#sed -i -e 's/\r$//' LinuxInstaller.sh
VERSION="4.2.3"
source ./progress_bar.sh
sudo echo "Installing Autodesk Synthesis $VERSION For Linux (Beta)"

enable_trapping
setup_scroll_area
draw_progress_bar 0
echo "Creating Directories..."
sudo mkdir -p $HOME/.config/Autodesk/Synthesis
draw_progress_bar 10
sudo mkdir -p $ROOT/usr/share/Autodesk
draw_progress_bar 20
echo "Initializing Directory Permissions..."
sudo chown -R $USER: $HOME/.config/Autodesk
draw_progress_bar 30
sudo chown -R $USER: $HOME/.config/Autodesk/Synthesis
draw_progress_bar 40
echo "Copying Robot Files..."
sudo rsync -a --info=progress2 Robots $HOME/.config/Autodesk/Synthesis
draw_progress_bar 50
echo "Copying Field Files..."
sudo rsync -a --info=progress2 Fields $HOME/.config/Autodesk/Synthesis
draw_progress_bar 60
echo "Copying Program Files..."
sudo rsync -a --info=progress2 Synthesis $ROOT/usr/share/Autodesk
draw_progress_bar 70
sudo rsync -a --info=progress2 synthesis.png $ROOT/usr/share/pixmaps
draw_progress_bar 80
sudo rsync -a --info=progress2 Synthesis.desktop $ROOT/usr/share/applications
draw_progress_bar 90
sudo chmod +x $ROOT/usr/share/Autodesk/Synthesis/Synthesis.x86_64
block_progress_bar 100
destroy_scroll_area
echo "Autodesk Synthesis $VERSION For Linux (Beta) Installation Complete"
