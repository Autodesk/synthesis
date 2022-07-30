# `>_` Synthesis App Image

For running Synthesis we have decided to package our application as an AppImage. It allows Synthesis to be packaged as a single .AppImage file. This also allows for users to run Synthesis without needing a specific distribution.

### Compiling The Package: ###
In order to acheive the proper package structure for proper extraction, you must first compile a Unity build as: `Synthesis.x86_64` and store it somewhere on your machine.

Note: It is important that you do not modify or remove any of the files and folders that come built with the `Synthesis.x86_64` file.

It is also strongly recommended that you have some fields and robots exported in the Mirabuf format.

### Running Init ###
Before packaging, it is important to run the init script unless you really know what you are doing. At the very least it is recommended to read it to see what is needed for setup.

To run the init script, you likely need to make it executable by running: `chmod +x init.sh` in your preferred terminal.

Now run the init script and specify input directories for the version of synthesis you compiled as well as fields and robots: `./init.sh -f /path/to/fields/ -r /path/to/robots/ -b /path/to/synthesis/`

Note: While it is not strictly necessary to include fields and robots, it is strongly recommended to include at least one of each

### Installing appimagetool ###
appimagetool is the name of the program that is used to create AppImages. You can download and install appimagetool through the official website https://appimage.github.io/appimagetool/ or get it through your distribution's package manager.

Note: appimagetool is usually packaged under AppImageKit rather than as a standalone application.

### Creating The AppImage ###
Finally you can create your AppImage! Run: `ARCH=x86_64 appimagetool Synthesis.AppDir` which will create the Synthesis AppImage.

Note: Run this instead if you installed appimagetool locally: `ARCH=x86_64 /path/to/appimagetool Synthesis.AppDir`

Note: You might get an error saying that AppImage needs FUSE installed. Install it. For Arch users you may need to run: `pacman -S fuse`. For Debian users run: `apt install fuse`
If you are still encountering issues, refer to this page: https://docs.appimage.org/user-guide/troubleshooting/fuse.html#ref-install-fuse

### Troubleshooting ###
Refer to the AppImage troubleshooting page first if you are having issues: https://docs.appimage.org/user-guide/troubleshooting/index.html
The general documentation may be of use as well: https://docs.appimage.org/index.html
If the issues persist, open a github issue with details about the problem.

