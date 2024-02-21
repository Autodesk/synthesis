#!/bin/bash

if test -d ./Synthesis; then
	rm -rf ./Synthesis
fi

cp -r ../../exporter/SynthesisFusionAddin/ ./Synthesis
echo "Copied over Synthesis exporter!"

zip -r SynthesisExporter Synthesis/ install.sh
echo "Created zip installer!"
