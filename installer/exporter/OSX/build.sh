#!/bin/bash

FUSION_ADDIN_LOCATION=~/Library/Application\ Support/Autodesk/ApplicationPlugins/
EXPORTER_SOURCE_DIR=../../../exporter/SynthesisFusionAddin/

mkdir -p tmp/
cp -r synthesis.bundle tmp/
rsync -av ../synthesis.bundle tmp/
cp -r "$EXPORTER_SOURCE_DIR"/* tmp/synthesis.bundle/Contents/

pkgbuild --root tmp/ --identifier com.Autodesk.Synthesis --scripts Scripts/ --version 2.0.0 --install-location "$FUSION_ADDIN_LOCATION" SynthesisExporter.pkg
productbuild --distribution distribution.xml --package-path . SynthesisExporterInstaller.pkg

rm SynthesisExporter.pkg
rm -r tmp/
