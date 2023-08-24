# Synthesis API

[![API](https://github.com/Autodesk/synthesis/actions/workflows/API.yml/badge.svg?branch=master)](https://github.com/Autodesk/synthesis/actions/workflows/API.yml)

The Synthesis API contains parts of Synthesis that can remain mostly Unity abnostic. The end goal of this API is to be used to extend Synthesis'
functionality and reused as throughout future iterations of Synthesis no matter where it may go.

## Getting Started

For more information on cloning this repository and the initial setup of the Synthesis codebase please visit the [Getting Started](../README.md#getting-started) section of the root *README*.

### Dependencies

Similar to the rest of Synthesis, the Synthesis API has the following dependencies that need to be satisfied before attempting to build Synthesis or the Synthesis API.

- [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-1-0)
- [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Protobuf 23.3](https://github.com/protocolbuffers/protobuf/releases/tag/v23.3)
- [Visual Studio 2017](https://visualstudio.microsoft.com/downloads/) or newer (recommended)

You can either install these dependencies manually or use the *resolve dependencies* scripts (`scripts/win/resolve_deps.bat` & `scripts/osx/resolve_deps.sh` respectively) to install some of them for you.

To automatically install .NET and Protobuf run the following script depending on your operating system:
- Windows: `scripts/win/resolve_deps.bat`
- MacOS & Linux: `scripts/osx/resolve_deps.sh`

Note that the windows script requires admin privileges to install .NET and Protobuf to the system PATH. For this reason it is not included by default in `init.bat` unlike how `init.sh` includes `resolve_deps.sh`.

### How To Build The API With An `init` Script

Synthesis comes with a robust build system that includes an `init` script that will automatically compile and link each part of Synthesis together. This is the recommended way to build Synthesis and the Synthesis API.

To build the API with an `init` script first ensure that all dependencies are resolved. Then run the following command based on your operating system in the [root](../) of this repository:

- Windows: `init.bat`
- MacOS & Linux: `init.sh`

### How To Build The API With The API Build Scripts

The Synthesis API comes with it's own build scripts to simplify the process of building the API from the command line. First ensure that all dependencies are resolved. Then run the following command based on your operating system in the [api](/) directory of this repository:

- Windows: `build.bat`
- MacOS & Linux: `build.sh`

### How To Build The API With Visual Studio

TODO: Note from Brandon, I need my windows computer first :)

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
