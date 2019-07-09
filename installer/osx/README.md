# OSX packager

## Build steps:

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
    |   - application
        |   - (Synthesis build goes here)