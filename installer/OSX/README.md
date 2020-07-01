# OSX packager

## Build step :

1. Get signed Synthesis.app

2. Make new directory ` mkdir [synthesis/installer/osx/application/.Assets] `

3. Copy Synthesis.app to synthesis macos install application ` cp [Synthesis.app] [synthesis/installer/osx/applcation/.Assets] `

4. Add data files to synthesis/installer/osx/application/.Assets/Synthesis
	
	- Add Environments to synthesis/installer/osx/application/.Assets/Synthesis/Environments
	- Add Robots to synthesis/installer/osx/application/.Assets/Synthesis/Robots
	- Add Modules to synthesis/installer/osx/application/.Assets/Synthesis/Modules

5. Install Node / NPM ` brew install node `

6. Install AppDMG ` npm i -g appdmg `

7. change directories to the osx/application directory ` cd synthesis/installer/osx/application/ `

8. Run ` appdmg spec.json Synthesis.app `

### Package

Publish the newly created Synthesis.dmg
