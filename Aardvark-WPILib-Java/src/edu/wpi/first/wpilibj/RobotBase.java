/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                   */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/
package edu.wpi.first.wpilibj;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.io.PrintStream;
import java.net.URL;

import edu.wpi.first.wpilibj.communication.FRCControl;
import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.networktables.NetworkTable;

/**
 * Implement a Robot Program framework. The RobotBase class is intended to be
 * subclassed by a user creating a robot program. Overridden autonomous() and
 * operatorControl() methods are called at the appropriate time as the match
 * proceeds. In the current implementation, the Autonomous code will run to
 * completion before the OperatorControl code could start. In the future the
 * Autonomous code might be spawned as a task, then killed at the end of the
 * Autonomous period.
 */
public abstract class RobotBase {

	/**
	 * The VxWorks priority that robot code should work at (so Java code should
	 * run at)
	 */
	public static final int ROBOT_TASK_PRIORITY = 101;
	/**
	 * Boolean System property. If true (default), send System.err messages to
	 * the driver station.
	 */
	public final static String ERRORS_TO_DRIVERSTATION_PROP = "first.driverstation.senderrors";
	/**
	 * name and contents of the version file that is read by the DS to determine
	 * the library version
	 */
	protected final static String FILE_NAME = "./FRC_Lib_Version.ini";
	protected final static String VERSION_CONTENTS = "Java 2014 Update 0";
	protected final DriverStation m_ds;
	private final Watchdog m_watchdog = Watchdog.getInstance();

	/**
	 * Constructor for a generic robot program. User code should be placed in
	 * the constructor that runs before the Autonomous or Operator Control
	 * period starts. The constructor will run to completion before Autonomous
	 * is entered.
	 * 
	 * This must be used to ensure that the communications code starts. In the
	 * future it would be nice to put this code into it's own task that loads on
	 * boot so ensure that it runs.
	 */
	protected RobotBase() {
		// TODO: StartCAPI();
		// TODO: See if the next line is necessary
		// Resource.RestartProgram();

		// if (getBooleanProperty(ERRORS_TO_DRIVERSTATION_PROP, true)) {
		// Utility.sendErrorStreamToDriverStation(true);
		// }
		NetworkTable.setServerMode();// must be before b
		m_ds = DriverStation.getInstance();
		m_watchdog.setEnabled(false);
		NetworkTable.getTable(""); // forces network tables to initialize
		NetworkTable.getTable("LiveWindow").getSubTable("~STATUS~")
				.putBoolean("LW Enabled", false);
	}

	/**
	 * Free the resources for a RobotBase class.
	 */
	public void free() {
	}

	/**
	 * Check on the overall status of the system.
	 * 
	 * @return Is the system active (i.e. PWM motor outputs, etc. enabled)?
	 */
	public boolean isSystemActive() {
		return m_watchdog.isSystemActive();
	}

	/**
	 * Return the instance of the Watchdog timer. Get the watchdog timer so the
	 * user program can either disable it or feed it when necessary.
	 * 
	 * @return The Watchdog timer.
	 */
	public Watchdog getWatchdog() {
		return m_watchdog;
	}

	/**
	 * @return If the robot is running in simulation.
	 */
	public static boolean isSimulation() {
		return false;
	}

	/**
	 * @return If the robot is running in the real world.
	 */
	public static boolean isReal() {
		return true;
	}

	/**
	 * Determine if the Robot is currently disabled.
	 * 
	 * @return True if the Robot is currently disabled by the field controls.
	 */
	public boolean isDisabled() {
		return m_ds.isDisabled();
	}

	/**
	 * Determine if the Robot is currently enabled.
	 * 
	 * @return True if the Robot is currently enabled by the field controls.
	 */
	public boolean isEnabled() {
		return m_ds.isEnabled();
	}

	/**
	 * Determine if the robot is currently in Autonomous mode.
	 * 
	 * @return True if the robot is currently operating Autonomously as
	 *         determined by the field controls.
	 */
	public boolean isAutonomous() {
		return m_ds.isAutonomous();
	}

	/**
	 * Determine if the robot is currently in Test mode
	 * 
	 * @return True if the robot is currently operating in Test mode as
	 *         determined by the driver station.
	 */
	public boolean isTest() {
		return m_ds.isTest();
	}

	/**
	 * Determine if the robot is currently in Operator Control mode.
	 * 
	 * @return True if the robot is currently operating in Tele-Op mode as
	 *         determined by the field controls.
	 */
	public boolean isOperatorControl() {
		return m_ds.isOperatorControl();
	}

	/**
	 * Indicates if new data is available from the driver station.
	 * 
	 * @return Has new data arrived over the network since the last time this
	 *         function was called?
	 */
	public boolean isNewDataAvailable() {
		return m_ds.isNewControlData();
	}

	/**
	 * Provide an alternate "main loop" via startCompetition().
	 */
	public abstract void startCompetition();

	public static boolean getBooleanProperty(String name, boolean defaultValue) {
		String propVal = System.getProperty(name);
		if (propVal == null) {
			return defaultValue;
		}
		if (propVal.equalsIgnoreCase("false")) {
			return false;
		} else if (propVal.equalsIgnoreCase("true")) {
			return true;
		} else {
			throw new IllegalStateException(propVal);
		}
	}

	/**
	 * Write the version string to the root directory
	 */
	protected void writeVersionString() {
		File file = null;
		OutputStream output = null;
		try {
			file = new File(FILE_NAME);

			file.createNewFile();

			output = new FileOutputStream(file);
			PrintStream ps = new PrintStream(output);

			ps.println(VERSION_CONTENTS);
		} catch (Exception ex) {
			ex.printStackTrace();
		} finally {
			if (output != null) {
				try {
					output.close();
				} catch (IOException ex) {
				}
			}
		}
	}

	/**
	 * Starting point for the applications. Starts the OtaServer and then runs
	 * the robot.
	 * 
	 * @throws javax.microedition.midlet.MIDletStateChangeException
	 */
	public final void startApp() {
		boolean errorOnExit = false;

		Watchdog.getInstance().setExpiration(0.1);
		Watchdog.getInstance().setEnabled(false);
		FRCControl.observeUserProgramStarting();
		UsageReporting.report(UsageReporting.kResourceType_Language,
				UsageReporting.kLanguage_Java);

		writeVersionString();

		try {
			this.startCompetition();
		} catch (Throwable t) {
			t.printStackTrace();
			errorOnExit = true;
		} finally {
			// startCompetition never returns unless exception occurs....
			System.err.println("WARNING: Robots don't quit!");
			if (errorOnExit) {
				System.err
						.println("---> The startCompetition() method (or methods called by it) should have handled the exception above.");
			} else {
				System.err
						.println("---> Unexpected return from startCompetition() method.");
			}
		}
	}

	/**
	 * Pauses the application
	 */
	protected final void pauseApp() {
		// This is not currently called by the Squawk VM
	}

	/**
	 * Called if the MIDlet is terminated by the system. I.e. if startApp throws
	 * any exception other than MIDletStateChangeException, if the isolate
	 * running the MIDlet is killed with Isolate.exit(), or if VM.stopVM() is
	 * called.
	 * 
	 * It is not called if MIDlet.notifyDestroyed() was called.
	 * 
	 * @param unconditional
	 *            If true when this method is called, the MIDlet must cleanup
	 *            and release all resources. If false the MIDlet may throw
	 *            MIDletStateChangeException to indicate it does not want to be
	 *            destroyed at this time.
	 * @throws MIDletStateChangeException
	 *             if there is an exception in terminating the midlet
	 */
	protected final void destroyApp(boolean unconditional) {
	}
}
