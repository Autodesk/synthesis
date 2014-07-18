/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef ROBOT_ITERATIVE_H_
#define ROBOT_ITERATIVE_H_

#include "Timer.h"
#include "RobotBase.h"

/**
 * IterativeRobot implements a specific type of Robot Program framework, extending the RobotBase class.
 * 
 * The IterativeRobot class is intended to be subclassed by a user creating a robot program.
 * 
 * This class is intended to implement the "old style" default code, by providing
 * the following functions which are called by the main loop, StartCompetition(), at the appropriate times:
 * 
 * RobotInit() -- provide for initialization at robot power-on
 * 
 * Init() functions -- each of the following functions is called once when the
 *                     appropriate mode is entered:
 *  - DisabledInit()   -- called only when first disabled
 *  - AutonomousInit() -- called each and every time autonomous is entered from another mode
 *  - TeleopInit()     -- called each and every time teleop is entered from another mode
 *  - TestInit()       -- called each and every time test is entered from another mode
 * 
 * Periodic() functions -- each of these functions is called iteratively at the
 *                         appropriate periodic rate (aka the "slow loop").  The default period of
 *                         the iterative robot is synced to the driver station control packets,
 *                         giving a periodic frequency of about 50Hz (50 times per second).
 *   - DisabledPeriodic()
 *   - AutonomousPeriodic()
 *   - TeleopPeriodic()
 *   - TestPeriodic()
 * 
 */

class IterativeRobot : public RobotBase {
public:
	/*
	 * The default period for the periodic function calls (seconds)
	 * Setting the period to 0.0 will cause the periodic functions to follow
	 * the Driver Station packet rate of about 50Hz.
	 */
	static constexpr double kDefaultPeriod = 0.0;

	virtual void StartCompetition();

	virtual void RobotInit();
	virtual void DisabledInit();
	virtual void AutonomousInit();
    virtual void TeleopInit();
    virtual void TestInit();

	virtual void DisabledPeriodic();
	virtual void AutonomousPeriodic();
    virtual void TeleopPeriodic();
    virtual void TestPeriodic();

	void SetPeriod(double period);
	double GetPeriod();
	double GetLoopsPerSec();

protected:
	virtual ~IterativeRobot();
	IterativeRobot();

private:
	bool NextPeriodReady();

	bool m_disabledInitialized;
	bool m_autonomousInitialized;
    bool m_teleopInitialized;
    bool m_testInitialized;
	double m_period;
	Timer m_mainLoopTimer;
};

#endif

