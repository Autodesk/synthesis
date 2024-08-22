# Synthesis Exporter Installers

This `readme` is for developers of Synthesis or those looking to build the installers themselves, if you are just looking for how to install our Fusion Exporter please navigate to [`/installer/`](../).

## Windows

The windows installer has the following prerequisites:
- Python `3.9` or newer and pip.
- And that's it!

### To Build:

Once you have verified that python and pip are installed on your computer:
- Open a powershell window and navigate to [`/installer/exporter/Windows/`]
- Run `./build.bat` in powershell.
- After some time you should see `installer.exe` in your current directory.
- And that's it! You have now built the Synthesis Exporter Installer for Windows!
- You can then run the `.exe` from file explorer or alternatively, for debugging purposes, run `./installer.exe` from the terminal.

## MacOS

The Mac installer has zero prerequisites. Hooray!

### To Build:

- Navigate to [`/installer/exporter/OSX/`](./OSX/).
- Run `./build.sh` in your terminal.
- You should then find `SynthesisExporterInstaller.pkg` in your current directory.
- And that's it! You now have built the Synthesis Exporter Installer for MacOS!
