# Simulator Engine

This project folder is the simulation engine component. Made in Unity 2018 using Bullet Physics libraries, this is the executable that loads exported robots and fields and provides a real time physics environment for them to interact with each other.

## Requirements:
* Unity 2018.2.2 (Required)
* Visual Studio 2017 (Recommended)

## Setup Instructions

Open the engine/unity5 project folder within Unity. Because we switched from the built-in
PhysX physics engine to Bullet Physics, you will need to download and import that asset first. Here
are the steps to do so:
* On the navigation bar, go to Window -> Asset Store
* Search for, download, and import 'Bullet Physics for Unity'
* Import all included assets and ignore warning message

Bullet Physics should now be integrated within the project.

You will also need to download and import NuGet to install more packages.
* Search for, download, and import 'NuGet for Unity'
* Restart Unity
* NuGet should automatically install the necessary pacakges. If not, from the navigation bar, select NugGet -> Restore Packages
* The NuGet packages should now be integrated within the project

Lastly, a naming issue prevents Unity from linking against one of the gRPC libraries. To fix this, rename `grpc_csharp_ext.x64.dll` to `grpc_csharp_ext.dll`. This file can be found in the Windows gRPC core runtime in the Nuget for Unity packages folder.

If you want to test, you will need to open the MainMenu.unity scene by double clicking the scene or dragging the scene to the 'Hiearchy' (the upper left hand column.) You can run it from there as you will need to select and load a robot and field for the simulator to properly intialize.

![unity-screenshot](https://user-images.githubusercontent.com/22991715/44364221-42982500-a47b-11e8-96d9-015836ecc307.png)

* Remember to click Assets and then double click MainMenu with the viewcube in order to compile it correctly.
* This will come after importing of the Bullet Asset files.

### Sample Robots and Fields for Testing

In order to test within the simulator, you will need exported robots and fields. You can export your own
CAD models, but for the sake of convenience we've provided a few sample robots and fields for testing.

* [Robots](https://autodesk.box.com/s/9lh1qvtxkurqvv3jlyfy455o5xqf6ts8)
* [Fields](https://autodesk.box.com/s/kx0sv4i7i4pmsl4rpj9gpk03kq6ynz8k)

You can place these anywhere within your main drive as we have a directory browser within the simulator, but it defaults to
**documents\synthesis\robots** and **documents\synthesis\fields**, so you might want to place it there for
convenience.



Any questions or problems? Feel free to contact us at frc@autodesk.com
