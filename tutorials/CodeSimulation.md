author: Synthesis Team
summary: Tutorial for simulating code in Synthesis
id: CodeSimulationCodelab
tags: WPILib, Code, C++, Java
categories: WPILib
environments: Synthesis, VSCode
status: Published
feedback link: https://github.com/Autodesk/synthesis/issues

# Code Simulation in Synthesis

## Setup (Robot Code)

The Synthesis simulator comes with code simulation already integrated. However, a development environment for what ever code your are trying to simulate will be required. Synthesis' code simulation relies on the WPILib HALSim extensions, specifically the websocket-client extension.  You'll need to make the following changes to your `build.gradle` to connect everything properly.

### C++/Java

#### 1. Desktop Support

You'll need to enable desktop support for your project in order to run the HALSim:

```java
def includeDesktopSupport = true
```

#### 2. Websocket Server Extension

In order to communicate with your browser, you'll need to enable the websocket server extension with the following:

```java
wpi.sim.addWebsocketsServer().defaultEnabled = true
```

#### 3. SyntheSim (Optional)

For CAN-based device support (TalonFX, CANSparkMax, most Gyros), you'll need our own library--SyntheSim. Currently only available for Java, SyntheSim adds additional support for third party devices that don't follow WPILib's web socket specification. It's still in early development, so you'll need to clone and install the library locally in order to use it:

```sh
$ git clone https://github.com/Autodesk/synthesis.git
$ cd synthesis/simulation/SyntheSimJava
$ ./gradlew build && ./gradlew publishToMavenLocal
```

Next, you'll need to have the local maven repository is added to your project by making sure the following is included in your `build.gradle` file:

```java
repositories {
  mavenLocal()
  ...
}
```

Finally, you can add the SyntheSim dependency to your `build.gradle`:

```java
dependencies {
  ...
  implementation "com.autodesk.synthesis:SyntheSimJava:1.0.0"
  ...
}
```

All of these instructions can be found in the [SyntheSim README](https://github.com/Autodesk/synthesis/blob/prod/simulation/SyntheSimJava/README.md).

SyntheSim is very much a work in progress. If there is a particular device that isn't compatible, feel free to head to our [GitHub](https://github.com/Autodesk/synthesis) to see about contributing.

#### 4. HALSim GUI

This should be added by default, but in case it isn't, add this to your `build.gradle` to enable the SimGUI extension by default.

```java
wpi.sim.addGui().defaultEnabled = true
```

This will allow you to change the state of the robot, as well as hook up any joysticks you'd like to use during teleop. You must use this GUI in order
to bring your robot out of disconnected mode, otherwise we won't be able to change the state of your robot from within the app.

#### 5. Start your code

To start your robot code, you can use the following simulate commands with gradle:

```bash
$ ./gradlew simulateJava
```

or for C++:

```bash
$ ./gradlew simulateNative
```

WPILib also has a command from within VSCode you can use the start your robot code:

![image_caption](img/code-sim/wpilib-ext-simulate.png)

### Python

#### 1. Install dependencies

Add the 'sim' component to `robotpy_extras` in your `pyproject.toml`:

```toml
[tool.robotpy]

robotpy_version = ...

robotpy_extras = [
  # other components here
  "sim",
]
```

Run `python -m robotpy sync` to install the needed packages.

#### 2. Start your code

To start your code, you can run the following:

```
python -m robotpy sim --ws-server
```

## Working with Hardware Components

Once your simulation is running, you can control various robot components just like you would on a physical robot. Here's how to work with the supported devices:

### Motors

**PWM Motors:** Standard servo and speed controller motors work right out of the box with WPILib's built-in simulation support. Here's how to set them up:

```java
private Spark m_leftMotor = new Spark(0);  
private Spark m_rightMotor = new Spark(1);  

public void teleopPeriodic() {
    double forward = -m_Controller.getLeftY();
    double turn = m_Controller.getRightX();
    
    m_leftMotor.set(forward + turn);
    m_rightMotor.set(forward - turn);
}
```

**CAN Motors:** For advanced motors like TalonFX or CANSparkMax, you'll need SyntheSim (mentioned above). These motors provide more precise control and feedback, including built-in encoders for position tracking:

```java
// Import SyntheSim classes
import com.autodesk.synthesis.revrobotics.CANSparkMax;
import com.autodesk.synthesis.ctre.TalonFX;
import com.revrobotics.CANSparkLowLevel.MotorType;

private CANSparkMax m_driveLeft = new CANSparkMax(1, MotorType.kBrushless);
private CANSparkMax m_driveRight = new CANSparkMax(2, MotorType.kBrushless);
private TalonFX m_shooter = new TalonFX(7);

public void autonomousPeriodic() {
    m_driveLeft.set(0.5);
    m_driveRight.set(-0.5);
    
    double position = m_driveLeft.getEncoder().getPosition();
    if (position >= 20) {
        m_driveLeft.set(0.0);  
    }
}
```

### Gyroscopes and Accelerometers

**NavX and AHRS devices:** These work seamlessly through SyntheSim and provide realistic orientation data as your robot moves around the virtual field:

```java
// Import NavX library
import com.kauailabs.navx.frc.AHRS;
import edu.wpi.first.wpilibj.SPI;

// Initialize NavX gyro
private AHRS m_gyro = new AHRS();

// Use gyro data in your code
public void autonomousPeriodic() {
    double currentAngle = m_gyro.getAngle();      
    double pitch = m_gyro.getPitch();            
    double roll = m_gyro.getRoll();             
    double yaw = m_gyro.getYaw();                
    
    // Example: Turn to a specific angle
    double targetAngle = 90.0;
    double error = targetAngle - currentAngle;
    double turnSpeed = error * 0.02;
    
    m_driveLeft.set(-turnSpeed);
    m_driveRight.set(turnSpeed);
}
```

**Acceleromoter simulation:** For detecting impacts, measuring tilt, or monitoring acceleration:

```java
import com.autodesk.synthesis.wpilibj.ADXL362;

// Initialize accelerometer
private ADXL362 m_accelerometer = new ADXL362(SPI.Port.kMXP, ADXL362.Range.k8G);

  // Example of reading acceleration data
  public void robotPeriodic() {
    double xAccel = m_accelerometer.getX(); 
    double yAccel = m_accelerometer.getY();  
    double zAccel = m_accelerometer.getZ();
    
    if (Math.abs(xAccel) > 0.5 || Math.abs(yAccel) > 0.5) {
        System.out.println("Warning: Robot is tilting!");
    }
}
```

### Cameras and Vision

**Camera simulation** in Synthesis uses a WebSocket bridge to stream real-time 3D rendered frames from the simulator to your robot code. The example code allows camera stream in Shuffleboard:

```java
// Required imports for Synthesis camera functionality
import edu.wpi.first.cameraserver.CameraServer;
import edu.wpi.first.cscore.CvSource;
import com.autodesk.synthesis.Camera;
import com.autodesk.synthesis.CameraFrameHandler;
import com.autodesk.synthesis.WebSocketMessageHandler;
import com.autodesk.synthesis.SynthesisWebSocketServer;

// Initialize camera components
private Camera m_Camera = new Camera("USB Camera 0", 0);
private CvSource m_videoSource;

/** This function is called once when the robot is first started up. */
@Override
public void simulationInit() {
    System.out.println("🚀 Starting WebSocket server for Synthesis communication...");
    SynthesisWebSocketServer.getInstance().startServer();
    
    // Camera metadata
    m_Camera.setConnected(true);
    m_Camera.setWidth(640);
    m_Camera.setHeight(480);
    m_Camera.setFPS(30);
    
    // Create custom video source. WPILib handles all the streaming!
    m_videoSource = CameraServer.putVideo("Synthesis Camera", 640, 480);
    
    // Register camera with frame handler to receive frames from simulation
    CameraFrameHandler.getInstance().registerCamera("USB Camera 0", m_videoSource);
    
    System.out.println("Camera simulation initialized using WPILib CameraServer!");
    System.out.println("Metadata: " + m_Camera.getWidth() + "x" + m_Camera.getHeight() + " @ " + m_Camera.getFPS() + "fps");
    System.out.println("WPILib automatically creates MJPEG stream and publishes to NetworkTables");
    System.out.println("Camera will appear in Shuffleboard/Glass as 'Synthesis Camera'");
    System.out.println("Camera frame bridge registered - waiting for frames from Synthesis...");
    System.out.println("MJPEG Stream should be available at: http://localhost:1181/?action=stream");
    System.out.println("Stream name in NetworkTables: 'Synthesis Camera'");
}

/** This function is called periodically whilst in simulation. */
@Override
public void simulationPeriodic() {
    // Read camera metadata from simulation
    boolean cameraConnected = m_Camera.isConnected();
    double cameraWidth = m_Camera.getWidth();
    double cameraHeight = m_Camera.getHeight();
    double cameraFPS = m_Camera.getFPS();
    
    // Display camera info on dashboard
    SmartDashboard.putBoolean("Camera Connected", cameraConnected);
    SmartDashboard.putNumber("Camera Width", cameraWidth);
    SmartDashboard.putNumber("Camera Height", cameraHeight);
    SmartDashboard.putNumber("Camera FPS", cameraFPS);
    SmartDashboard.putNumber("Registered Cameras", CameraFrameHandler.getInstance().getCameraCount());
}
```

## Setup (Simulator)

Once started, make sure in the SimGUI that your robot state is set to "Disabled", **not** "Disconnected".

### Spawning in a Robot

Head over to [Fission](https://synthesis.autodesk.com/fission/) and load your robot model into the simulation. Once it appears, place it on the field and open the configuration panel - either through the left sidebar menu or by right-clicking your robot and selecting "Configure."

Here's where the magic happens: you need to switch your robot's "brain" from "Synthesis" (the built-in physics controller) to "WPILib" (your actual code). This tells the simulator to stop using its internal logic and start listening to commands from your running robot program.

Keep an eye on the connection status indicator in the top-right corner. If your code was already running when you switched brains, you should see it connect almost immediately. The indicator will turn green when everything's working properly.

### Simulation Configuration

Once you've switched to the WPILib brain, a new "Simulation" section will appear in your robot's config panel. This is your control center for fine-tuning how your code interacts with the virtual robot.

![image_caption](img/code-sim/config-panel-simulation.png)

#### Auto Reconnect

Enable this if you're experiencing connection drops between your code and the simulator. It's especially helpful when you're frequently restarting your robot program during testing. To activate it, toggle the setting on, switch back to "Synthesis" brain, then switch to "WPILib" again. The setting will stick for future sessions.

#### Wiring Panel

Think of this as your virtual electronics breadboard. The wiring panel shows all available inputs and outputs from both your running code and the simulated robot hardware. Each connection point (the small labeled circles) is color-coded by data type - analog signals, digital I/O, PWM outputs, and more.

![image_caption](img/code-sim/wiring-panel.png)

Use the bottom-left controls to navigate: zoom in and out, auto-fit the view, and add junction nodes when you need to split one signal to multiple destinations. This visual approach makes it easy to see exactly how your code maps to robot hardware.

#### Auto Testing

The Auto Testing panel is perfect for rapid autonomous development. Instead of manually repositioning your robot and restarting your program each time, this feature automates the process so you can focus on tweaking your code.

![image_caption](img/code-sim/auto-testing.png)

Set your maximum test time, choose your alliance station (red or blue), and input any game-specific data your autonomous routine needs. Position your robot exactly where you want it to start, then hit the start button to begin your auto sequence.

When the timer runs out or you manually stop the test, the simulation pauses and gives you two options: reset to the starting position for another run, or close the panel to return to normal operation. This makes it incredibly easy to iterate on autonomous routines without the hassle of manual setup each time.

## Video Walkthrough

Watch the video below to walk through setting up code simulation with Synthesim.

## Need More Help?

If you need help with anything regarding Synthesis or it's related features please reach out through our
[discord server](https://www.discord.gg/hHcF9AVgZA). It's the best way to get in contact with the community and our current developers.
