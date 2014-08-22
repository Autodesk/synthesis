/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __WAIT_UNTIL_COMMAND_H__
#define __WAIT_UNTIL_COMMAND_H__

#include "Commands/Command.h"

class WaitUntilCommand : public Command
{
public:
	WaitUntilCommand(double time);
	WaitUntilCommand(const char *name, double time);
	virtual ~WaitUntilCommand() {}

protected:
	virtual void Initialize();
	virtual void Execute();
	virtual bool IsFinished();
	virtual void End();
	virtual void Interrupted();	

private:
	double m_time;
};

#endif
