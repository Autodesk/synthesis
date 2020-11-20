# Engine

## Setup Instructions

### Requirements
- Unity Hub with Unity 2019.4.0f1 (Required)
- Visual Studio 2019 (Recommended)

### Downloading Synthesis
1. Use git to clone Synthesis from our repository.

### Compiling the Synthesis API
1. Navigate to your local copy of Synthesis, and enter the `api` folder.
2. Open `api.sln` in Visual Studio.
3. Build the solution from the Visual Studio toolbar.

### Setting Up Modules
1. Navigate to your local copy of Synthesis, and enter the `modules` folder.
2. Open `modules.sln` in Visual Studio.
3. Build the solution from the Visual Studio toolbar.
4. Set up the modules by running one of the following scripts:
	- For Windows users, run `update_modules.ps1` using PowerShell.
	- Linux and Mac scripts are under construction.

### Setting Up the Unity Project
1. Open Unity Hub, click the Add button, and navigate to your local copy of Synthesis. Enter the `engine` folder, and click "Select Folder."
2. Open the engine project that has been added to your Unity Hub.
3. From the Project Window within the Unity Editor, navigate to `Assets/Scenes` and open the `MainSim` scene.
4. Play the scene by hitting the play button at the top of the Editor. 
