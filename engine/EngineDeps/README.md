# Engine Dependencies
This will be replacing the NuGet for Unity package as this is a bit more reliable and expandable

## Windows Dependencies
1. Compile [`EngineDeps.csproj`](/engine/EngineDeps/EngineDeps.csproj).
	- Use `dotnet` to compile the solution:
		```
		$ dotnet build
		```
	- or, Use Visual Studio to open the solution and compile the API.
2. Run `copy_deps.bat`. This will remove everything in the Packages folder so make sure you run [`post_build.bat`](/api/post_build.bat) afterwards.
    ```
    $ copy_deps.bat
    ```

## Linux Dependencies
1. Compile [`EngineDeps.csproj`](/engine/EngineDeps/EngineDeps.csproj).
	- Use `dotnet` to compile the solution:
		```
		$ dotnet build
		```
	- or, Use Visual Studio to open the solution and compile the API.
2. Run `copy_deps.sh`. This will remove everything in the Packages folder so make sure you run [`post_build.sh`](/api/post_build.sh) afterwards.
    ```
    $ ./copy_deps.sh
    ```
