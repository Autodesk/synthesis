# Hardware Emulation Layer Documentation

- [Project Structure](#project-structure)
  - [HELBuildTool](#hel-build-tool)
- [Building](#building)
  - [Installing Cygwin](#installing-cygwin)
  - [Basic Cygwin Usage](#basic-cygwin-usage)
  - [Building The HEL](#building-the-hel)
  - [Make Targets](#make-targets)
- [Packaging Into the Installer](#packaging-into-the-installer)
  - [cygscripts](#cygscripts)



## Project Structure

The goal of this project is to support user code with minimal maintenance and difficulty for the user. To accomplish this, the HEL contains a copy of the RoboRIO HAL which has been modified to work on windows and connect to synthesis. Both the Java and C++ wpilibs are left mostly unmodified, and are instead compiled against the modified HAL. Currently, the only major modification is removing camera related headers because we don't want to pull in OpenCV as a dependency yet.

Because wpilib is left unmodified, we are using submodules for wpilib and the other dependencies. In order to use these submodules, you have either clone the repository using `git clone --recursive`, or later run `git submodule init && git submodule update` inside the synthesis directory.

### `HELBuildTool`

The user's robot code is currently built using the Cygwin MinGW compiler. This can be a pain for the end user, so we created a tool to compile and run code fairly easily. This tool is in `emulation/HELBuildTool` and is written in C# with windows forms. The main purpose is to allow the user to select a team number and robot code directory, followed by launching a Cygwin terminal to run `make` and build the code. The makefile used for this is `emulation/HELBuildTool/Makefile`, but is installed to a different location. When `HELBuildTool` is built in release mode, it will point towards the absolute path of the installed makefile, but when built in debug mode it will use the relative path from it's executable location. It is also important to make sure that it is built in the `x64` mode instead of `Any CPU` or things will break.

The team number configuration is a little weird. The only reason that we need to know what the team number is is to connect to the driver station by selecting the correct IP address that corresponds to the loopback adapter. This usage is in `emulation/hel/lib/athena/FRCDriverStation.cpp`. Unfortunately, we don't know what the team number is when building the HAL because it is distributed as a binary to the user. For this to work, we link the HAL as a static library with `extern int teamID` declared. Then, when compiling the user code, we compile a separate file containing just the `teamID` definition. Things get a little more complicated because now we have to figure out when to recompile this file. Ideally, it should only be compiled when the preprocessor macro for `TEAM_ID` changes. Detecting this is quite difficult, and the file compiles very quickly, so we opted to recompile it every time that the code is linked. This still leaves the issue that the file is not linked if the user code is not changed, so we declared the output binary as `.PHONY` to force the target to be run every time.

In the future, the `HELBuildTool` will probably be replaced with an Eclipse plugin which integrates in to the IDE to provide a more seamless interface to the user. Unfortunately, eclipse is a massive, badly documented, project with a disgusting API, so this will be a lot of work.

### Modifications to Wpilib or Ntcore

Wpilib and ntcore are both provided as git submodules, which means that changes get tracked in a sligntly different way from things that are stored in the repository normally. Submodules work by including a reference to an external git repository, but the files included in this external repository are not included in our git repository. 

## Building

The build system for the HEL uses GNU Make on Cygwin. Because of this, there are a few extra steps to do before setting it up.

### Installing Cygwin

Cygwin is a Linux layer for Windows. A lot of the original HAL code was written for the RoboRIO, which is a Linux system, meaning that it is easier to compile it for a more similar platform than for a non-Unix based OS like Windows. In addition, Cygwin gives use the ability to use GCC, which is the only supported compiler in Eclipse. If we want to implement an Eclipse plugin, we will have to use Cygwin at some point.

To install Cygwin, use the setup executable from [here](https://cygwin.com/setup-x86_64.exe) and run it. Select "Install from internet", followed by "All users" and `C:\cygwin64` as the root directory. *Do not* install to `C:\Program Files` or bad things will happen (Cygwin is supposed to be placed in it's own directory, and if it gets put in Program Files, it will end up putting a bunch of random Linux stuff directly in Program Files). You can leave the local package directory at whatever it defaults to. Also leave "Direct connection" on.

The next step is selecting which packages to install. Cygwin has support for automatically installing a large number of Linux packages, although the interface for doing this is less than ideal. The first step is to select a mirror. You can leave this at default, or select something else. It only really affects the speed of installation. Next, there will be a menu in the upper left hand corner labeled "view". This should be set to "Full". In order to install a package, type the package name into the search field. All packages containing that name will be listed. You want to find the one that exactly matches the name. One easy mistake to make here is pressing enter after typing into the search field. This will actually move on to the next step of installation and will not let you go back, so don't do that. Once you find the package, you can mark it for installation by clicking the `⟲` icon once. In this case, the packages needed for the HEL are:

- `make`
- `patch`
- `mingw64-x86_64-gcc-g++`

Once all of these packages have been selected, you can click next until the progress bars appear, at which point Cygwin will install these packages to the root (`C:\cygwin64`).

### Basic Cygwin Usage

Because Cygwin is a Linux emulation layer, most basic Linux commands will work fine. You can find a list of common commands [here](https://www-uxsup.csx.cam.ac.uk/pub/doc/suse/suse9.0/userguide-9.0/ch24s04.html). The most common commands that are used for working with the HEL are `cd` and `make`. Pretty much everything else can be done through the Windows GUI, if you are more comfortable with that.

#### Command Line Syntax In General

The model for interacting with Cygwin is a command line, which can make it a very unfamiliar environment if you normally use GUIs for tasks like file management. In a command line, the user interacts with the environment by typing in commands followed by the enter key. After a command has been entered, bash (the shell used by default in Cygwin), will return the output of the command. Commands in bash are written starting with the name of the command followed by the parameters separated by spaces. For example, the `echo` command is analogous to `printf` in C/C++. To output the text "Hello, World!", you would run `echo Hello, World!`

#### Path Specification

In Windows, paths are generally formatted using backslashes and this weird, broken, letter drive naming scheme. For example, Cygwin's `bin` directory is `C:\cygwin64\bin`. In Linux, paths work differently. Basically, Linux paths use slash instead of backslash, and don't have drives separated at the root level. Instead of `C:\`, linux has `/`. For example, the home directory is `/home/your_username`. One thing to be aware of is that Cygwin mounts `C:\cygwin64` to `/` rather than `C:\`. In order to access other drives in Cygwin, use `/cygdrive/c` (or whatever windows drive letter you want to access). Finally, although windows culture encourages the use of spaces in filenames, this is generally a bad idea and cumbersome to use on the command line, because space is the separator between parameters. In order to access a path containing a space, use a backslash before each space like this `/cygdrive/c/Program\ Files`.

Relative paths are similar to the way they would work in windows. If a path does not start with `/`, it will be relative to the current directory. Path's starting with `.` are explicitly relative. An easy way to input absolute paths to Cygwin, if you are familiar with windows, is to drag the directory from windows explorer onto the Cygwin window. This will put the path where the cursor is. This is especially useful because the home directory in Cygwin is, by default, not the same as your windows home directory, so if you try to move to `~/Documents/synthesis`, it probably won't exist unless you put it in the Cygwin home directory on purpose.

#### `cd`

In Linux, the command line is always in a specific directory. Most commands use the current directory to base relative file paths, so it is useful to be in whatever directory is most relevant to what you are doing. In the case of working on the HEL, this would probably be `synthesis/emulation/hel`. In order to change the current directory, use the `cd` command. This command works by typing `cd` with the path to move into as it's first and only parameter. Note that you can drag the synthesis directory onto the Cygwin window to make things a little easier.

#### `ls`

`ls` will display the contents of the current directory as output. It's pretty simple to use, just type `ls` and it will list all the files and folders. One thing to be aware of is that, while on windows files beginning with the `.` character are fine, on Linux and Cygwin they are automatically hidden. To show files beginning with a `.` (like `.gitignore`), use `ls -a`.

### Building The HEL

The HEL uses GNU Make as it's build system, which is a little different from building in visual studio. First, you need to `cd` to the HEL directory in Cygwin. The most basic way to build it is to simply run the `make` command. This is not idea because `make` uses only one core by default. Since modern computers have lots of cores, there is no reason not to take advantage of that and compile more than one file at once. If you pass the parameter `-jN` (where `N` is the number of cores to use) to `make`, it will compile multiple files in parallel. For example, `make -j8` compiles 8 files at the same time.

The makefile for the HEL has four build targets which you will probably use. The first, which is used by default when you call `make`, builds both the C++ and Java wpilibs using the modified HAL. The targets `cpp`, and `java` build only the C++ or Java wpilib versions respectively. You build a particular target by passing it as a parameter to `make`. For example, `make cpp -j8` will build the C++ wpilib only with 8 cores. Finally, the `clean` target will remove all of the compiled files. This is used when something has gone wrong with the build process to the point that you want to recompile everything from scratch instead of trying to do another incremental build. In addition to being able to build only a certain part of the HEL, you can also select between debug and release builds. Debug is used by default, but to do a release build, simply pass `TARGET=release` to make like this: `make TARGET=release -j8`

### Make Targets

Targets from `emulation/hel/Makefile`:

  - `make`: Build everything
  - `make cpp`: Build only the c++ wpilib
  - `make java`: Build only the java wpilib
  - `make hal`: Build only the HAL
  - `make clean`: Remove build files to force a clean rebuild
  - `make patch`: Manually apply patches to wpilib (this will normally be done every time you build)
  - `make revert_patch`: Undo the patch to wpilib (required in order to update wpilib, although it will be run automatically with the `update_wpilib` target
  - `make update_wpilib`: Attempt to pull the latest changes to the wpilib submodules

Targets from `emulation/Makefile`:
  - `make install`: Builds and copies all files to the installer build directory

## Packaging Into the Installer

Unlike the other components of Synthesis, the process for copying files to build an installer is automated for the emulator. In the `synthesis/emulator` directory, there is a makefile which will install all of the files to `../installer` by running `make install`. Before doing this, you have to build HELBuildTool in Visual Studio, although the makefile will warn you if you have not done this yet. It is required to build HELBuildTool in Release mode for the x64 cpu, or it may either not update in the installer, or fail to build the installer alltogether.

### cygscripts

In addition to copying files into the installer directory, one component of the emulator is already there. The cygscripts directory contains a few batch scripts (and one visual basic script because windows has no command line download tool, for some reason). These scripts are used by the installer to install Cygwin on the users machine for building user robot code. Installing Cygwin takes about 100 times longer than the rest of installation combined, so we may want some sort of warning in the future. In addition to installing Cygwin, cygscripts will also install all packages specified on the command line. In order to add or remove packages from the installer, you have to edit both `installer/installerDev.nsi` and `installer/installerRelease.nsi`. In the section which installs to the `SynthesisDrive` directory, there will be a line which begins with `ShellExecWait`. The end of this line contains a comma-separated list of packages to install. To install or remove packages, simply edit this list in both files.
