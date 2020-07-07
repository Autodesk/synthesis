# OSX packager

## Build steps :

1. Get signed Synthesis.app

2.  Synthesis.app ` cp [Synthesis.app] [synthesis/installer/OSX/App/payload] `

    - Remove the Synthesis.app placeholder ` rm [synthesis/installer/OSX/App/payload/README.md] `

3. Add data files to synthesis/installer/OSX/Assets/payload/Contents/Synthesis

    - Remove the data file placeholder ` rm [synthesis/installer/OSX/Assets/payload/Contents/Synthesis/README.md] `	
  
4. Change directories to the osx installer directory ` cd synthesis/installer/OSX `

5. Run the pkginstall script ` ./pkginstall `

### Optional Build Steps

Update the license, welcome and conclusion installer menus located in synthesis/installer/OSX/Installer/Resources

## Package

Publish the newly created Synthesis.pkg

## Important Note

**Do not** rename or move files

