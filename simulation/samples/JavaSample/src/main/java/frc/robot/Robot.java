// Copyright (c) FIRST and other WPILib contributors.
// Open Source Software; you can modify and/or share it under the terms of
// the WPILib BSD license file in the root directory of this project.

package frc.robot;

import com.revrobotics.CANSparkLowLevel.MotorType;

import com.autodesk.synthesis.io.*;

import edu.wpi.first.wpilibj.SPI;

import edu.wpi.first.wpilibj.ADXL362;
import edu.wpi.first.wpilibj.TimedRobot;
import edu.wpi.first.wpilibj.motorcontrol.Spark;
import edu.wpi.first.wpilibj.smartdashboard.SendableChooser;
import edu.wpi.first.wpilibj.smartdashboard.SmartDashboard;
import edu.wpi.first.wpilibj.XboxController;

import com.autodesk.synthesis.revrobotics.CANSparkMax;
import com.kauailabs.navx.frc.AHRS;
import com.autodesk.synthesis.ctre.TalonFX;

/**
 * The VM is configured to automatically run this class, and to call the
 * functions corresponding to
 * each mode, as described in the TimedRobot documentation. If you change the
 * name of this class or
 * the package after creating this project, you must also update the
 * build.gradle file in the
 * project.
 */
public class Robot extends TimedRobot {
  private static final String kDefaultAuto = "Default";
  private static final String kCustomAuto = "My Auto";
  private String m_autoSelected;
  private final SendableChooser<String> m_chooser = new SendableChooser<>();

  private Spark m_Spark1 = new Spark(0);
  private Spark m_Spark2 = new Spark(1);
  private TalonFX m_Talon = new TalonFX(7);
  private XboxController m_Controller = new XboxController(0);

  private ADXL362 m_Accelerometer = new ADXL362(SPI.Port.kMXP, ADXL362.Range.k8G);
  private AHRS m_Gyro = new AHRS();

  private DigitalInput m_DI = new DigitalInput(0);
  private DigitalOutput m_DO = new DigitalOutput(1);
  private AnalogInput m_AI = new AnalogInput(0);
  private AnalogOutput m_AO = new AnalogOutput(1);

  private CANSparkMax m_SparkMax1 = new CANSparkMax(1, MotorType.kBrushless);
  private CANSparkMax m_SparkMax2 = new CANSparkMax(2, MotorType.kBrushless);
  private CANSparkMax m_SparkMax3 = new CANSparkMax(3, MotorType.kBrushless);
  private CANSparkMax m_SparkMax4 = new CANSparkMax(4, MotorType.kBrushless);
  private CANSparkMax m_SparkMax5 = new CANSparkMax(5, MotorType.kBrushless);
  private CANSparkMax m_SparkMax6 = new CANSparkMax(6, MotorType.kBrushless);

  /**
   * This function is run when the robot is first started up and should be used for any
   * initialization code.
   */
  @Override
  public void robotInit() {
    m_chooser.setDefaultOption("Default Auto", kDefaultAuto);
    m_chooser.addOption("My Auto", kCustomAuto);
    SmartDashboard.putData("Auto choices", m_chooser);
  }

  /**
   * This function is called every 20 ms, no matter the mode. Use this for items like diagnostics
   * that you want ran during disabled, autonomous, teleoperated and test.
   *
   * <p>This runs after the mode specific periodic functions, but before LiveWindow and
   * SmartDashboard integrated updating.
   */
  @Override
  public void robotPeriodic() {}

  /**
   * This autonomous (along with the chooser code above) shows how to select between different
   * autonomous modes using the dashboard. The sendable chooser code works with the Java
   * SmartDashboard. If you prefer the LabVIEW Dashboard, remove all of the chooser code and
   * uncomment the getString line to get the auto name from the text box below the Gyro
   *
   * <p>You can add additional auto modes by adding additional comparisons to the switch structure
   * below with additional strings. If using the SendableChooser make sure to add them to the
   * chooser code above as well.
   */
  @Override
  public void autonomousInit() {
    m_autoSelected = m_chooser.getSelected();
    // m_autoSelected = SmartDashboard.getString("Auto Selector", kDefaultAuto);
    System.out.println("Auto selected: " + m_autoSelected);
    m_DO.set(true);
    m_AO.setVoltage(0.0);
  }

  /** This function is called periodically during autonomous. */
  @Override
  public void autonomousPeriodic() {

    m_Spark1.set(0.5);
    m_Spark2.set(-0.5);
    m_Talon.set(-1.0);

    double position = m_SparkMax1.getAbsoluteEncoderSim().getPosition();

    if (position >= 20) {
        m_SparkMax1.set(0.0);
        m_SparkMax2.set(0.0);
        m_SparkMax3.set(0.0);
        m_SparkMax4.set(0.0);
        m_SparkMax5.set(0.0);
        m_SparkMax6.set(0.0);
    } else {
        m_SparkMax1.set(1.0);
        m_SparkMax2.set(1.0);
        m_SparkMax3.set(1.0);
        m_SparkMax4.set(1.0);
        m_SparkMax5.set(1.0);
        m_SparkMax6.set(1.0);
    }

    switch (m_autoSelected) {
      case kCustomAuto:
        // Put custom auto code here
        break;
      case kDefaultAuto:
      default:
        // Put default auto code here
        break;
    }
  }

  /** This function is called once when teleop is enabled. */
  @Override
  public void teleopInit() {
    m_DO.set(false);
    m_AO.setVoltage(6.0);
  }

  /** This function is called periodically during operator control. */
  @Override
  public void teleopPeriodic() {
    m_SparkMax1.set(m_Controller.getLeftY());
    m_SparkMax2.set(-m_Controller.getRightY());
    m_Talon.set(m_Controller.getLeftX());
    System.out.println("LeftX: " + m_Controller.getLeftX());
    // System.out.println("OUT: " + m_DO.get());
    // System.out.println("AI: " + m_AI.getVoltage());
    // m_Talon.set(-0.5);
    // m_SparkMax1.set(-0.75);
    // m_SparkMax2.set(-0.75);
    m_SparkMax3.set(-0.75);
    m_SparkMax4.set(-0.75);
    m_SparkMax5.set(-0.75);
    m_SparkMax6.set(-0.75);

    m_SparkMax1.getEncoder().setPosition(0.0);
  }


  /** This function is called once when the robot is disabled. */
  @Override
  public void disabledInit() {
      m_SparkMax1.set(0.0);
      m_SparkMax2.set(0.0);
      m_SparkMax3.set(0.0);
      m_SparkMax4.set(0.0);
      m_SparkMax5.set(0.0);
      m_SparkMax6.set(0.0);
      m_AO.setVoltage(12.0);
  }

  /** This function is called periodically when disabled. */
  @Override
  public void disabledPeriodic() {
  }

  /** This function is called once when test mode is enabled. */
  @Override
  public void testInit() {
  }

  /** This function is called periodically during test mode. */
  @Override
  public void testPeriodic() {
  }

  /** This function is called once when the robot is first started up. */
  @Override
  public void simulationInit() {
  }

  /** This function is called periodically whilst in simulation. */
  @Override
  public void simulationPeriodic() {
  }
}
