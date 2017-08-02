# Autodesk BXD Synthesis

We're doing a code sprint competition right now! Click [HERE](http://bxd.autodesk.com/codesprint.html) to learn more about it!
- [Official Competition Document](http://bxd.autodesk.com/codesprint/synthesis_codesprint.pdf)

## Overview
Synthesis is a simulation engine allowing FIRST Robotics teams to test robot designs and software prior to completion of robot construction.

## Build
### Rolling builds
Each subsection of this repository has a `README.md` giving, among other things, build instructions and a list of dependencies (if applicable).

### Release builds
#### Dependencies
(these are updated on best effort basis)
- [OpenTk](http://www.opentk.com/) 
- [bulletSharp website](https://andrestraks.github.io/BulletSharp/)
- [Unity4.7.2](https://unity3d.com/get-unity/download/archive)
- [Unity5.3.5.f1](https://unity3d.com/get-unity/download/archive)
- [WPILIB](https://github.com/wpilibsuite/allwpilib)

#### Instructions
each project will have a readme inside of its directory. 
go to the directory and read the readme for instructions on building the project. 
## Directory Structure

In order to achieve maximum user satisfaction we can insert all of the code into a single repository and
then follow up with constant commits while using
the issue tracker and tags for the major build
versions, the entire team would transfer to this
version of git in order to best benefit the
community. We plan to include all of the current
repositories into a single repository by making
sub directories and more specific READMEs to
direct the users on how to make pull requests
and fork properly. This would also be configured
with recursive build files for easy access to the
built executables. 

Below are some examples I
made on how to achieve this properly:

![Directory Structure](https://cloud.githubusercontent.com/assets/6741771/16959078/360a5042-4d98-11e6-904b-bf5f636f2430.png)
