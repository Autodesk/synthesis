// Copyright (c) FIRST and other WPILib contributors.
// Open Source Software; you can modify and/or share it under the terms of
// the WPILib BSD license file in the root directory of this project.

package frc.robot;

import com.revrobotics.spark.SparkLowLevel.MotorType;

import com.autodesk.synthesis.io.*;
import com.autodesk.synthesis.revrobotics.spark.SparkMax;
import com.autodesk.synthesis.studica.AHRS;
import com.autodesk.synthesis.ctre.TalonFX;

import edu.wpi.first.wpilibj.SPI;
import edu.wpi.first.wpilibj.ADXL362;
import edu.wpi.first.wpilibj.TimedRobot;
import edu.wpi.first.wpilibj.motorcontrol.Spark;
import edu.wpi.first.wpilibj.smartdashboard.SendableChooser;
import edu.wpi.first.wpilibj.smartdashboard.SmartDashboard;
import edu.wpi.first.wpilibj.XboxController;

/**
 * The VM is configured to automatically run this class, and to call the
 * functions corresponding to each mode, as described in the TimedRobot documentation.
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

  private AHRS m_Gyro = new AHRS();
  private ADXL362 m_accelerometer = new ADXL362(SPI.Port.kMXP, ADXL362.Range.k8G);

  private DigitalInput m_DI = new DigitalInput(0);
  private DigitalOutput m_DO = new DigitalOutput(1);
  private AnalogInput m_AI = new AnalogInput(0);
  private AnalogOutput m_AO = new AnalogOutput(1);

  private SparkMax m_SparkMax1 = new SparkMax(1, MotorType.kBrushless);
  private SparkMax m_SparkMax2 = new SparkMax(2, MotorType.kBrushless);
  private SparkMax m_SparkMax3 = new SparkMax(3, MotorType.kBrushless);
  private SparkMax m_SparkMax4 = new SparkMax(4, MotorType.kBrushless);
  private SparkMax m_SparkMax5 = new SparkMax(5, MotorType.kBrushless);
  private SparkMax m_SparkMax6 = new SparkMax(6, MotorType.kBrushless);

  @Override
  public void robotInit() {
    m_chooser.setDefaultOption("Default Auto", kDefaultAuto);
    m_chooser.addOption("My Auto", kCustomAuto);
    SmartDashboard.putData("Auto choices", m_chooser);
  }

  @Override
  public void robotPeriodic() {
    // Required for TalonFX sim: pushes current motor output and encoder state to Synthesis.
    m_Talon.syncSim();

    SmartDashboard.putNumber("Gyro Angle", m_Gyro.getAngle());
    SmartDashboard.putNumber("Accel X", m_accelerometer.getX());
    SmartDashboard.putNumber("Accel Y", m_accelerometer.getY());
    SmartDashboard.putNumber("Accel Z", m_accelerometer.getZ());
  }

  @Override
  public void autonomousInit() {
    m_autoSelected = m_chooser.getSelected();
    System.out.println("Auto selected: " + m_autoSelected);
    m_DO.set(true);
    m_AO.setVoltage(0.0);
  }

  @Override
  public void autonomousPeriodic() {
    m_SparkMax1.set(0.2);
    m_SparkMax2.set(-0.2);

    switch (m_autoSelected) {
      case kCustomAuto:
        break;
      case kDefaultAuto:
      default:
        break;
    }
  }

  @Override
  public void teleopInit() {
    m_DO.set(false);
    m_AO.setVoltage(6.0);
  }

  private double clamp(double a, double min, double max) {
    return Math.min(Math.max(a, min), max);
  }

  @Override
  public void teleopPeriodic() {
    double forward = -m_Controller.getLeftY();
    double turn = m_Controller.getRightX();
    if (Math.abs(forward) < 0.2) forward = 0.0;
    if (Math.abs(turn) < 0.2) turn = 0.0;

    m_SparkMax1.set(clamp(forward + turn, -1, 1));
    m_SparkMax2.set(clamp(forward - turn, -1, 1));

    m_Talon.set(m_Controller.getLeftX());

    m_SparkMax3.set(-0.75);
    m_SparkMax4.set(-0.75);
    m_SparkMax5.set(-0.75);
    m_SparkMax6.set(-0.75);

  }

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

  @Override
  public void disabledPeriodic() {}

  @Override
  public void testInit() {}

  @Override
  public void testPeriodic() {}

  @Override
  public void simulationInit() {}

  @Override
  public void simulationPeriodic() {}
}
