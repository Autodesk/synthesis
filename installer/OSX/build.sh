#!/bin/bash

FUSION_ADDIN_LOCATION="Support/Autodesk/Autodesk\ Fusion\ 360/API/AddIns/"
EXPORTER_SOURCE_DIR=../../exporter/
FUSION_ADDIN_LOCATION=~/Documents/

pkgbuild --root $EXPORTER_SOURCE_DIR --identifier com.Autodesk.Synthesis --version 1.0 --scripts Scripts --install-location $FUSION_ADDIN_LOCATION MyApp.pkg
productbuild --distribution distribution.xml --package-path . MyAppInstaller.pkg
