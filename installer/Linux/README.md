# `>_` Shell Script Installer

For installation on Linux operating systems, we use our own custom written shell script to extract all the necessary files to their proper locations in the file system. 

### Compiling The Package: ###
In order to acheive the proper package structure for proper extraction, you must first compile a Unity build as `Synthesis.x86_64` which must reside in the `Synthesis` folder. 

Prior to compiling the package, you must first ensure that all scripts have the execute bit set properly using:   
`$ chmod +x LinuxInstaller.sh`. 

Then, download [makeself](https://makeself.io) and compile the `.run` executable using:  
`$ ./makeself.sh LinuxInstaller/ SynthesisLinux*.run "Synthesis For Linux 4.X.X" ./LinuxInstaller.sh`

### Running The Package: ###
Installing the package should be as simple as `$ ./SynthesisLinux*.run`.  

An alternate method of execution using BA$H would be `$ sh SynthesisLinux*.run`.

### Troubleshooting: ###
If you're having trouble executing the `.run` package, try ensuring that the execute bit is properly set by running  
`chmod +x SynthesisLinux*.run`

If still run into issues or have problems with the installation itself, please feel free to submit an [issue](https://github.com/Autodesk/synthesis/issues).

For questions reguarding compilation, feel free to contact matthew.moradi@autodesk.com
