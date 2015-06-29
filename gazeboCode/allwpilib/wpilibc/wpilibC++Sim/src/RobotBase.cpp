/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "RobotBase.h"
#include "RobotState.h"
#include "Utility.h"

#include <cstring>

RobotBase* RobotBase::m_instance = NULL;

void RobotBase::setInstance(RobotBase* robot)
{
	wpi_assert(m_instance == NULL);
	m_instance = robot;
}

RobotBase &RobotBase::getInstance()
{
	return *m_instance;
}

/**
 * Constructor for a generic robot program.
 * User code should be placed in the constuctor that runs before the Autonomous or Operator
 * Control period starts. The constructor will run to completion before Autonomous is entered.
 * 
 * This must be used to ensure that the communications code starts. In the future it would be
 * nice to put this code into it's own task that loads on boot so ensure that it runs.
 */
RobotBase::RobotBase()
{
	m_ds = DriverStation::GetInstance();
    RobotState::SetImplementation(DriverStation::GetInstance());
}

/**
 * Free the resources for a RobotBase class.
 * This includes deleting all classes that might have been allocated as Singletons to they
 * would never be deleted except here.
 */
RobotBase::~RobotBase()
{
}

/**
 * Determine if the Robot is currently enabled.
 * @return True if the Robot is currently enabled by the field controls.
 */
bool RobotBase::IsEnabled()
{
	return m_ds->IsEnabled();
}

/**
 * Determine if the Robot is currently disabled.
 * @return True if the Robot is currently disabled by the field controls.
 */
bool RobotBase::IsDisabled()
{
	return m_ds->IsDisabled();
}

/**
 * Determine if the robot is currently in Autnomous mode.
 * @return True if the robot is currently operating Autonomously as determined by the field controls.
 */
bool RobotBase::IsAutonomous()
{
	return m_ds->IsAutonomous();
}

/**
 * Determine if the robot is currently in Operator Control mode.
 * @return True if the robot is currently operating in Tele-Op mode as determined by the field controls.
 */
bool RobotBase::IsOperatorControl()
{
	return m_ds->IsOperatorControl();
}

/**
 * Determine if the robot is currently in Test mode.
 * @return True if the robot is currently running tests as determined by the field controls.
 */
bool RobotBase::IsTest()
{
    return m_ds->IsTest();
}

/**
 * Indicates if new data is available from the driver station.
 * @return Has new data arrived over the network since the last time this function was called?
 */
// bool RobotBase::IsNewDataAvailable()
// {
// 	return m_ds->IsNewControlData();
// }

/**
 * This class exists for the sole purpose of getting its destructor called when the module unloads.
 * Before the module is done unloading, we need to delete the RobotBase derived singleton.  This should delete
 * the other remaining singletons that were registered.  This should also stop all tasks that are using
 * the Task class.
 */
class RobotDeleter
{
public:
	RobotDeleter() {}
	~RobotDeleter()
	{
		delete &RobotBase::getInstance();
	}
};
static RobotDeleter g_robotDeleter;
