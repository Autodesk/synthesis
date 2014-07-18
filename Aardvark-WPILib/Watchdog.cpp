/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Watchdog.h"

constexpr double Watchdog::kDefaultWatchdogExpiration;

/**
 * The Watchdog is born.
 */
Watchdog::Watchdog()
	:	m_fpgaWatchDog(NULL)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaWatchDog = tWatchdog::create(&localStatus);
	wpi_setError(localStatus);
	SetExpiration(kDefaultWatchdogExpiration);
	SetEnabled(true);
}

/**
 * Time to bury him in the back yard.
 */
Watchdog::~Watchdog()
{
	SetEnabled(false);
	delete m_fpgaWatchDog;
	m_fpgaWatchDog = NULL;
}

/**
 * Throw the dog a bone.
 * 
 * When everything is going well, you feed your dog when you get home.
 * Let's hope you don't drive your car off a bridge on the way home...
 * Your dog won't get fed and he will starve to death.
 * 
 * By the way, it's not cool to ask the neighbor (some random task) to
 * feed your dog for you.  He's your responsibility!
 * 
 * @returns Returns the previous state of the watchdog before feeding it.
 */
bool Watchdog::Feed()
{
	bool previous = GetEnabled();
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaWatchDog->strobeFeed(&localStatus);
	wpi_setError(localStatus);
	return previous;
}

/**
 * Put the watchdog out of its misery.
 * 
 * Don't wait for your dying robot to starve when there is a problem.
 * Kill it quickly, cleanly, and humanely.
 */
void Watchdog::Kill()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaWatchDog->strobeKill(&localStatus);
	wpi_setError(localStatus);
}

/**
 * Read how long it has been since the watchdog was last fed.
 * 
 * @return The number of seconds since last meal.
 */
double Watchdog::GetTimer()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint32_t timer = m_fpgaWatchDog->readTimer(&localStatus);
	wpi_setError(localStatus);
	return timer / (kSystemClockTicksPerMicrosecond * 1e6);
}

/**
 * Read what the current expiration is.
 * 
 * @return The number of seconds before starvation following a meal (watchdog starves if it doesn't eat this often).
 */
double Watchdog::GetExpiration()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint32_t expiration = m_fpgaWatchDog->readExpiration(&localStatus);
	wpi_setError(localStatus);
	return expiration / (kSystemClockTicksPerMicrosecond * 1e6);
}

/**
 * Configure how many seconds your watchdog can be neglected before it starves to death.
 * 
 * @param expiration The number of seconds before starvation following a meal (watchdog starves if it doesn't eat this often).
 */
void Watchdog::SetExpiration(double expiration)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaWatchDog->writeExpiration((uint32_t)(expiration * (kSystemClockTicksPerMicrosecond * 1e6)), &localStatus);
	wpi_setError(localStatus);
}

/**
 * Find out if the watchdog is currently enabled or disabled (mortal or immortal).
 * 
 * @return Enabled or disabled.
 */
bool Watchdog::GetEnabled()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool enabled = !m_fpgaWatchDog->readImmortal(&localStatus);
	wpi_setError(localStatus);
	return enabled;
}

/**
 * Enable or disable the watchdog timer.
 * 
 * When enabled, you must keep feeding the watchdog timer to
 * keep the watchdog active, and hence the dangerous parts 
 * (motor outputs, etc.) can keep functioning.
 * When disabled, the watchdog is immortal and will remain active
 * even without being fed.  It will also ignore any kill commands
 * while disabled.
 * 
 * @param enabled Enable or disable the watchdog.
 */
void Watchdog::SetEnabled(bool enabled)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaWatchDog->writeImmortal(!enabled, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Check in on the watchdog and make sure he's still kicking.
 * 
 * This indicates that your watchdog is allowing the system to operate.
 * It is still possible that the network communications is not allowing the
 * system to run, but you can check this to make sure it's not your fault.
 * Check IsSystemActive() for overall system status.
 * 
 * If the watchdog is disabled, then your watchdog is immortal.
 * 
 * @return Is the watchdog still alive?
 */
bool Watchdog::IsAlive()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool alive = m_fpgaWatchDog->readStatus_Alive(&localStatus);
	wpi_setError(localStatus);
	return alive;
}

/**
 * Check on the overall status of the system.
 * 
 * @return Is the system active (i.e. PWM motor outputs, etc. enabled)?
 */
bool Watchdog::IsSystemActive()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool alive = m_fpgaWatchDog->readStatus_SystemActive(&localStatus);
	wpi_setError(localStatus);
	return alive;
}

