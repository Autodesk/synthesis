# OSX packager

## Build steps :

1. Get signed Synthesis.app

2. Zip Synthesis.app

3. Copy Synthesis.zip to file system ` cp [Synthesis.zip] [synthesis/installer/OSX/App/payload/Contents] `

    - Remove the Synthesis.zip placeholder ` rm [synthesis/installer/OSX/App/payload/Contents/README.md] `

3. Add data files to synthesis/installer/OSX/Assets/payload/Contents/Synthesis

    - Remove the data file placeholder ` rm [synthesis/installer/OSX/Assets/payload/Contents/Synthesis/README.md] `	

4. Add unzipped exporter files to synthesis/installer/OSX/Exporter/payload/Contents/SynthesisFusionGltfExporter
  
    - Remove the exporter file placeholder ` rm [synthesis/installer/OSX/Exporter/payload/Contents/SynthesisFusionGltfExporter/README.md] `	

5. Change directories to the osx installer directory ` cd synthesis/installer/OSX `

6. Run the pkginstall script ` ./pkginstall `

### Optional Build Steps

Update the license, welcome and conclusion installer menus located in synthesis/installer/OSX/Installer/Resources

## Package

Publish the newly created Synthesis.pkg

## Important Note

**Do not** rename or move files

