$MODULES_DIR = $env:APPDATA + "\Autodesk\Synthesis\modules"
$SYNTHESIS_DIR = (Get-Item .).FullName

New-Item $MODULES_DIR -ItemType Directory -ErrorAction SilentlyContinue
CD $MODULES_DIR

New-Item Controller -ItemType Directory -ErrorAction SilentlyContinue
cp -r $SYNTHESIS_DIR/Controller/Assets/* Controller/ -Force
cp -r $SYNTHESIS_DIR/Controller/bin/Debug/netstandard2.0/Controller.dll Controller/ -Force -ErrorAction SilentlyContinue

New-Item SynthesisCore -ItemType Directory -ErrorAction SilentlyContinue
cp -r $SYNTHESIS_DIR/SynthesisCore/Assets/* SynthesisCore/ -Force -ErrorAction SilentlyContinue
cp -r $SYNTHESIS_DIR/SynthesisCore/bin/Debug/netstandard2.0/SynthesisCore.dll SynthesisCore/ -Force -ErrorAction SilentlyContinue

rm Controller.zip -Force -ErrorAction SilentlyContinue
rm SynthesisCore.zip -Force -ErrorAction SilentlyContinue

zip -r Controller.zip Controller
zip -r SynthesisCore.zip SynthesisCore
