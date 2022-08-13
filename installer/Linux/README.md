# `>_` Synthesis App Image

For running Synthesis we have decided to package our application as an AppImage. It allows Synthesis to be packaged as a single .AppImage file. This also allows for users to run Synthesis without needing a specific distribution.

### Initial Setup ###
It is recommended that you update the packages on your system. For arch users, run `pacman -Syu` as root. Debian users can run `apt update && apt upgrade` as root.

### Dependencies ###
Certain dependencies are necessary in order to package Synthesis as an AppImage.

##### FUSE #####
For Arch, you may need to run: `pacman -S fuse` as root. For Debian, run: `apt install fuse` as root.
If you are still encountering issues, refer to this page: https://docs.appimage.org/user-guide/troubleshooting/fuse.html#ref-install-fuse

##### appimagetool #####
You will also end up needing appimagetool to actually create the AppImage file. However the `package.sh` script will prompt you and install it automatically. If you wish to install in manually, download the latest release from here: https://github.com/AppImage/AppImageKit/releases and make it executable. It should be called appimagetool-$ARCH.AppImage ($ARCH = whatever architecture you are using to package synthesis; most likely x86_64).

### Initial Setup ###
In order to acheive the proper package structure for proper extraction, you must first compile a Unity build as: `Synthesis.x86_64` and store it along with all other files and directories that came with it somewhere on your machine.

Note: It is important that you do not modify or remove any of the files and folders that come built with the `Synthesis.x86_64` file.

It is also strongly recommended that you have some fields and robots exported in the Mirabuf format.

### Packaging ###
The recommended way of creating the AppImage is by using the `package.sh` script. You may also opt to package Synthesis manually. There is some documentation for that process but it is recommended that you have a good understanding of what you are doing if you choose this option.

To run the script, you will likely need to make it executable by running: `chmod +x package.sh` in your preferred terminal. You may also right click on `package.sh` in a file browser and select the option to make it executable.

Now run the script and specify input directories for the version of synthesis you compiled as well as fields and robots: `./init.sh -f /path/to/fields/ -r /path/to/robots/ -b /path/to/synthesis/`

Note: While it is not strictly necessary to include fields and robots, it is strongly recommended to include at least one of each

If it is not already installed, the script will ask to install appimagetool. We recommended answering yes as it will install appimagetool.AppImage to the `~/Applications/` directory and is necessary for creating AppImage files.

### Installing appimagetool ###
appimagetool is the name of the program that is used to create AppImages. You can download and install appimagetool through the official website https://appimage.github.io/appimagetool/ or get it through your distribution's package manager.

Note: appimagetool is usually packaged under AppImageKit rather than as a standalone application.

### Manual Packaging ###

##### File locations #####
Certain files must be moved to the correct locations. First you should move any robot files to `Synthesis.AppDir/robots` (create it if it doesn't exist). Do the same for field files but put them in `Synthesis.AppDir/fields` (create it if it doesn't exist). Finally, move all files and directories in your Synthesis build directory into `Synthesis.AppDir/usr/bin/` (once again, create it if it doesn't exist).

##### Creating The AppImage #####
Finally you can create your AppImage! Make sure you have all dependencies installed and run: `ARCH=x86_64 appimagetool Synthesis.AppDir` which will create the Synthesis AppImage.

Note: Run this instead if you installed appimagetool locally: `ARCH=x86_64 /path/to/appimagetool Synthesis.AppDir`

### Final Note ###
When the end user is downloading the AppImage file, it is strongly recommended to have them put it in the `~/Applications/` directory. This allows it to be found by appimaged as well as itself when running uninstall. It is also recommended to allow them to download the checksum.md5 file so that the file integrity can be verified using `md5sum -c checksum.md5` in the same directory as the AppImage and md5 files.

### Troubleshooting ###
Refer to the AppImage troubleshooting page first if you are having issues: https://docs.appimage.org/user-guide/troubleshooting/index.html
The general documentation may be of use as well: https://docs.appimage.org/index.html
If the issues persist, open a github issue with details about the problem.

