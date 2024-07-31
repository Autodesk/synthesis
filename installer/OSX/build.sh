#!/bin/bash

FUSION_ADDIN_LOCATION=~/Library/Application\ Support/Autodesk/Autodesk\ Fusion\ 360/API/AddIns/
EXPORTER_SOURCE_DIR=../../exporter/SynthesisFusionAddin/

mkdir -p tmp/
cp -r "$EXPORTER_SOURCE_DIR"/* tmp/

pkgbuild --root tmp/ --identifier com.Autodesk.Synthesis --version 1.0 --install-location "$FUSION_ADDIN_LOCATION" MyApp.pkg
productbuild --distribution distribution.xml --package-path . MyAppInstaller.pkg

rm -r tmp/
