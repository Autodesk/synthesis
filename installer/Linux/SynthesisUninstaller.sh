#!/bin/bash
#sed -i -e 's/\r$//' SynthesisUninstaller.sh

sudo rm -rf $HOME/.config/Autodesk/Synthesis
sudo rm -rf $HOME/.config/unity3d/Autodesk
sudo rm -rf $ROOT/usr/share/Autodesk/Synthesis

sudo rm $ROOT/usr/share/pixmaps/synthesis.png
sudo rm $ROOT/usr/share/applications/Synthesis.desktop

sudo apt-get remove --auto-remove qemu
sudo apt-get purge --auto-remove qemu

echo "Autodesk Synthesis Has Been Uninstalled"