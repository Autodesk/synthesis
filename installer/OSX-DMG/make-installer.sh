#!/bin/sh
if ! test -f addins-folder-link; then
  ln -s ~/Library/Application\ Support/Autodesk/Autodesk\ Fusion\ 360/API/AddIns addins-folder-link
fi

test -f Synthesis-Installer.dmg && rm Synthesis-Installer.dmg
./create-dmg/create-dmg \
  --volname "Synthesis Installer" \
  --background "SynthesisMacInstallerBackground.png" \
  --window-pos 200 120 \
  --window-size 500 600 \
  --text-size 12 \
  --icon-size 80 \
  --icon "Synthesis.app" 120 115 \
  --add-file AddIns addins-folder-link 380 0 \
  --add-file SynthesisFusionAddin ../../exporter/SynthesisFusionAddin 120 0 \
  --hide-extension "Synthesis.app" \
  --app-drop-link 380 115 \
  --eula "license.txt" \
  "Synthesis-Installer.dmg" \
  "source_folder/"

# --volicon "synthesis-icon.icns" \
# --background "installer_background.png" \
# --background "Synthesis-Background.png" \
