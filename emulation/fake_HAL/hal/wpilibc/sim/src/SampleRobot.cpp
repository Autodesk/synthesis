/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "SampleRobot.h"

#include <cstdio>

#include "LiveWindow/LiveWindow.h"
#include "Timer.h"
#include "networktables/NetworkTable.h"

#if defined(_UNIX)
#include <unistd.h>
#elif defined(_WIN32)
#include <windows.h>
void sleep(unsigned int milliseconds) { Sleep(milliseconds); }
#endif

using namespace frc;

SampleRobot::SampleRobot() : m_robotMainOverridden(true) {}

/**
 * Robot-wide initialization code should go here.
 *
 * Programmers should override this method for default Robot-wide initialization
 * which will be called each time the robot enters the disabled state.
 */
void SampleRobot::RobotInit() {
  std::printf("Default %s() method... Override me!\n", __FUNCTION__);
}

/**
 * Disabled should go here.
 *
 * Programmers should override this method to run code that should run while the
 * field is disabled.
 */
void SampleRobot::Disabled() {
  std::printf("Default %s() method... Override me!\n", __FUNCTION__);
}

/**
 * Autonomous should go here.
 *
 * Programmers should override this method to run code that should run while the
 * field is in the autonomous period. This will be called once each time the
 * robot enters the autonomous state.
 */
void SampleRobot::Autonomous() {
  std::printf("Default %s() method... Override me!\n", __FUNCTION__);
}

/**
 * Operator control (tele-operated) code should go here.
 *
 * Programmers should override this method to run code that should run while the
 * field is in the Operator Control (tele-operated) period. This is called once
 * each time the robot enters the teleop state.
 */
void SampleRobot::OperatorControl() {
  std::printf("Default %s() method... Override me!\n", __FUNCTION__);
}

/**
 * Test program should go here.
 *
 * Programmers should override this method to run code that executes while the
 * robot is in test mode. This will be called once whenever the robot enters
 * test mode.
 */
void SampleRobot::Test() {
  std::printf("Default %s() method... Override me!\n", __FUNCTION__);
}

/**
 * Robot main program for free-form programs.
 *
 * This should be overridden by user subclasses if the intent is to not use the
 * Autonomous() and OperatorControl() methods. In that case, the program is
 * responsible for sensing when to run the autonomous and operator control
 * functions in their program.
 *
 * This method will be called immediately after the constructor is called. If it
 * has not been overridden by a user subclass (i.e. the default version runs),
 * then the Autonomous() and OperatorControl() methods will be called.
 */
void SampleRobot::RobotMain() { m_robotMainOverridden = false; }

/**
 * Start a competition.
 *
 * This code needs to track the order of the field starting to ensure that
 * everything happens in the right order. Repeatedly run the correct method,
 * either Autonomous or OperatorControl or Test when the robot is enabled.
 * After running the correct method, wait for some state to change, either the
 * other mode starts or the robot is disabled. Then go back and wait for the
 * robot to be enabled again.
 */
void SampleRobot::StartCompetition() {
  LiveWindow* lw = LiveWindow::GetInstance();

  NetworkTable::GetTable("LiveWindow")
      ->GetSubTable("~STATUS~")
      ->PutBoolean("LW Enabled", false);

  RobotMain();

  if (!m_robotMainOverridden) {
    // first and one-time initialization
    lw->SetEnabled(false);
    RobotInit();

    while (true) {
      if (IsDisabled()) {
        m_ds.InDisabled(true);
        Disabled();
        m_ds.InDisabled(false);
        while (IsDisabled()) sleep(1);  // m_ds.WaitForData();
      } else if (IsAutonomous()) {
        m_ds.InAutonomous(true);
        Autonomous();
        m_ds.InAutonomous(false);
        while (IsAutonomous() && IsEnabled()) sleep(1);  // m_ds.WaitForData();
      } else if (IsTest()) {
        lw->SetEnabled(true);
        m_ds.InTest(true);
        Test();
        m_ds.InTest(false);
        while (IsTest() && IsEnabled()) sleep(1);  // m_ds.WaitForData();
        lw->SetEnabled(false);
      } else {
        m_ds.InOperatorControl(true);
        OperatorControl();
        m_ds.InOperatorControl(false);
        while (IsOperatorControl() && IsEnabled())
          sleep(1);  // m_ds.WaitForData();
      }
    }
  }
}
