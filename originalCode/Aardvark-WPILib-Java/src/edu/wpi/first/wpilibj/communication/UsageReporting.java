/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj.communication;

import com.ni.rio.NiFpga;
import com.sun.jna.Function;
import com.sun.jna.NativeLibrary;
import com.sun.jna.Pointer;

/**
 * 
 * @author mwills
 */
public final class UsageReporting {

	private UsageReporting() {
	}

	// reporting version
	public static final int kUsageReporting_version = 1;
	// resources
	public static final int kResourceType_Controller = 0;
	public static final int kResourceType_Module = 1;
	public static final int kResourceType_Language = 2;
	public static final int kResourceType_CANPlugin = 3;
	public static final int kResourceType_Accelerometer = 4;
	public static final int kResourceType_ADXL345 = 5;
	public static final int kResourceType_AnalogChannel = 6;
	public static final int kResourceType_AnalogTrigger = 7;
	public static final int kResourceType_AnalogTriggerOutput = 8;
	public static final int kResourceType_CANJaguar = 9;
	public static final int kResourceType_Compressor = 10;
	public static final int kResourceType_Counter = 11;
	public static final int kResourceType_Dashboard = 12;
	public static final int kResourceType_DigitalInput = 13;
	public static final int kResourceType_DigitalOutput = 14;
	public static final int kResourceType_DriverStationCIO = 15;
	public static final int kResourceType_DriverStationEIO = 16;
	public static final int kResourceType_DriverStationLCD = 17;
	public static final int kResourceType_Encoder = 18;
	public static final int kResourceType_GearTooth = 19;
	public static final int kResourceType_Gyro = 20;
	public static final int kResourceType_I2C = 21;
	public static final int kResourceType_Framework = 22;
	public static final int kResourceType_Jaguar = 23;
	public static final int kResourceType_Joystick = 24;
	public static final int kResourceType_Kinect = 25;
	public static final int kResourceType_KinectStick = 26;
	public static final int kResourceType_PIDController = 27;
	public static final int kResourceType_Preferences = 28;
	public static final int kResourceType_PWM = 29;
	public static final int kResourceType_Relay = 30;
	public static final int kResourceType_RobotDrive = 31;
	public static final int kResourceType_SerialPort = 32;
	public static final int kResourceType_Servo = 33;
	public static final int kResourceType_Solenoid = 34;
	public static final int kResourceType_SPI = 35;
	public static final int kResourceType_Task = 36;
	public static final int kResourceType_Ultrasonic = 37;
	public static final int kResourceType_Victor = 38;
	public static final int kResourceType_Button = 39;
	public static final int kResourceType_Command = 40;
	public static final int kResourceType_AxisCamera = 41;
	public static final int kResourceType_PCVideoServer = 42;
	public static final int kResourceType_SmartDashboard = 43;
	public static final int kResourceType_Talon = 44;
	public static final int kResourceType_HiTechnicColorSensor = 45;
	public static final int kResourceType_HiTechnicAccel = 46;
	public static final int kResourceType_HiTechnicCompass = 47;
	public static final int kResourceType_SRF08 = 48;
	// languages
	public static final int kLanguage_LabVIEW = 1;
	public static final int kLanguage_CPlusPlus = 2;
	public static final int kLanguage_Java = 3;
	public static final int kLanguage_Python = 4;
	// CAN
	public static final int kCANPlugin_BlackJagBridge = 1;
	public static final int kCANPlugin_2CAN = 2;
	// Framework
	public static final int kFramework_Iterative = 1;
	public static final int kFramework_Simple = 2;
	// Robot Drive types
	public static final int kRobotDrive_ArcadeStandard = 1;
	public static final int kRobotDrive_ArcadeButtonSpin = 2;
	public static final int kRobotDrive_ArcadeRatioCurve = 3;
	public static final int kRobotDrive_Tank = 4;
	public static final int kRobotDrive_MecanumPolar = 5;
	public static final int kRobotDrive_MecanumCartesian = 6;
	// DS Compatible IO
	public static final int kDriverStationCIO_Analog = 1;
	public static final int kDriverStationCIO_DigitalIn = 2;
	public static final int kDriverStationCIO_DigitalOut = 3;
	// DS Enhanced IO
	public static final int kDriverStationEIO_Acceleration = 1;
	public static final int kDriverStationEIO_AnalogIn = 2;
	public static final int kDriverStationEIO_AnalogOut = 3;
	public static final int kDriverStationEIO_Button = 4;
	public static final int kDriverStationEIO_LED = 5;
	public static final int kDriverStationEIO_DigitalIn = 6;
	public static final int kDriverStationEIO_DigitalOut = 7;
	public static final int kDriverStationEIO_FixedDigitalOut = 8;
	public static final int kDriverStationEIO_PWM = 9;
	public static final int kDriverStationEIO_Encoder = 10;
	public static final int kDriverStationEIO_TouchSlider = 11;
	// Accelerometer connection types
	public static final int kADXL345_SPI = 1;
	public static final int kADXL345_I2C = 2;
	// Commands
	public static final int kCommand_Scheduler = 1;
	// Smart Dashboard
	public static final int kSmartDashboard_Instance = 1;

	private static final Function nUsageReporting_reportFn = NativeLibrary
			.getInstance(NiFpga.LIBRARY_NAME).getFunction(
					"FRC_NetworkCommunication_nUsageReporting_report");

	/**
	 * Report the usage of a resource of interest.
	 * 
	 * @param resource
	 *            one of the values in the tResourceType above (max value 51).
	 * @param instanceNumber
	 *            an index that identifies the resource instance.
	 */
	public static void report(int resource, int instanceNumber) {
		report(resource, instanceNumber, 0);
	}

	/**
	 * Report the usage of a resource of interest.
	 * 
	 * @param resource
	 *            one of the values in the tResourceType above (max value 51).
	 * @param instanceNumber
	 *            an index that identifies the resource instance.
	 * @param context
	 *            an optional additional context number for some cases (such as
	 *            module number). Set to 0 to omit.
	 */
	public static void report(int resource, int instanceNumber, int context) {
		report(resource, instanceNumber, context, null);
	}

	/**
	 * Report the usage of a resource of interest.
	 * 
	 * @param resource
	 *            one of the values in the tResourceType above (max value 51).
	 * @param instanceNumber
	 *            an index that identifies the resource instance.
	 * @param context
	 *            an optional additional context number for some cases (such as
	 *            module number). Set to 0 to omit.
	 * @param feature
	 *            a string to be included describing features in use on a
	 *            specific resource. Setting the same resource more than once
	 *            allows you to change the feature string.
	 */
	public static void report(int resource, int instanceNumber, int context,
			String feature) {
		if (feature != null) {
			Pointer featureStringPointer = new Pointer(feature.length() + 1);
			featureStringPointer.write(0, feature.getBytes(), 0,
					feature.length());
			featureStringPointer.setByte(feature.length(), (byte) 0);
			nUsageReporting_reportFn.invokeVoid(new Object[] { resource,
					instanceNumber, context, featureStringPointer });
			// featureStringPointer.free();//TODO check to see if this can get
			// called or not
		} else {
			nUsageReporting_reportFn.invokeVoid(new Object[] { resource,
					instanceNumber, context, Pointer.NULL });
		}
	}
}
