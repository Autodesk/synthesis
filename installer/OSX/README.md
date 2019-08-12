# OSX packager

## Build step (temp fix) :

1. Get signed Synthesis.app

2. Copy to synthesis macos install application ` cp [Synthesis.app] [syntheis/installer/osx/applcation/] `

3. Install Node / NPM ` brew install node `

4. Install AppDMG ` npm i -g appdmg `

5. change directories to the osx/application directory ` cd synthesis/installer/osx/application/ `

6. Run ` appdmg spec.json Synthesis.app `

### Package

1. Make a new folder 

2. Put the now built Synthesis.dmg into the folder

3. Copy post-install.sh into the new folder

4. Copt instructions.txt into the new folder

5. Zip that folder and publish


## Build steps (Not Working) :

0.5 Copy Synthesis build to application folder - ` cp [synthesis/build/dir/Synthesis.app] [syntheis/installer/osx/applcation/] `

1. ` cd installer/osx `

2. ` ./build-macos-x64.sh [synthesis version] `

3. Type N + enter when asked to sign package (for now)

## Builds installer to

` synthesis/installer/osx/target/pkg `

## Structure

    |   - build-macos-x64.sh
    |   - darwin
        |   - Resources
            |   - Logos (Banner image)
            |   - HTML (Welcome/Conclusion)
            |   - Uninstall.sh
        |   - Scripts
            |   - postinstall.sh
        |   - Distribution (Linker for everything else in this directory)
    |   - application
        |   - (Synthesis build goes here)
    | utils
        |   - ascii_art.txt (Cool Synthesis Logo)

## Acknowledgments

This awesome script more or less came from a cool dude at [https://medium.com/swlh/the-easiest-way-to-build-macos-installer-for-your-application-34a11dd08744]medium.