#!/bin/sh
if ! test -f addins-folder-link; then
  ln -s ~/Library/Application\ Support/Autodesk/Autodesk\ Fusion\ 360/API/AddIns addins-folder-link
fi

test -f Synthesis-Installer.dmg && rm Synthesis-Installer.dmg
./create-dmg/create-dmg \
  --volname "Synthesis Installer" \
  --background "SynthesisMacInstallerBackground.png" \
  --window-pos 200 120 \
  --window-size 375 320 \
  --text-size 12 \
  --icon-size 50 \
  --icon "Synthesis.app" 80 80 \
  --add-file AddIns addins-folder-link 280 190 \
  --add-file Exporter ../../exporter/SynthesisFusionAddin 80 190 \
  --hide-extension "Synthesis.app" \
  --app-drop-link 280 80 \
  --eula "license.txt" \
  --text-size 10 \
  "Synthesis-Installer.dmg" \
  "source_folder/"

# --volicon "synthesis-icon.icns" \
# --background "installer_background.png" \
# --background "Synthesis-Background.png" \
