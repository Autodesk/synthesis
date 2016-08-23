# research

This project is made from the ground up with Bullet Sharp and OpenTK
The goal is to create our own physics environment that we can optimize for simulation

### What there is

There are two projects: the OpenTK build and the Simulation R/D build.
The OpenTK build is mainly for figuring out how to orioerly draw the visual meshes.
The Simulation build is for developing the physics of the environment.

### How to set up

Open either project in Visual Studio 2015 (supports cpp11 & 14).
The OpenTK DLLs are NuGet packages, also included in the libs folder
All other DLLs should be included in the 'Lib' filder.

### How to build
Include a reference to the dlls through the reference object in visual studio
if nuget package is not found: Goto tools -> Nuget Package Management -> Search OpenTK (Download glcontrol and regular)

If you cannot compile or find the libraries that you need to build then you can download a package from our website at http://bxd.autodesk.com/?page=Downloads. 

Or
You can download it directly at http://bxd.autodesk.com/Downloadables/R&DLib.zip
