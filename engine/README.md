# Simulator Engine

This project folder is the simulation engine component. Made with Unity using Bullet Physics libraries, this is the executable that loads exported robots and fields and provides a real time physics environment for them to interact with each other.

## Requirements
* Unity 2019.2.1f1 (Required)
* Visual Studio 2019 (Recommended)

## Setup Instructions

Open the engine project folder within Unity. Because we switched from the built-in
PhysX physics engine to Bullet Physics, you will need to download and import that asset first. Here
are the steps to do so:
* On the navigation bar, go to Window -> Asset Store
* Search for, download, and import 'Bullet Physics for Unity'
* Import all included assets and ignore warning message

Bullet Physics should now be integrated within the project.

You will also need to download and import NuGet to install more packages:
* Search for, download, and import 'NuGet for Unity'
* Restart Unity
* NuGet should automatically install the necessary pacakges. If not, from the navigation bar, select NugGet -> Restore Packages
* The NuGet packages should now be integrated within the project

**Note: If there are errors, verify that SSH.NET is SSH.NET[2016.0.0].**

Lastly, a naming issue prevents Unity from linking against one of the gRPC libraries. To fix this, rename `grpc_csharp_ext.x64.dll` to `grpc_csharp_ext.dll`. Please note that this process must also be done with the `.dylib` and `.so` files for the other platforms. These files can be found in the gRPC core runtime in the Nuget for Unity packages folder.

If you want to test, you will need to open the Scene.unity by double clicking the scene or dragging the scene to the 'Hiearchy' (the upper left hand column.) You can run it from there as you will need to select and load a robot for the simulator to properly intialize.

![unity-screenshot](https://user-images.githubusercontent.com/22991715/44364221-42982500-a47b-11e8-96d9-015836ecc307.png)


## Sample Robots and Fields for Testing

In order to test within the simulator, you will need exported robots and fields. You can export your own
CAD models, but for the sake of convenience we've provided a few sample robots and fields for testing.

* [Robots](http://synthesis.autodesk.com/robot-library.html)
* [Fields](http://synthesis.autodesk.com/field-library.html)

You can place these anywhere within your main drive as we have a directory browser within the simulator, but we recommend `%APPDATA%\Autodesk\Synthesis\Robots` and `%APPDATA%\Autodesk\Synthesis\Fields` as those are the default directories.

Any questions or problems? Feel free to contact us at frc@autodesk.com
