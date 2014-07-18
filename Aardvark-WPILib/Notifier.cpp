/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Notifier.h"
#include "Timer.h"
#include "Utility.h"
#include "WPIErrors.h"

const uint32_t Notifier::kTimerInterruptNumber;
Notifier *Notifier::timerQueueHead = NULL;
ReentrantSemaphore Notifier::queueSemaphore;
tAlarm *Notifier::talarm = NULL;
tInterruptManager *Notifier::manager = NULL;
int Notifier::refcount = 0;

/**
 * Create a Notifier for timer event notification.
 * @param handler The handler is called at the notification time which is set
 * using StartSingle or StartPeriodic.
 */
Notifier::Notifier(TimerEventHandler handler, void *param)
{
	if (handler == NULL)
		wpi_setWPIErrorWithContext(NullParameter, "handler must not be NULL");
	m_handler = handler;
	m_param = param;
	m_periodic = false;
	m_expirationTime = 0;
	m_period = 0;
	m_nextEvent = NULL;
	m_queued = false;
	m_handlerSemaphore = semBCreate(SEM_Q_PRIORITY, SEM_FULL);
	tRioStatusCode localStatus = NiFpga_Status_Success;
	{
		Synchronized sync(queueSemaphore);
		// do the first time intialization of static variables
		if (refcount == 0)
		{
			manager = new tInterruptManager(1 << kTimerInterruptNumber, false, &localStatus);
			manager->registerHandler( (nFPGA::tInterruptHandler) ProcessQueue, NULL, &localStatus);
			manager->enable(&localStatus);
			talarm = tAlarm::create(&localStatus);
		}
		refcount++;
	}
	wpi_setError(localStatus);
}

/**
 * Free the resources for a timer event.
 * All resources will be freed and the timer event will be removed from the
 * queue if necessary.
 */
Notifier::~Notifier()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	{
		Synchronized sync(queueSemaphore);
		DeleteFromQueue();

		// Delete the static variables when the last one is going away
		if (!(--refcount))
		{
			talarm->writeEnable(false, &localStatus);
			delete talarm;
			talarm = NULL;
			manager->disable(&localStatus);
			delete manager;
			manager = NULL;
		}
	}
	wpi_setError(localStatus);

	// Acquire the semaphore; this makes certain that the handler is 
	// not being executed by the interrupt manager.
	semTake(m_handlerSemaphore, WAIT_FOREVER);
	// Delete while holding the semaphore so there can be no race.
	semDelete(m_handlerSemaphore);
}

/**
 * Update the alarm hardware to reflect the current first element in the queue.
 * Compute the time the next alarm should occur based on the current time and the
 * period for the first element in the timer queue.
 * WARNING: this method does not do synchronization! It must be called from somewhere
 * that is taking care of synchronizing access to the queue.
 */
void Notifier::UpdateAlarm()
{
	if (timerQueueHead != NULL)
	{
		tRioStatusCode localStatus = NiFpga_Status_Success;
		// write the first item in the queue into the trigger time
		talarm->writeTriggerTime((uint32_t)(timerQueueHead->m_expirationTime * 1e6), &localStatus);
		// Enable the alarm.  The hardware disables itself after each alarm.
		talarm->writeEnable(true, &localStatus);
		wpi_setStaticError(timerQueueHead, localStatus);
	}
}

/**
 * ProcessQueue is called whenever there is a timer interrupt.
 * We need to wake up and process the current top item in the timer queue as long
 * as its scheduled time is after the current time. Then the item is removed or 
 * rescheduled (repetitive events) in the queue.
 */
void Notifier::ProcessQueue(uint32_t mask, void *params)
{
	Notifier *current;
	while (true)				// keep processing past events until no more
	{
		{
			Synchronized sync(queueSemaphore);
			double currentTime = GetClock();
			current = timerQueueHead;
			if (current == NULL || current->m_expirationTime > currentTime)
			{
				break;		// no more timer events to process
			}
			// need to process this entry
			timerQueueHead = current->m_nextEvent;
			if (current->m_periodic)
			{
				// if periodic, requeue the event
				// compute when to put into queue
				current->InsertInQueue(true);
			}
			else
			{
				// not periodic; removed from queue
				current->m_queued = false;
			}
			// Take handler semaphore while holding queue semaphore to make sure
			//  the handler will execute to completion in case we are being deleted.
			semTake(current->m_handlerSemaphore, WAIT_FOREVER);
		}

		current->m_handler(current->m_param);	// call the event handler
		semGive(current->m_handlerSemaphore);
	}
	// reschedule the first item in the queue
	Synchronized sync(queueSemaphore);
	UpdateAlarm();
}

/**
 * Insert this Notifier into the timer queue in right place.
 * WARNING: this method does not do synchronization! It must be called from somewhere
 * that is taking care of synchronizing access to the queue.
 * @param reschedule If false, the scheduled alarm is based on the curent time and UpdateAlarm
 * method is called which will enable the alarm if necessary.
 * If true, update the time by adding the period (no drift) when rescheduled periodic from ProcessQueue.
 * This ensures that the public methods only update the queue after finishing inserting.
 */
void Notifier::InsertInQueue(bool reschedule)
{
	if (reschedule)
	{
		m_expirationTime += m_period;
	}
	else
	{
		m_expirationTime = GetClock() + m_period;
	}
	if (timerQueueHead == NULL || timerQueueHead->m_expirationTime >= this->m_expirationTime)
	{
		// the queue is empty or greater than the new entry
		// the new entry becomes the first element
		this->m_nextEvent = timerQueueHead;
		timerQueueHead = this;
		if (!reschedule)
		{
			// since the first element changed, update alarm, unless we already plan to
			UpdateAlarm();
		}
	}
	else
	{
		for (Notifier **npp = &(timerQueueHead->m_nextEvent); ; npp = &(*npp)->m_nextEvent)
		{
			Notifier *n = *npp;
			if (n == NULL || n->m_expirationTime > this->m_expirationTime)
			{
				*npp = this;
				this->m_nextEvent = n;
				break;
			}
		}
	}
	m_queued = true;
}

/**
 * Delete this Notifier from the timer queue.
 * WARNING: this method does not do synchronization! It must be called from somewhere
 * that is taking care of synchronizing access to the queue.
 * Remove this Notifier from the timer queue and adjust the next interrupt time to reflect
 * the current top of the queue.
 */
void Notifier::DeleteFromQueue()
{
	if (m_queued)
	{
		m_queued = false;
		wpi_assert(timerQueueHead != NULL);
		if (timerQueueHead == this)
		{
			// remove the first item in the list - update the alarm
			timerQueueHead = this->m_nextEvent;
			UpdateAlarm();
		}
		else
		{
			for (Notifier *n = timerQueueHead; n != NULL; n = n->m_nextEvent)
			{
				if (n->m_nextEvent == this)
				{
					// this element is the next element from *n from the queue
					n->m_nextEvent = this->m_nextEvent;	// point around this one
				}
			}
		}
	}
}

/**
 * Register for single event notification.
 * A timer event is queued for a single event after the specified delay.
 * @param delay Seconds to wait before the handler is called.
 */
void Notifier::StartSingle(double delay)
{
	Synchronized sync(queueSemaphore);
	m_periodic = false;
	m_period = delay;
	DeleteFromQueue();
	InsertInQueue(false);
}

/**
 * Register for periodic event notification.
 * A timer event is queued for periodic event notification. Each time the interrupt
 * occurs, the event will be immediately requeued for the same time interval.
 * @param period Period in seconds to call the handler starting one period after the call to this method.
 */
void Notifier::StartPeriodic(double period)
{
	Synchronized sync(queueSemaphore);
	m_periodic = true;
	m_period = period;
	DeleteFromQueue();
	InsertInQueue(false);
}

/**
 * Stop timer events from occuring.
 * Stop any repeating timer events from occuring. This will also remove any single
 * notification events from the queue.
 * If a timer-based call to the registered handler is in progress, this function will
 * block until the handler call is complete.
 */
void Notifier::Stop()
{
	{
		Synchronized sync(queueSemaphore);
		DeleteFromQueue();
	}
	// Wait for a currently executing handler to complete before returning from Stop()
	Synchronized sync(m_handlerSemaphore);
}
