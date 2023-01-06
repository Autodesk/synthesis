author: Synthesis Team
summary: Tutorial for simulating code in Synthesis
id: CodeSimulationCodelab
tags: WPILib, Code, C++, Java
categories: WPILib
environments: Synthesis, VSCode
status: Published
feedback link: https://github.com/Autodesk/synthesis/issues

# Code Simulation in Synthesis

## Setup

### Setup (Project Side)

The Synthesis simulator comes with code simulation already integrated. However, a development environment for what ever code your are trying to simulate will be required.
Synthesis' code simulation relies on the WPILib HALSim extensions, specifically the websocket-client extension. To enable this for your project, add the following lines to
your `build.gradle` file.

```java
wpi.sim.envVar("HALSIMWS_HOST", "127.0.0.1")
wpi.sim.addWebsocketsClient().defaultEnabled = true
```

<b>NOTE:</b> The GUI extension interfaces really well and add Joystick/Controller support to your code, whereas Synthesis currently only supports controllers for non-code simulation.
If you wish to test using your controllers, I recommend using the GUI extension in conjunction.

### Setup (Robot Side)

Inside of Synthesis, you must configure your "IO map" so we know what signals go where. You can access the configuration modal on the side bar.
Once inside the configuration modal, you can add the devices you want to add support for.
<br/>
<b>NOTE:</b> Currently, due to the way the websocket extension works, no CAN devices are supported. We plan on bringing CAN support for the 2023 Summer release or potentially earlier.
At the moment, PWM and Quadrature Encoders (encoders that use 2 DIO ports each) are supported, with more device types and sensors on the way.

## Usage

### Synthesis

Inside of Synthesis, open up the DriverStation. Once connected, you can use it like normal. Our DriverStation is currently limited in its features, so if you need anything beyond
enabling and choosing between Telop and Auto, I recommend using the GUI extension for more functionality.

### Running the code Simulation

I recommend using VSCode and the WPILib Suite extension for running the code simulation. Alternatively you can use this command:
`gradlew.bat simulationJava` or `gradlew.bat simulate`

## Need more help?

If you need any help with code simulation or want to be involved in the process of its development, feel free to join our [Discord](https://www.discord.gg/hHcF9AVgZA) where you can be in direct contact with Synthesis developers.
