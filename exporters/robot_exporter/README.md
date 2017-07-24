# Synthesis-Robot-Exporter

The field exporter is an application used to convert an Autodesk Inventor Assembly file into a format that can be read and loaded by the Synthesis simulator.

## Cloning the repository
In order to properly clone this repository and its dependencies onto your local machine you need to execute the following commands:

        git clone --recursive https://github.com/Autodesk/synthesis
        
OR

       git clone https://github.com/Autodesk/synthesis
       cd synthesis/exporters/robot_exporter
       git submodule update --init --recursive

## Building the solution
### Microsoft Visual Studio
#### 2015
Building the solution with Visual Studio 2015 is relatively easy. First, open the solution in Visual Studio 2015. Any edition of Visual Studio 2015 is capable of building this project. Ensure that the "Solution Platforms" drop-down menu is set to "Any CPU." Right-click on the solution, then click "Build Solution." The project will now build.

#### Older versions
Visual Studio 2013 and 2012 can also be used to build the project, but you must change the .NET framework version and toolset so the project can be compiled. After this is done, follow the instructions for Visual Studio 2015 above.
