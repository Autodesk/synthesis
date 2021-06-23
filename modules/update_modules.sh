#/bin/bash -e

# TODO This is a work in progress

MODULES_DIR=~/.config/autodesk/synthesis/modules
SYNTHESIS_DIR=$(pwd)

mkdir -p $MODULES_DIR
cd $MODULES_DIR

mkdir -p Controller
cp -r $SYNTHESIS_DIR/Controller/Assets/* Controller/
cp $SYNTHESIS_DIR/Controller/bin/Debug/netstandard2.0/Controller.dll Controller/

mkdir -p SynthesisCore
cp -r $SYNTHESIS_DIR/SynthesisCore/Assets/* SynthesisCore/
cp $SYNTHESIS_DIR/SynthesisCore/bin/Debug/netstandard2.0/SynthesisCore.dll SynthesisCore/

rm -f Controller.zip SynthesisCore.zip

zip -r Controller.zip Controller
zip -r SynthesisCore.zip SynthesisCore
