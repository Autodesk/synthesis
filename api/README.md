# Module API
The Synthesis™ Module API allows for 3rd party development on the Synthesis™ platform through an open source [NuGet](https://www.nuget.org/packages/Autodesk.Synthesis.Module.API/) package. This enables users and contributors to develop their own addins for the platform without the need for compiling and building our Unity engine. This API is currently still under development and is scheduled for an offical release with Synthesis 5.1.0.

## Using the API in Your Project
### Adding the API Nuget Package
To use the API in your project, simply add the [Autodesk.Synthesis.Module.API](https://www.nuget.org/packages/Autodesk.Synthesis.Module.API/) NuGet package into your .NET Class Library project. Some example modules can be found in the [modules](/modules/) directory in this repository. Note: A VS template for automating this process is underway.
### Importing Your Module Into Synthesis
1. Open your module project solution in Visual Studio.
2. Build the solution from the Visual Studio toolbar.
3. Set up the modules by running one of the following scripts:
	- For Windows users, run `update_modules.ps1` using PowerShell.
	- Linux and Mac scripts are under construction.

## Building the API from Source
### Requirements
- .NET Standard 2.0 (Required)
- Visual Studio 2019 (Recommended)
### Compiling the Synthesis API
1. Use git to clone Synthesis from our repository.
2. Navigate to the `api` directory in your clone of Synthesis.
3. Open `api.sln` in Visual Studio.
4. Build the solution from the Visual Studio toolbar.
