# <img src="https://raw.githubusercontent.com/Autodesk/synthesis/master/installer/Windows/W16_SYN_launch.ico" alt="logo" width="50" height ="50" align="left"/>Synthesis: An Autodesk Technology

Synthesis is a robotics simulator designed to help FIRST Robotics teams design, strategize, test, and practice. Teams can import their own robot and field designs or use preexisting ones into the simulator for a variety of uses, including:
* Testing robot designs
* Exploring the field environment
* Driver practice & strategy
* Code emulation

Developed solely by FIRST students, Synthesis is built with a direct focus on the FIRST community. We've also made the project completely open source in order to better involve the community. This way contributors can help make Synthesis better or modify Synthesis to better suit their team’s needs.

For more information on the product itself or the team, visit http://synthesis.autodesk.com/

## Getting Started

Synthesis is comprised of 4 separate components that are mostly developed independently of each other. You will want to clone the entire repository first, then follow a different set of steps depending on which component you wish to work on. The links below will lead you to specific instructions that will get you a copy of that component up and running on your local machine for development and testing purposes.

* [Simulation Engine](/engine/unity5/)
* [Inventor Robot Exporter (Inventor Plugin)](/exporters/InventorRobotExporter/)
* [Fusion Robot Exporter (Fusion 360 Plugin)](/exporters/FusionRobotExporter/)
* [Code Emulator](/emulation/)



## Contributing

When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change. This  way, we can ensure that there is no overlap between contributions and internal development work. You may contact us at frc@autodesk.com.

When ready to contribute, just submit a pull request and be sure to include a clear and detailed description of the changes you've made so that we can verify them and eventually merge.

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
