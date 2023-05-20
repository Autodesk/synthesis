# Linux Dependencies
1. Compile [`LinuxDeps.csproj`](/engine/LinuxDeps/LinuxDeps.csproj).
	- Use `dotnet` to compile the solution:
		```
		$ dotnet build
		```
	- or, Use Visual Studio to open the solution and compile the API.
2. Run `copy_deps.sh`. This will remove everything in the Packages folder so make sure you run [`post_build.sh`](/api/post_build.sh) afterwards.
    ```
    $ ./copy_deps.sh
    ```
