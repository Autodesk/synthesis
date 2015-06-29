/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Commands/WaitForChildren.h"
#include "Commands/CommandGroup.h"

WaitForChildren::WaitForChildren(double timeout) :
	Command("WaitForChildren", timeout)
{	
}

WaitForChildren::WaitForChildren(const char *name, double timeout) :
	Command(name, timeout)
{
}

void WaitForChildren::Initialize()
{
}

void WaitForChildren::Execute()
{
}

void WaitForChildren::End()
{
}

void WaitForChildren::Interrupted()
{
}

bool WaitForChildren::IsFinished()
{
    return GetGroup() == NULL || GetGroup()->GetSize() == 0;
}
