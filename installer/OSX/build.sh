#!/bin/bash

FUSION_ADDIN_LOCATION=~/Library/Application\ Support/Autodesk/Autodesk\ Fusion\ 360/API/AddIns/
EXPORTER_SOURCE_DIR=../../exporter/SynthesisFusionAddin/

mkdir -p tmp/
cp -r "$EXPORTER_SOURCE_DIR"/* tmp/

pkgbuild --root tmp/ --identifier com.Autodesk.Synthesis --version 2.0.0 --install-location "$FUSION_ADDIN_LOCATION" SynthesisExporter.pkg
productbuild --distribution distribution.xml --package-path . SynthesisExporterInstaller.pkg

rm SynthesisExporter.pkg
rm -r tmp/
