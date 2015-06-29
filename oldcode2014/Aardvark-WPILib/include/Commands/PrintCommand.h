/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __PRINT_COMMAND_H__
#define __PRINT_COMMAND_H__

#include "Commands/Command.h"
#include <string>

class PrintCommand : public Command
{
public:
	PrintCommand(const char *message);
	virtual ~PrintCommand() {}

protected:
	virtual void Initialize();
	virtual void Execute();
	virtual bool IsFinished();
	virtual void End();
	virtual void Interrupted();

private:
	std::string m_message;
};

#endif
