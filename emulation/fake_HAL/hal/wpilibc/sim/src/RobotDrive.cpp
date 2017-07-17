/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "RobotDrive.h"

#include <algorithm>
#include <cmath>

#include "GenericHID.h"
#include "Joystick.h"
#include "Talon.h"
#include "Utility.h"
#include "WPIErrors.h"

using namespace frc;

const int RobotDrive::kMaxNumberOfMotors;

static auto make_shared_nodelete(SpeedController* ptr) {
  return std::shared_ptr<SpeedController>(ptr, NullDeleter<SpeedController>());
}

/*
 * Driving functions.
 *
 * These functions provide an interface to multiple motors that is used for C
 * programming.
 * The Drive(speed, direction) function is the main part of the set that makes
 * it easy to set speeds and direction independently in one call.
 */

/**
 * Common function to initialize all the robot drive constructors.
 * Create a motor safety object (the real reason for the common code) and
 * initialize all the motor assignments. The default timeout is set for the
 * robot drive.
 */
void RobotDrive::InitRobotDrive() {
  // FIXME: m_safetyHelper = new MotorSafetyHelper(this);
  // FIXME: m_safetyHelper->SetSafetyEnabled(true);
}

/**
 * Constructor for RobotDrive with 2 motors specified with channel numbers.
 *
 * Set up parameters for a two wheel drive system where the
 * left and right motor pwm channels are specified in the call.
 * This call assumes Talons for controlling the motors.
 *
 * @param leftMotorChannel  The PWM channel number that drives the left motor.
 * @param rightMotorChannel The PWM channel number that drives the right motor.
 */
RobotDrive::RobotDrive(int leftMotorChannel, int rightMotorChannel) {
  InitRobotDrive();
  m_rearLeftMotor = std::make_shared<Talon>(leftMotorChannel);
  m_rearRightMotor = std::make_shared<Talon>(rightMotorChannel);
  SetLeftRightMotorOutputs(0.0, 0.0);
  m_deleteSpeedControllers = true;
}

/**
 * Constructor for RobotDrive with 4 motors specified with channel numbers.
 *
 * Set up parameters for a four wheel drive system where all four motor
 * pwm channels are specified in the call.
 * This call assumes Talons for controlling the motors.
 *
 * @param frontLeftMotor  Front left motor channel number
 * @param rearLeftMotor   Rear Left motor channel number
 * @param frontRightMotor Front right motor channel number
 * @param rearRightMotor  Rear Right motor channel number
 */
RobotDrive::RobotDrive(int frontLeftMotor, int rearLeftMotor,
                       int frontRightMotor, int rearRightMotor) {
  InitRobotDrive();
  m_rearLeftMotor = std::make_shared<Talon>(rearLeftMotor);
  m_rearRightMotor = std::make_shared<Talon>(rearRightMotor);
  m_frontLeftMotor = std::make_shared<Talon>(frontLeftMotor);
  m_frontRightMotor = std::make_shared<Talon>(frontRightMotor);
  SetLeftRightMotorOutputs(0.0, 0.0);
  m_deleteSpeedControllers = true;
}

/**
 * Constructor for RobotDrive with 2 motors specified as SpeedController
 * objects.
 *
 * The SpeedController version of the constructor enables programs to use the
 * RobotDrive classes with subclasses of the SpeedController objects, for
 * example, versions with ramping or reshaping of the curve to suit motor bias
 * or deadband elimination.
 *
 * @param leftMotor  The left SpeedController object used to drive the robot.
 * @param rightMotor the right SpeedController object used to drive the robot.
 */
RobotDrive::RobotDrive(SpeedController* leftMotor,
                       SpeedController* rightMotor) {
  InitRobotDrive();
  if (leftMotor == nullptr || rightMotor == nullptr) {
    wpi_setWPIError(NullParameter);
    m_rearLeftMotor = m_rearRightMotor = nullptr;
    return;
  }
  m_rearLeftMotor = make_shared_nodelete(leftMotor);
  m_rearRightMotor = make_shared_nodelete(rightMotor);
}

// TODO: Change to rvalue references & move syntax.
RobotDrive::RobotDrive(SpeedController& leftMotor,
                       SpeedController& rightMotor) {
  InitRobotDrive();
  m_rearLeftMotor = make_shared_nodelete(&leftMotor);
  m_rearRightMotor = make_shared_nodelete(&rightMotor);
}

RobotDrive::RobotDrive(std::shared_ptr<SpeedController> leftMotor,
                       std::shared_ptr<SpeedController> rightMotor) {
  InitRobotDrive();
  if (leftMotor == nullptr || rightMotor == nullptr) {
    wpi_setWPIError(NullParameter);
    m_rearLeftMotor = m_rearRightMotor = nullptr;
    return;
  }
  m_rearLeftMotor = leftMotor;
  m_rearRightMotor = rightMotor;
}

/**
 * Constructor for RobotDrive with 4 motors specified as SpeedController
 * objects.
 *
 * Speed controller input version of RobotDrive (see previous comments).
 *
 * @param rearLeftMotor   The back left SpeedController object used to drive the
 *                        robot.
 * @param frontLeftMotor  The front left SpeedController object used to drive
 *                        the robot
 * @param rearRightMotor  The back right SpeedController object used to drive
 *                        the robot.
 * @param frontRightMotor The front right SpeedController object used to drive
 *                        the robot.
 */
RobotDrive::RobotDrive(SpeedController* frontLeftMotor,
                       SpeedController* rearLeftMotor,
                       SpeedController* frontRightMotor,
                       SpeedController* rearRightMotor) {
  InitRobotDrive();
  if (frontLeftMotor == nullptr || rearLeftMotor == nullptr ||
      frontRightMotor == nullptr || rearRightMotor == nullptr) {
    wpi_setWPIError(NullParameter);
    return;
  }
  m_frontLeftMotor = make_shared_nodelete(frontLeftMotor);
  m_rearLeftMotor = make_shared_nodelete(rearLeftMotor);
  m_frontRightMotor = make_shared_nodelete(frontRightMotor);
  m_rearRightMotor = make_shared_nodelete(rearRightMotor);
}

RobotDrive::RobotDrive(SpeedController& frontLeftMotor,
                       SpeedController& rearLeftMotor,
                       SpeedController& frontRightMotor,
                       SpeedController& rearRightMotor) {
  InitRobotDrive();
  m_frontLeftMotor = make_shared_nodelete(&frontLeftMotor);
  m_rearLeftMotor = make_shared_nodelete(&rearLeftMotor);
  m_frontRightMotor = make_shared_nodelete(&frontRightMotor);
  m_rearRightMotor = make_shared_nodelete(&rearRightMotor);
}

RobotDrive::RobotDrive(std::shared_ptr<SpeedController> frontLeftMotor,
                       std::shared_ptr<SpeedController> rearLeftMotor,
                       std::shared_ptr<SpeedController> frontRightMotor,
                       std::shared_ptr<SpeedController> rearRightMotor) {
  InitRobotDrive();
  if (frontLeftMotor == nullptr || rearLeftMotor == nullptr ||
      frontRightMotor == nullptr || rearRightMotor == nullptr) {
    wpi_setWPIError(NullParameter);
    return;
  }
  m_frontLeftMotor = frontLeftMotor;
  m_rearLeftMotor = rearLeftMotor;
  m_frontRightMotor = frontRightMotor;
  m_rearRightMotor = rearRightMotor;
}

/**
 * Drive the motors at "outputMagnitude" and "curve".
 * Both outputMagnitude and curve are -1.0 to +1.0 values, where 0.0 represents
 * stopped and not turning. curve < 0 will turn left and curve > 0 will turn
 * right.
 *
 * The algorithm for steering provides a constant turn radius for any normal
 * speed range, both forward and backward. Increasing m_sensitivity causes
 * sharper turns for fixed values of curve.
 *
 * This function will most likely be used in an autonomous routine.
 *
 * @param outputMagnitude The speed setting for the outside wheel in a turn,
 *                        forward or backwards, +1 to -1.
 * @param curve           The rate of turn, constant for different forward
 *                        speeds. Set curve < 0 for left turn or curve > 0 for
 *                        right turn. Set curve = e^(-r/w) to get a turn radius
 *                        r for wheelbase w of your robot. Conversely, turn
 *                        radius r = -ln(curve)*w for a given value of curve and
 *                        wheelbase w.
 */
void RobotDrive::Drive(double outputMagnitude, double curve) {
  double leftOutput, rightOutput;
  static bool reported = false;
  if (!reported) {
    reported = true;
  }

  if (curve < 0) {
    double value = std::log(-curve);
    double ratio = (value - m_sensitivity) / (value + m_sensitivity);
    if (ratio == 0) ratio = .0000000001;
    leftOutput = outputMagnitude / ratio;
    rightOutput = outputMagnitude;
  } else if (curve > 0) {
    double value = std::log(curve);
    double ratio = (value - m_sensitivity) / (value + m_sensitivity);
    if (ratio == 0) ratio = .0000000001;
    leftOutput = outputMagnitude;
    rightOutput = outputMagnitude / ratio;
  } else {
    leftOutput = outputMagnitude;
    rightOutput = outputMagnitude;
  }
  SetLeftRightMotorOutputs(leftOutput, rightOutput);
}

/**
 * Provide tank steering using the stored robot configuration.
 *
 * Drive the robot using two joystick inputs. The Y-axis will be selected from
 * each Joystick object.
 *
 * @param leftStick  The joystick to control the left side of the robot.
 * @param rightStick The joystick to control the right side of the robot.
 */
void RobotDrive::TankDrive(GenericHID* leftStick, GenericHID* rightStick,
                           bool squaredInputs) {
  if (leftStick == nullptr || rightStick == nullptr) {
    wpi_setWPIError(NullParameter);
    return;
  }
  TankDrive(leftStick->GetY(), rightStick->GetY(), squaredInputs);
}

void RobotDrive::TankDrive(GenericHID& leftStick, GenericHID& rightStick,
                           bool squaredInputs) {
  TankDrive(leftStick.GetY(), rightStick.GetY(), squaredInputs);
}

/**
 * Provide tank steering using the stored robot configuration.
 *
 * This function lets you pick the axis to be used on each Joystick object for
 * the left and right sides of the robot.
 *
 * @param leftStick  The Joystick object to use for the left side of the robot.
 * @param leftAxis   The axis to select on the left side Joystick object.
 * @param rightStick The Joystick object to use for the right side of the robot.
 * @param rightAxis  The axis to select on the right side Joystick object.
 */
void RobotDrive::TankDrive(GenericHID* leftStick, int leftAxis,
                           GenericHID* rightStick, int rightAxis,
                           bool squaredInputs) {
  if (leftStick == nullptr || rightStick == nullptr) {
    wpi_setWPIError(NullParameter);
    return;
  }
  TankDrive(leftStick->GetRawAxis(leftAxis), rightStick->GetRawAxis(rightAxis),
            squaredInputs);
}

void RobotDrive::TankDrive(GenericHID& leftStick, int leftAxis,
                           GenericHID& rightStick, int rightAxis,
                           bool squaredInputs) {
  TankDrive(leftStick.GetRawAxis(leftAxis), rightStick.GetRawAxis(rightAxis),
            squaredInputs);
}

/**
 * Provide tank steering using the stored robot configuration.
 *
 * This function lets you directly provide joystick values from any source.
 *
 * @param leftValue  The value of the left stick.
 * @param rightValue The value of the right stick.
 */
void RobotDrive::TankDrive(double leftValue, double rightValue,
                           bool squaredInputs) {
  static bool reported = false;
  if (!reported) {
    reported = true;
  }

  // square the inputs (while preserving the sign) to increase fine control
  // while permitting full power
  leftValue = Limit(leftValue);
  rightValue = Limit(rightValue);
  if (squaredInputs) {
    if (leftValue >= 0.0) {
      leftValue = (leftValue * leftValue);
    } else {
      leftValue = -(leftValue * leftValue);
    }
    if (rightValue >= 0.0) {
      rightValue = (rightValue * rightValue);
    } else {
      rightValue = -(rightValue * rightValue);
    }
  }

  SetLeftRightMotorOutputs(leftValue, rightValue);
}

/**
 * Arcade drive implements single stick driving.
 *
 * Given a single Joystick, the class assumes the Y axis for the move value and
 * the X axis for the rotate value.
 * (Should add more information here regarding the way that arcade drive works.)
 *
 * @param stick         The joystick to use for Arcade single-stick driving.
 *                      The Y-axis will be selected for forwards/backwards and
 *                      the X-axis will be selected for rotation rate.
 * @param squaredInputs If true, the sensitivity will be increased for small
 *                      values
 */
void RobotDrive::ArcadeDrive(GenericHID* stick, bool squaredInputs) {
  // simply call the full-featured ArcadeDrive with the appropriate values
  ArcadeDrive(stick->GetY(), stick->GetX(), squaredInputs);
}

/**
 * Arcade drive implements single stick driving.
 *
 * Given a single Joystick, the class assumes the Y axis for the move value and
 * the X axis for the rotate value.
 * (Should add more information here regarding the way that arcade drive works.)
 *
 * @param stick         The joystick to use for Arcade single-stick driving.
 *                      The Y-axis will be selected for forwards/backwards and
 *                      the X-axis will be selected for rotation rate.
 * @param squaredInputs If true, the sensitivity will be increased for small
 *                      values
 */
void RobotDrive::ArcadeDrive(GenericHID& stick, bool squaredInputs) {
  // simply call the full-featured ArcadeDrive with the appropriate values
  ArcadeDrive(stick.GetY(), stick.GetX(), squaredInputs);
}

/**
 * Arcade drive implements single stick driving.
 *
 * Given two joystick instances and two axis, compute the values to send to
 * either two or four motors.
 *
 * @param moveStick     The Joystick object that represents the forward/backward
 *                      direction
 * @param moveAxis      The axis on the moveStick object to use for
 *                      forwards/backwards (typically Y_AXIS)
 * @param rotateStick   The Joystick object that represents the rotation value
 * @param rotateAxis    The axis on the rotation object to use for the rotate
 *                      right/left (typically X_AXIS)
 * @param squaredInputs Setting this parameter to true increases the sensitivity
 *                      at lower speeds
 */
void RobotDrive::ArcadeDrive(GenericHID* moveStick, int moveAxis,
                             GenericHID* rotateStick, int rotateAxis,
                             bool squaredInputs) {
  double moveValue = moveStick->GetRawAxis(moveAxis);
  double rotateValue = rotateStick->GetRawAxis(rotateAxis);

  ArcadeDrive(moveValue, rotateValue, squaredInputs);
}

/**
 * Arcade drive implements single stick driving.
 *
 * Given two joystick instances and two axis, compute the values to send to
 * either two or four motors.
 *
 * @param moveStick     The Joystick object that represents the forward/backward
 *                      direction
 * @param moveAxis      The axis on the moveStick object to use for
 *                      forwards/backwards (typically Y_AXIS)
 * @param rotateStick   The Joystick object that represents the rotation value
 * @param rotateAxis    The axis on the rotation object to use for the rotate
 *                      right/left (typically X_AXIS)
 * @param squaredInputs Setting this parameter to true increases the sensitivity
 *                      at lower speeds
 */
void RobotDrive::ArcadeDrive(GenericHID& moveStick, int moveAxis,
                             GenericHID& rotateStick, int rotateAxis,
                             bool squaredInputs) {
  double moveValue = moveStick.GetRawAxis(moveAxis);
  double rotateValue = rotateStick.GetRawAxis(rotateAxis);

  ArcadeDrive(moveValue, rotateValue, squaredInputs);
}

/**
 * Arcade drive implements single stick driving.
 *
 * This function lets you directly provide joystick values from any source.
 *
 * @param moveValue     The value to use for fowards/backwards
 * @param rotateValue   The value to use for the rotate right/left
 * @param squaredInputs If set, increases the sensitivity at low speeds
 */
void RobotDrive::ArcadeDrive(double moveValue, double rotateValue,
                             bool squaredInputs) {
  static bool reported = false;
  if (!reported) {
    reported = true;
  }

  // local variables to hold the computed PWM values for the motors
  double leftMotorOutput;
  double rightMotorOutput;

  moveValue = Limit(moveValue);
  rotateValue = Limit(rotateValue);

  if (squaredInputs) {
    // square the inputs (while preserving the sign) to increase fine control
    // while permitting full power
    if (moveValue >= 0.0) {
      moveValue = (moveValue * moveValue);
    } else {
      moveValue = -(moveValue * moveValue);
    }
    if (rotateValue >= 0.0) {
      rotateValue = (rotateValue * rotateValue);
    } else {
      rotateValue = -(rotateValue * rotateValue);
    }
  }

  if (moveValue > 0.0) {
    if (rotateValue > 0.0) {
      leftMotorOutput = moveValue - rotateValue;
      rightMotorOutput = std::max(moveValue, rotateValue);
    } else {
      leftMotorOutput = std::max(moveValue, -rotateValue);
      rightMotorOutput = moveValue + rotateValue;
    }
  } else {
    if (rotateValue > 0.0) {
      leftMotorOutput = -std::max(-moveValue, rotateValue);
      rightMotorOutput = moveValue + rotateValue;
    } else {
      leftMotorOutput = moveValue - rotateValue;
      rightMotorOutput = -std::max(-moveValue, -rotateValue);
    }
  }
  SetLeftRightMotorOutputs(leftMotorOutput, rightMotorOutput);
}

/**
 * Drive method for Mecanum wheeled robots.
 *
 * A method for driving with Mecanum wheeled robots. There are 4 wheels
 * on the robot, arranged so that the front and back wheels are toed in 45
 * degrees. When looking at the wheels from the top, the roller axles should
 * form an X across the robot.
 *
 * This is designed to be directly driven by joystick axes.
 *
 * @param x         The speed that the robot should drive in the X direction.
 *                  [-1.0..1.0]
 * @param y         The speed that the robot should drive in the Y direction.
 *                  This input is inverted to match the forward == -1.0 that
 *                  joysticks produce. [-1.0..1.0]
 * @param rotation  The rate of rotation for the robot that is completely
 *                  independent of the translation. [-1.0..1.0]
 * @param gyroAngle The current angle reading from the gyro.  Use this to
 *                  implement field-oriented controls.
 */
void RobotDrive::MecanumDrive_Cartesian(double x, double y, double rotation,
                                        double gyroAngle) {
  static bool reported = false;
  if (!reported) {
    reported = true;
  }

  double xIn = x;
  double yIn = y;
  // Negate y for the joystick.
  yIn = -yIn;
  // Compenstate for gyro angle.
  RotateVector(xIn, yIn, gyroAngle);

  double wheelSpeeds[kMaxNumberOfMotors];
  wheelSpeeds[kFrontLeftMotor] = xIn + yIn + rotation;
  wheelSpeeds[kFrontRightMotor] = -xIn + yIn - rotation;
  wheelSpeeds[kRearLeftMotor] = -xIn + yIn + rotation;
  wheelSpeeds[kRearRightMotor] = xIn + yIn - rotation;

  Normalize(wheelSpeeds);

  m_frontLeftMotor->Set(wheelSpeeds[kFrontLeftMotor] *
                        m_invertedMotors[kFrontLeftMotor] * m_maxOutput);
  m_frontRightMotor->Set(wheelSpeeds[kFrontRightMotor] *
                         m_invertedMotors[kFrontRightMotor] * m_maxOutput);
  m_rearLeftMotor->Set(wheelSpeeds[kRearLeftMotor] *
                       m_invertedMotors[kRearLeftMotor] * m_maxOutput);
  m_rearRightMotor->Set(wheelSpeeds[kRearRightMotor] *
                        m_invertedMotors[kRearRightMotor] * m_maxOutput);

  // FIXME: m_safetyHelper->Feed();
}

/**
 * Drive method for Mecanum wheeled robots.
 *
 * A method for driving with Mecanum wheeled robots. There are 4 wheels
 * on the robot, arranged so that the front and back wheels are toed in 45
 * degrees. When looking at the wheels from the top, the roller axles should
 * form an X across the robot.
 *
 * @param magnitude The speed that the robot should drive in a given direction.
 *                  [-1.0..1.0]
 * @param direction The direction the robot should drive in degrees. The
 *                  direction and maginitute are independent of the rotation
 *                  rate.
 * @param rotation  The rate of rotation for the robot that is completely
 *                  independent of the magnitute or direction. [-1.0..1.0]
 */
void RobotDrive::MecanumDrive_Polar(double magnitude, double direction,
                                    double rotation) {
  static bool reported = false;
  if (!reported) {
    reported = true;
  }

  // Normalized for full power along the Cartesian axes.
  magnitude = Limit(magnitude) * std::sqrt(2.0);
  // The rollers are at 45 degree angles.
  double dirInRad = (direction + 45.0) * 3.14159 / 180.0;
  double cosD = std::cos(dirInRad);
  double sinD = std::sin(dirInRad);

  double wheelSpeeds[kMaxNumberOfMotors];
  wheelSpeeds[kFrontLeftMotor] = sinD * magnitude + rotation;
  wheelSpeeds[kFrontRightMotor] = cosD * magnitude - rotation;
  wheelSpeeds[kRearLeftMotor] = cosD * magnitude + rotation;
  wheelSpeeds[kRearRightMotor] = sinD * magnitude - rotation;

  Normalize(wheelSpeeds);

  m_frontLeftMotor->Set(wheelSpeeds[kFrontLeftMotor] *
                        m_invertedMotors[kFrontLeftMotor] * m_maxOutput);
  m_frontRightMotor->Set(wheelSpeeds[kFrontRightMotor] *
                         m_invertedMotors[kFrontRightMotor] * m_maxOutput);
  m_rearLeftMotor->Set(wheelSpeeds[kRearLeftMotor] *
                       m_invertedMotors[kRearLeftMotor] * m_maxOutput);
  m_rearRightMotor->Set(wheelSpeeds[kRearRightMotor] *
                        m_invertedMotors[kRearRightMotor] * m_maxOutput);

  // FIXME: m_safetyHelper->Feed();
}

/**
 * Holonomic Drive method for Mecanum wheeled robots.
 *
 * This is an alias to MecanumDrive_Polar() for backward compatability
 *
 * @param magnitude The speed that the robot should drive in a given direction.
 *                  [-1.0..1.0]
 * @param direction The direction the robot should drive. The direction and
 *                  magnitude are independent of the rotation rate.
 * @param rotation  The rate of rotation for the robot that is completely
 *                  independent of the magnitude or direction.  [-1.0..1.0]
 */
void RobotDrive::HolonomicDrive(double magnitude, double direction,
                                double rotation) {
  MecanumDrive_Polar(magnitude, direction, rotation);
}

/**
 * Set the speed of the right and left motors.
 *
 * This is used once an appropriate drive setup function is called such as
 * TwoWheelDrive(). The motors are set to "leftOutput" and "rightOutput"
 * and includes flipping the direction of one side for opposing motors.
 *
 * @param leftOutput  The speed to send to the left side of the robot.
 * @param rightOutput The speed to send to the right side of the robot.
 */
void RobotDrive::SetLeftRightMotorOutputs(double leftOutput,
                                          double rightOutput) {
  wpi_assert(m_rearLeftMotor != nullptr && m_rearRightMotor != nullptr);

  if (m_frontLeftMotor != nullptr)
    m_frontLeftMotor->Set(Limit(leftOutput) *
                          m_invertedMotors[kFrontLeftMotor] * m_maxOutput);
  m_rearLeftMotor->Set(Limit(leftOutput) * m_invertedMotors[kRearLeftMotor] *
                       m_maxOutput);

  if (m_frontRightMotor != nullptr)
    m_frontRightMotor->Set(-Limit(rightOutput) *
                           m_invertedMotors[kFrontRightMotor] * m_maxOutput);
  m_rearRightMotor->Set(-Limit(rightOutput) *
                        m_invertedMotors[kRearRightMotor] * m_maxOutput);

  // FIXME: m_safetyHelper->Feed();
}

/**
 * Limit motor values to the -1.0 to +1.0 range.
 */
double RobotDrive::Limit(double num) {
  if (num > 1.0) {
    return 1.0;
  }
  if (num < -1.0) {
    return -1.0;
  }
  return num;
}

/**
 * Normalize all wheel speeds if the magnitude of any wheel is greater than 1.0.
 */
void RobotDrive::Normalize(double* wheelSpeeds) {
  double maxMagnitude = std::fabs(wheelSpeeds[0]);
  for (int i = 1; i < kMaxNumberOfMotors; i++) {
    double temp = std::fabs(wheelSpeeds[i]);
    if (maxMagnitude < temp) maxMagnitude = temp;
  }
  if (maxMagnitude > 1.0) {
    for (int i = 0; i < kMaxNumberOfMotors; i++) {
      wheelSpeeds[i] = wheelSpeeds[i] / maxMagnitude;
    }
  }
}

/**
 * Rotate a vector in Cartesian space.
 */
void RobotDrive::RotateVector(double& x, double& y, double angle) {
  double cosA = std::cos(angle * (3.14159 / 180.0));
  double sinA = std::sin(angle * (3.14159 / 180.0));
  double xOut = x * cosA - y * sinA;
  double yOut = x * sinA + y * cosA;
  x = xOut;
  y = yOut;
}

/**
 * Invert a motor direction.
 *
 * This is used when a motor should run in the opposite direction as the drive
 * code would normally run it. Motors that are direct drive would be inverted,
 * the Drive code assumes that the motors are geared with one reversal.
 *
 * @param motor      The motor index to invert.
 * @param isInverted True if the motor should be inverted when operated.
 */
void RobotDrive::SetInvertedMotor(MotorType motor, bool isInverted) {
  if (motor < 0 || motor > 3) {
    wpi_setWPIError(InvalidMotorIndex);
    return;
  }
  m_invertedMotors[motor] = isInverted ? -1 : 1;
}

/**
 * Set the turning sensitivity.
 *
 * This only impacts the Drive() entry-point.
 *
 * @param sensitivity Effectively sets the turning sensitivity (or turn radius
 *                    for a given value)
 */
void RobotDrive::SetSensitivity(double sensitivity) {
  m_sensitivity = sensitivity;
}

/**
 * Configure the scaling factor for using RobotDrive with motor controllers in a
 * mode other than PercentVbus.
 *
 * @param maxOutput Multiplied with the output percentage computed by the drive
 *                  functions.
 */
void RobotDrive::SetMaxOutput(double maxOutput) { m_maxOutput = maxOutput; }

void RobotDrive::SetExpiration(double timeout) {
  // FIXME: m_safetyHelper->SetExpiration(timeout);
}

double RobotDrive::GetExpiration() const {
  return -1;  // FIXME: return m_safetyHelper->GetExpiration();
}

bool RobotDrive::IsAlive() const {
  return true;  // FIXME: m_safetyHelper->IsAlive();
}

bool RobotDrive::IsSafetyEnabled() const {
  return false;  // FIXME: return m_safetyHelper->IsSafetyEnabled();
}

void RobotDrive::SetSafetyEnabled(bool enabled) {
  // FIXME: m_safetyHelper->SetSafetyEnabled(enabled);
}

void RobotDrive::GetDescription(llvm::raw_ostream& desc) const {
  desc << "RobotDrive";
}

void RobotDrive::StopMotor() {
  if (m_frontLeftMotor != nullptr) m_frontLeftMotor->Disable();
  if (m_frontRightMotor != nullptr) m_frontRightMotor->Disable();
  if (m_rearLeftMotor != nullptr) m_rearLeftMotor->Disable();
  if (m_rearRightMotor != nullptr) m_rearRightMotor->Disable();
}
