#!/bin/sh
test -f Synthesis-Installer.dmg && rm Synthesis-Installer.dmg
./create-dmg/create-dmg \
  --volname "Synthesis Installer" \
  --window-pos 200 120 \
  --window-size 500 180 \
  --text-size 12 \
  --icon-size 80 \
  --icon "Synthesis.app" 50 50 \
  --hide-extension "Synthesis.app" \
  --app-drop-link 360 50 \
  --eula "license.txt" \
  "Synthesis-Installer.dmg" \
  "source_folder/"

# --volicon "synthesis-icon.icns" \
# --background "installer_background.png" \
