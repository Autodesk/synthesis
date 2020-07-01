## Build steps (Not Working) :

1. Get signed Synthesis app

2. Copy Synthesis build to application folder - ` cp [synthesis/build/dir/Synthesis.app] [syntheis/installer/osx/applcation/] `

3. ` cd installer/osx `

4. ` ./build-macos-x64.sh [synthesis version] `

5. Type N + enter when asked to sign package (for now)

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
