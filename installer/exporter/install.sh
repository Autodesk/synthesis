#!/bin/bash

if test -d ~/Library/Application\ Support/Autodesk/Autodesk\ Fusion\ 360/API/AddIns/SynthesisFusionAddin; then
	echo "Removing existing Synthesis exporter..."
	rm -rf ~/Library/Application\ Support/Autodesk/Autodesk\ Fusion\ 360/API/AddIns/SynthesisFusionAddin
fi

cp -r Synthesis/ ~/Library/Application\ Support/Autodesk/Autodesk\ Fusion\ 360/API/AddIns/SynthesisFusionAddin

echo "Synthesis successfully copied!"
