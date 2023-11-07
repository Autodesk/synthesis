#!/bin/sh
test -f Synthesis-Exporter-Installer.dmg && rm Synthesis-Exporter-Installer.dmg
./create-dmg/create-dmg \
  --volname "Synthesis Exporter Installer" \
  --background "Synthesis-Background.png" \
  --window-pos 200 120 \
  --window-size 500 270 \
  --text-size 12 \
  --icon-size 80 \
  --icon "applink/" 120 115 \
  --hide-extension "Synthesis.app" \
  --eula "license.txt" \
  "Synthesis-Exporter-Installer.dmg" \
  "exporter_source_folder/"

# --volicon "synthesis-icon.icns" \
# --background "installer_background.png" \
