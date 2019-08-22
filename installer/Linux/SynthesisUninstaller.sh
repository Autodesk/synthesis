#!/bin/bash
#sed -i -e 's/\r$//' SynthesisUninstaller.sh

read -r -p "Would you like to uninstall QEMU as well? [ Y / N ] " response
if [[ "$response" =~ ^([yY][eE][sS]|[yY])+$ ]]
then
	sudo apt-get remove --auto-remove qemu
	sudo apt-get purge --auto-remove qemu
fi

sudo rm -rf $HOME/.config/Autodesk/Synthesis
sudo rm -rf $HOME/.config/unity3d/Autodesk
sudo rm -rf $ROOT/usr/share/Autodesk/Synthesis

sudo rm $ROOT/usr/share/pixmaps/synthesis.png
sudo rm $ROOT/usr/share/applications/Synthesis.desktop

echo "Autodesk Synthesis Has Been Uninstalled"