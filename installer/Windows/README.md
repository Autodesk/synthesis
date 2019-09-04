# <img src="https://raw.githubusercontent.com/Autodesk/synthesis/master/installer/Windows/orange-install-nsis.ico" alt="logo" width="50" height ="50" align="left"/>Nullsoft Scriptable Install System

For installation on Windows Operating Systems, we use our own custom written NSIS installer in order to extract all the necessary files to their proper locations on the system.

- [MainInstaller(x64)](https://github.com/Autodesk/synthesis/blob/master/installer/Windows/MainInstaller.nsi) - (Used for the full installation of Synthesis, only compatible on 64 bit operating systems.)
- [EngInstaller(x86)](https://github.com/Autodesk/synthesis/blob/master/installer/Windows/EngInstaller(x86).nsi) - (Used only for installation on 32 bit operating systems and extracts just the Unity Engine.)

### Compiling NSIS:
In order to compile the NSIS configuration properly, you must compile all of the individual components of Synthesis pertaining to the particular script you are trying to compile. Then the compiled components must be stored in the same directory as the NSIS script, in order for them to be packaged during NSIS compilation. For details on this process, feel free to contact matthew.moradi@autodesk.com

### NSIS FAQ:

Q: Will I need admin privileges to run the installer?

A: Yup.

Q: If I download an updated Synthesis installer, will running it replace all of my custom robot export files?

A: No, _reinstalling_ Synthesis will only replace all of the application components, but your custom robots will be saved.

Q: Is it possible to accidently install multiple versions of Synthesis?

A: It shouldn't be. The installer will always replace any existing Synthesis installations on your system.
