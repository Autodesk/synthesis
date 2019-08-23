# <img src="https://raw.githubusercontent.com/Autodesk/synthesis/master/installer/Windows/W16_SYN_launch.ico" alt="logo" width="50" height ="50" align="left"/>Synthesis: An Autodesk Technology

Synthesis is a robotics simulator designed to help FIRST Robotics teams design, strategize, test, and practice. Teams can import their own robot and field designs or use preexisting ones into the simulator for a variety of uses, including:
* Testing robot designs
* Exploring the field environment
* Driver practice & strategy
* Code emulation

Developed solely by FIRST students, Synthesis is built with a direct focus on the FIRST community. We've also made the project completely open source in order to better involve the community. This way contributors can help make Synthesis better or modify Synthesis to better suit their team’s needs.

For more information on the product itself or the team, visit http://synthesis.autodesk.com/

## Engine

The engine code is based in Unity, more info on how to build and dependency listings can be found in the engine subdirectory that is comprised of the following files:

```
.
└── unity5
    ├── Assets
    │   ├── Materials
    │   │   └── Materials
    │   ├── Prefabs
    │   ├── Resources
    │   │   ├── Fonts
    │   │   ├── Images
    │   │   │   ├── Mix\ and\ Match
    │   │   │   ├── New\ Icons
    │   │   │   ├── New\ Textures
    │   │   │   ├── Old\ Assets
    │   │   │   │   └── Toolbar
    │   │   │   └── ReplayMode
    │   │   ├── Materials
    │   │   └── Prefabs
    │   ├── Scene
    │   ├── Scripts
    │   │   ├── Analytics
    │   │   ├── BUExtensions
    │   │   ├── Camera
    │   │   ├── Configuration
    │   │   ├── DriverPractice
    │   │   ├── Emulation
    │   │   ├── FEA
    │   │   ├── FSM
    │   │   ├── Field
    │   │   ├── GUI
    │   │   │   ├── Scrollables
    │   │   │   └── ToolbarStates
    │   │   ├── Input
    │   │   │   ├── Enums
    │   │   │   └── Inputs
    │   │   ├── Media
    │   │   ├── MixAndMatch
    │   │   ├── Network
    │   │   ├── RN
    │   │   ├── Robot
    │   │   ├── Sensors
    │   │   ├── States
    │   │   ├── Threading
    │   │   └── Utils
    │   ├── StandaloneFileBrowser
    │   │   ├── Common
    │   │   │   └── Editor
    │   │   └── Plugins
    │   │       ├── Linux
    │   │       │   └── x86_64
    │   │       ├── MacOS
    │   │       │   └── StandaloneFileBrowser.bundle
    │   │       │       └── Contents
    │   │       │           └── MacOS
    │   │       └── Windows
    │   └── Textures
    ├── Packages
    ├── ProjectSettings
    └── Synthesis_Data
```

## Exporters 

The exporters are split up in primarily 2 different ways, first is by the product they are exporting from and second is by the type of exporter, field or robot. You can find more info in the exporter directory.

```
.
├── Aardvark-Libraries
│   ├── CLEW
│   ├── ConvexLibraryWrapper
│   ├── MIConvexHull
│   │   ├── ConvexHull
│   │   ├── Properties
│   │   └── Triangulation
│   ├── SimulatorFileIO
│   │   ├── IO
│   │   │   ├── BXDA
│   │   │   ├── BXDF
│   │   │   └── BXDJ
│   │   ├── Joints
│   │   │   ├── Driver
│   │   │   └── Sensors
│   │   ├── Math
│   │   ├── ModelTree
│   │   ├── Properties
│   │   └── Utilities
│   └── VHACD
│       ├── cl
│       ├── inc
│       ├── public
│       └── src
├── BxDFieldExporter
│   ├── BxDFieldExporter
│   │   ├── Components
│   │   ├── Forms
│   │   ├── Images
│   │   ├── Properties
│   │   └── Resources
│   └── packages
│       ├── Costura.Fody.1.6.2
│       │   └── tools
│       ├── Fody.2.0.0
│       │   ├── Content
│       │   └── Tools
│       ├── Newtonsoft.Json.11.0.2
│       │   └── lib
│       │       ├── net20
│       │       ├── net35
│       │       ├── net40
│       │       ├── net45
│       │       ├── netstandard1.0
│       │       ├── netstandard1.3
│       │       ├── netstandard2.0
│       │       ├── portable-net40+sl5+win8+wp8+wpa81
│       │       └── portable-net45+win8+wp8+wpa81
│       └── OpenTK.3.0.1
│           ├── content
│           └── lib
│               └── net20
├── FusionRobotExporter
│   ├── Include
│   │   └── rapidjson
│   │       ├── error
│   │       ├── internal
│   │       └── msinttypes
│   ├── Resources
│   │   ├── DOFIcons
│   │   ├── DriveIcons
│   │   ├── FinishIcons
│   │   ├── JointIcons
│   │   ├── PrecheckIcons
│   │   ├── SettingsIcons
│   │   └── WeightIcons
│   ├── Source
│   │   ├── AddIn
│   │   └── Data
│   │       ├── BXDA
│   │       └── BXDJ
│   │           ├── Components
│   │           └── Joints
│   ├── VHACD
│   │   ├── Include
│   │   └── Lib
│   └── palette
│       ├── css
│       ├── js
│       └── resources
├── InventorRobotExporter
│   ├── InventorRobotExporter
│   │   ├── Exporter
│   │   ├── GUI
│   │   │   ├── Editors
│   │   │   │   ├── AdvancedJointEditor
│   │   │   │   │   └── JointSubEditors
│   │   │   │   └── JointEditor
│   │   │   ├── Guide
│   │   │   ├── JointView
│   │   │   ├── Loading
│   │   │   └── Messages
│   │   ├── Managers
│   │   ├── OGLViewer
│   │   ├── Properties
│   │   ├── Resources
│   │   │   ├── Branding
│   │   │   ├── DrivetrainTypeIcons
│   │   │   ├── LoadingAnimations
│   │   │   ├── RibbonIcons
│   │   │   └── Shaders
│   │   ├── RigidAnalyzer
│   │   ├── SkeletalStructure
│   │   └── Utilities
│   │       ├── ImageConverters
│   │       └── Synthesis
│   └── RobotExporterTests
└── field_exporter
    ├── FieldExporter
    │   ├── Components
    │   ├── Exporter
    │   ├── Forms
    │   └── Properties
    └── packages
        ├── Newtonsoft.Json.12.0.2
        │   └── lib
        │       ├── net20
        │       ├── net35
        │       ├── net40
        │       ├── net45
        │       ├── netstandard1.0
        │       ├── netstandard1.3
        │       ├── netstandard2.0
        │       ├── portable-net40+sl5+win8+wp8+wpa81
        │       └── portable-net45+win8+wp8+wpa81
        └── OpenTK.3.0.1
            ├── content
            └── lib
                └── net20
```

## Emulation

Emulaton will require a significant amount of setup and resources to configure and build properly. More info can be found in the emulation directory. Here is the file structure:

```
.
├── EmulationCommunication
│   └── EmulationCommunication
│       └── Properties
├── api
│   └── v1
│       └── proto
├── bridge
│   ├── cmd
│   ├── pkg
│   │   ├── protocol
│   │   │   └── grpc
│   │   └── service
│   │       └── v1
│   └── scripts
├── hel
│   ├── build
│   │   └── lib
│   ├── docs
│   ├── external_configs
│   ├── include
│   ├── scripts
│   ├── src
│   └── tests
│       └── test_projects
│           └── java
└── wpi_cross
```

## Getting Started

Synthesis is comprised of 4 separate components that are mostly developed independently of each other. You will want to clone the entire repository first, then follow a different set of steps depending on which component you wish to work on. The links below will lead you to specific instructions that will get you a copy of that component up and running on your local machine for development and testing purposes.

* [Simulation Engine](https://github.com/Autodesk/synthesis/blob/master/engine/unity5/README.md)
* [Inventor Robot Exporter (Inventor Plugin)](https://github.com/Autodesk/synthesis/blob/master/exporters/robot_exporter/README.md)
* [Fusion Robot Exporter (Fusion 360 Plugin)](https://github.com/Autodesk/synthesis/blob/master/exporters/FusionRobotExporter/README.md)
* [Code Emulator](https://github.com/Autodesk/synthesis/blob/master/emulation/README.md)



## Contributing

When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change. This  way, we can ensure that there is no overlap between contributions and internal development work. You may contact us at frc@autodesk.com.

When ready to contribute, just submit a pull request and be sure to include a clear and detailed description of the changes you've made so that we can verify them and eventually merge.