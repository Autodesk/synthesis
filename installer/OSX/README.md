# OSX packager

## Build step :

1. Get signed Synthesis.app

2. Copy to synthesis macos install application ` cp [Synthesis.app] [synthesis/installer/osx/applcation/.Assets] `

3. Add data files to synthesis/installer/osx/application/.Assets/Synthesis

3. Install Node / NPM ` brew install node `

4. Install AppDMG ` npm i -g appdmg `

5. change directories to the osx/application directory ` cd synthesis/installer/osx/application/ `

6. Run ` appdmg spec.json Synthesis.app `

### Package

1. Publish the newly created Synthesis.dmg
