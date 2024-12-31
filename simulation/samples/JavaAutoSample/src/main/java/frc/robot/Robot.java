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
import com.autodesk.synthesis.revrobotics.RelativeEncoder;
import com.autodesk.synthesis.revrobotics.SparkAbsoluteEncoder;
import com.kauailabs.navx.frc.AHRS;
import com.autodesk.synthesis.CANEncoder;
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

    private ADXL362 m_Accelerometer = new ADXL362(SPI.Port.kMXP, ADXL362.Range.k8G);
    private AHRS m_Gyro = new AHRS();

    private CANSparkMax m_sparkLeft = new CANSparkMax(1, MotorType.kBrushless);
    private CANSparkMax m_sparkRight = new CANSparkMax(2, MotorType.kBrushless);
    private CANSparkMax m_sparkArm = new CANSparkMax(3, MotorType.kBrushless);
    private RelativeEncoder m_encoder;

    /**
     * This function is run when the robot is first started up and should be used
     * for any
     * initialization code.
     */
    @Override
    public void robotInit() {
        m_chooser.setDefaultOption("Default Auto", kDefaultAuto);
        m_chooser.addOption("My Auto", kCustomAuto);
        SmartDashboard.putData("Auto choices", m_chooser);

        m_encoder = m_sparkLeft.getEncoderSim();
        // 4 inch diameter wheels, default is 1 unit = 1 radian.
        // Following conversion factor is 1 unit = 1 inch travelled.
        m_encoder.setPositionConversionFactor(2.0);
    }

    /**
     * This function is called every 20 ms, no matter the mode. Use this for items
     * like diagnostics
     * that you want ran during disabled, autonomous, teleoperated and test.
     *
     * <p>
     * This runs after the mode specific periodic functions, but before LiveWindow
     * and
     * SmartDashboard integrated updating.
     */
    @Override
    public void robotPeriodic() {
    }

    /**
     * This autonomous (along with the chooser code above) shows how to select
     * between different
     * autonomous modes using the dashboard. The sendable chooser code works with
     * the Java
     * SmartDashboard. If you prefer the LabVIEW Dashboard, remove all of the
     * chooser code and
     * uncomment the getString line to get the auto name from the text box below the
     * Gyro
     *
     * <p>
     * You can add additional auto modes by adding additional comparisons to the
     * switch structure
     * below with additional strings. If using the SendableChooser make sure to add
     * them to the
     * chooser code above as well.
     */
    @Override
    public void autonomousInit() {
        m_autoSelected = m_chooser.getSelected();
        // m_autoSelected = SmartDashboard.getString("Auto Selector", kDefaultAuto);
        System.out.println("Auto selected: " + m_autoSelected);
        m_encoder.setPosition(0.0);
        m_autoState = AutoState.Stage1;
    }

    enum AutoState {
        Stage1, Stage2
    }
    private AutoState m_autoState = AutoState.Stage1;

    /** This function is called periodically during autonomous. */
    @Override
    public void autonomousPeriodic() {
        switch (m_autoState) {
            case Stage1:
                m_sparkLeft.set(0.5);
                m_sparkRight.set(0.5);
                if (m_encoder.getPosition() > 36.0) {
                    m_autoState = AutoState.Stage2;
                    System.out.println("--- Transitioning to Stage 2 ---");
                }
                break;
            case Stage2:
                m_sparkLeft.set(0.5);
                m_sparkRight.set(-0.5);
                break;
            default:
                break;
        }
    }

    /** This function is called once when teleop is enabled. */
    @Override
    public void teleopInit() {
    }

    /** This function is called periodically during operator control. */
    @Override
    public void teleopPeriodic() {
    }

    /** This function is called once when the robot is disabled. */
    @Override
    public void disabledInit() {
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
