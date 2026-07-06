# Code Simulation

## Summary

Code simulation in Synthesis works using websockets (HALSim).
We have a Java library to be included in robot code projects called SyntheSimJava under the `simulation/` directory in the Synthesis repository.
It essentially just wraps common vendor library classes (e.g., SparkMax, AHRS, etc.) and forces them to comply with the [HALSim WS Standard](https://github.com/wpilibsuite/allwpilib/blob/main/simulation/halsim_ws_core/doc/hardware_ws_api.md).
We also have sample projects used both for reference and testing within the `simulation/samples/` directory.

## Testing

In order to test code sim, SyntheSimJava must be built and published to the local maven repository.
Ensure you are using the correct Java version, and navigate to the SyntheSimJava project in your terminal.
Run `./gradlew build` and `./gradlew publishToMavenLocal` to build and publish.

Next, go to a sample project directory and run `./gradlew simulateJava`.
This will open up the simulator, with widgets for NetworkTable entries, HALSim devices, etc.

Open Fission, spawn a robot, configure its brain (Configure → Brain), choose WPILib, and you should then see in the top right that it’s connected to the simulator.
Once the brain is configured, a new Simulation option will show up in the configure menu – enter that menu and choose Wiring Panel to configure the flow of data between Fission and the robot code simulator.

Once code sim is configured in Fission, change the mode from Disconnected to Auto or Teleop depending on the phase you’re trying to test, and the code should simulate.
You should see updates both in Fission and in the readouts in the code simulator for the motors and sensors.
The robot code will control the movement of the robot in Fission, and you can expand each of the devices in the robot code simulator GUI to see if their fields are being updated by Fission.
For example, running the JavaAutoSample should cause the `ADXL362[4]` and `SYN AHRS[0]` devices to update with rotation and acceleration data from Fission
(note that the names may differ if the code changes).
