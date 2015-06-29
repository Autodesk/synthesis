/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef INTERRUPTABLE_SENSORBASE_H_
#define INTERRUPTABLE_SENSORBASE_H_

#include "ChipObject.h"
#include "SensorBase.h"

class InterruptableSensorBase : public SensorBase
{
public:
	InterruptableSensorBase();
	virtual ~InterruptableSensorBase();
	virtual void RequestInterrupts(tInterruptHandler handler, void *param) = 0; ///< Asynchronus handler version.
	virtual void RequestInterrupts() = 0;		///< Synchronus Wait version.
	virtual void CancelInterrupts();			///< Free up the underlying chipobject functions.
	virtual void WaitForInterrupt(float timeout); ///< Synchronus version.
	virtual void EnableInterrupts();			///< Enable interrupts - after finishing setup.
	virtual void DisableInterrupts();		///< Disable, but don't deallocate.
	virtual double ReadInterruptTimestamp();		///< Return the timestamp for the interrupt that occurred.
protected:
	tInterrupt *m_interrupt;
	tInterruptManager *m_manager;
	uint32_t m_interruptIndex;
	void AllocateInterrupts(bool watcher);
};

#endif

