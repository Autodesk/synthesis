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
