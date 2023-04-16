# Synthesis API
The Synthesis API contains parts of Synthesis that can remain mostly Unity abnostic. The end goal of this API is to be used to extend Synthesis'
functionality and reused as throughout future iterations of Synthesis no matter where it may go.

<!--
## Using the API in Your Project
### Adding the API Nuget Package
To use the API in your project, simply add the [Autodesk.Synthesis.Module.API](https://www.nuget.org/packages/Autodesk.Synthesis.Module.API/) NuGet package into your .NET Class Library project. Some example modules can be found in the [modules](/modules/) directory in this repository. Note: A VS template for automating this process is underway.
### Importing Your Module Into Synthesis
1. Open your module project solution in Visual Studio.
2. Build the solution from the Visual Studio toolbar.
3. Set up the modules by running one of the following scripts:
	- For Windows users, run `update_modules.ps1` using PowerShell.
	- Linux and Mac scripts are under construction.
-->
## Building the API from Source
### Requirements
- .NET Standard 2.0 (Required)
- .NET Core 3.1 (Required)
- Visual Studio 2019 (Recommended)
### Compiling the Synthesis API
1. Use git to clone Synthesis from our repository.
2. Navigate to the [`api`](/api/) directory in your clone of Synthesis.
3. Compile [`api.sln`](/api/api.sln).
	- Use `dotnet` to compile the solution:
		```
		$ dotnet build
		```
	- Use Visual Studio to open the solution and compile the API.
