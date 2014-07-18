/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Synchronized.h"

/**
 * Synchronized class deals with critical regions.
 * Declare a Synchronized object at the beginning of a block. That will take the semaphore.
 * When the code exits from the block it will call the destructor which will give the semaphore.
 * This ensures that no matter how the block is exited, the semaphore will always be released.
 * Use the CRITICAL_REGION(SEM_ID) and END_REGION macros to make the code look cleaner (see header file)
 * @param semaphore The semaphore controlling this critical region.
 */
Synchronized::Synchronized(SEM_ID semaphore)
{
	m_semaphore = semaphore;
	semTake(m_semaphore, WAIT_FOREVER);
}

Synchronized::Synchronized(ReentrantSemaphore& semaphore)
{
	m_semaphore = semaphore.m_semaphore;
	semTake(m_semaphore, WAIT_FOREVER);
}

/**
 * This destructor unlocks the semaphore.
 */
Synchronized::~Synchronized()
{
	semGive(m_semaphore);
}
