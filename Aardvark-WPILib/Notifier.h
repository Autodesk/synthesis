/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef NOTIFIER_H
#define NOTIFIER_H

#include "ChipObject.h"
#include "ErrorBase.h"
#include "Synchronized.h"

typedef void (*TimerEventHandler)(void *param);

class Notifier : public ErrorBase
{
public:
	Notifier(TimerEventHandler handler, void *param = NULL);
	virtual ~Notifier();
	void StartSingle(double delay);
	void StartPeriodic(double period);
	void Stop();
private:
	static Notifier *timerQueueHead;
	static ReentrantSemaphore queueSemaphore;
	static tAlarm *talarm;
	static tInterruptManager *manager;
	static int refcount;

	static const uint32_t kTimerInterruptNumber = 28;
	static void ProcessQueue(uint32_t mask, void *params); // process the timer queue on a timer event
	static void UpdateAlarm();			// update the FPGA alarm since the queue has changed
	void InsertInQueue(bool reschedule);	// insert this Notifier in the timer queue
	void DeleteFromQueue();				// delete this Notifier from the timer queue
	TimerEventHandler m_handler;			// address of the handler
	void *m_param;							// a parameter to pass to the handler
	double m_period;						// the relative time (either periodic or single)
	double m_expirationTime;				// absolute expiration time for the current event
	Notifier *m_nextEvent;					// next Nofifier event
	bool m_periodic;						// true if this is a periodic event
	bool m_queued;							// indicates if this entry is queued
	SEM_ID m_handlerSemaphore;				// held by interrupt manager task while handler call is in progress 
	DISALLOW_COPY_AND_ASSIGN(Notifier);
};

#endif
