/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Ultrasonic.h"

#include "Counter.h"
#include "DigitalInput.h"
#include "DigitalOutput.h"
#include "NetworkCommunication/UsageReporting.h"
#include "Timer.h"
#include "Utility.h"
#include "WPIErrors.h"
#include "LiveWindow/LiveWindow.h"

constexpr double Ultrasonic::kPingTime;	///< Time (sec) for the ping trigger pulse.
const uint32_t Ultrasonic::kPriority;	///< Priority that the ultrasonic round robin task runs.
constexpr double Ultrasonic::kMaxUltrasonicTime;	///< Max time (ms) between readings.
constexpr double Ultrasonic::kSpeedOfSoundInchesPerSec;
Task Ultrasonic::m_task("UltrasonicChecker", (FUNCPTR)UltrasonicChecker); // task doing the round-robin automatic sensing
Ultrasonic *Ultrasonic::m_firstSensor = NULL; // head of the ultrasonic sensor list
bool Ultrasonic::m_automaticEnabled = false; // automatic round robin mode
SEM_ID Ultrasonic::m_semaphore = 0;

/**
 * Background task that goes through the list of ultrasonic sensors and pings each one in turn. The counter
 * is configured to read the timing of the returned echo pulse.
 * 
 * DANGER WILL ROBINSON, DANGER WILL ROBINSON:
 * This code runs as a task and assumes that none of the ultrasonic sensors will change while it's
 * running. If one does, then this will certainly break. Make sure to disable automatic mode before changing
 * anything with the sensors!!
 */
void Ultrasonic::UltrasonicChecker()
{
	Ultrasonic *u = NULL;
	while (m_automaticEnabled)
	{
		if (u == NULL) u = m_firstSensor;
		if (u == NULL) return;
		if (u->IsEnabled())
			u->m_pingChannel->Pulse(kPingTime);	// do the ping
		u = u->m_nextSensor;
		Wait(0.1);							// wait for ping to return
	}
}

/**
 * Initialize the Ultrasonic Sensor.
 * This is the common code that initializes the ultrasonic sensor given that there
 * are two digital I/O channels allocated. If the system was running in automatic mode (round robin)
 * when the new sensor is added, it is stopped, the sensor is added, then automatic mode is
 * restored.
 */
void Ultrasonic::Initialize()
{
	m_table = NULL;
	bool originalMode = m_automaticEnabled;
	if (m_semaphore == 0) m_semaphore = semBCreate(SEM_Q_PRIORITY, SEM_FULL);
	SetAutomaticMode(false); // kill task when adding a new sensor
	semTake(m_semaphore, WAIT_FOREVER); // link this instance on the list
	{
		m_nextSensor = m_firstSensor;
		m_firstSensor = this;
	}
	semGive(m_semaphore);

	m_counter = new Counter(m_echoChannel); // set up counter for this sensor
	m_counter->SetMaxPeriod(1.0);
	m_counter->SetSemiPeriodMode(true);
	m_counter->Reset();
	m_counter->Start();
	m_enabled = true; // make it available for round robin scheduling
	SetAutomaticMode(originalMode);

	static int instances = 0;
	instances++;
	nUsageReporting::report(nUsageReporting::kResourceType_Ultrasonic, instances);
	LiveWindow::GetInstance()->AddSensor("Ultrasonic", m_echoChannel->GetModuleForRouting(), m_echoChannel->GetChannel(), this);
}

/**
 * Create an instance of the Ultrasonic Sensor using the default module.
 * This is designed to supchannel the Daventech SRF04 and Vex ultrasonic sensors. This
 * constructor assumes that both digital I/O channels are in the default digital module.
 * @param pingChannel The digital output channel that sends the pulse to initiate the sensor sending
 * the ping.
 * @param echoChannel The digital input channel that receives the echo. The length of time that the
 * echo is high represents the round trip time of the ping, and the distance.
 * @param units The units returned in either kInches or kMilliMeters
 */
Ultrasonic::Ultrasonic(uint32_t pingChannel, uint32_t echoChannel, DistanceUnit units)
{
	m_pingChannel = new DigitalOutput(pingChannel);
	m_echoChannel = new DigitalInput(echoChannel);
	m_allocatedChannels = true;
	m_units = units;
	Initialize();
}

/**
 * Create an instance of an Ultrasonic Sensor from a DigitalInput for the echo channel and a DigitalOutput
 * for the ping channel.
 * @param pingChannel The digital output object that starts the sensor doing a ping. Requires a 10uS pulse to start.
 * @param echoChannel The digital input object that times the return pulse to determine the range.
 * @param units The units returned in either kInches or kMilliMeters
 */
Ultrasonic::Ultrasonic(DigitalOutput *pingChannel, DigitalInput *echoChannel, DistanceUnit units)
{
	if (pingChannel == NULL || echoChannel == NULL)
	{
		wpi_setWPIError(NullParameter);
		return;
	}
	m_allocatedChannels = false;
	m_pingChannel = pingChannel;
	m_echoChannel = echoChannel;
	m_units = units;
	Initialize();
}

/**
 * Create an instance of an Ultrasonic Sensor from a DigitalInput for the echo channel and a DigitalOutput
 * for the ping channel.
 * @param pingChannel The digital output object that starts the sensor doing a ping. Requires a 10uS pulse to start.
 * @param echoChannel The digital input object that times the return pulse to determine the range.
 * @param units The units returned in either kInches or kMilliMeters
 */
Ultrasonic::Ultrasonic(DigitalOutput &pingChannel, DigitalInput &echoChannel, DistanceUnit units)
{
	m_allocatedChannels = false;
	m_pingChannel = &pingChannel;
	m_echoChannel = &echoChannel;
	m_units = units;
	Initialize();
}

/**
 * Create an instance of the Ultrasonic sensor using specified modules.
 * This is designed to supchannel the Daventech SRF04 and Vex ultrasonic sensors. This
 * constructors takes the channel and module slot for each of the required digital I/O channels.
 * @param pingModuleNumber The digital module that the pingChannel is on.
 * @param pingChannel The digital output channel that sends the pulse to initiate the sensor
 * sending the ping.
 * @param echoModuleNumber The digital module that the echoChannel is on.
 * @param echoChannel The digital input channel that receives the echo. The length of time
 * that the echo is high represents the round trip time of the ping, and the distance.
 * @param units The units returned in either kInches or kMilliMeters
 */
Ultrasonic::Ultrasonic(uint8_t pingModuleNumber, uint32_t pingChannel,
		uint8_t echoModuleNumber, uint32_t echoChannel, DistanceUnit units)
{
	m_pingChannel = new DigitalOutput(pingModuleNumber, pingChannel);
	m_echoChannel = new DigitalInput(echoModuleNumber, echoChannel);
	m_allocatedChannels = true;
	m_units = units;
	Initialize();
}

/**
 * Destructor for the ultrasonic sensor.
 * Delete the instance of the ultrasonic sensor by freeing the allocated digital channels.
 * If the system was in automatic mode (round robin), then it is stopped, then started again
 * after this sensor is removed (provided this wasn't the last sensor).
 */
Ultrasonic::~Ultrasonic()
{
	bool wasAutomaticMode = m_automaticEnabled;
	SetAutomaticMode(false);
	if (m_allocatedChannels)
	{
		delete m_pingChannel;
		delete m_echoChannel;
	}
	wpi_assert(m_firstSensor != NULL);

	semTake(m_semaphore, WAIT_FOREVER);
	{
		if (this == m_firstSensor)
		{
			m_firstSensor = m_nextSensor;
			if (m_firstSensor == NULL)
			{
				SetAutomaticMode(false);
			}
		}
		else
		{
			wpi_assert(m_firstSensor->m_nextSensor != NULL);
			for (Ultrasonic *s = m_firstSensor; s != NULL; s = s->m_nextSensor)
			{
				if (this == s->m_nextSensor)
				{
					s->m_nextSensor = s->m_nextSensor->m_nextSensor;
					break;
				}
			}
		}
	}
	semGive(m_semaphore);
	if (m_firstSensor != NULL && wasAutomaticMode)
		SetAutomaticMode(true);
}

/**
 * Turn Automatic mode on/off.
 * When in Automatic mode, all sensors will fire in round robin, waiting a set
 * time between each sensor.
 * @param enabling Set to true if round robin scheduling should start for all the ultrasonic sensors. This
 * scheduling method assures that the sensors are non-interfering because no two sensors fire at the same time.
 * If another scheduling algorithm is preffered, it can be implemented by pinging the sensors manually and waiting
 * for the results to come back.
 */
void Ultrasonic::SetAutomaticMode(bool enabling)
{
	if (enabling == m_automaticEnabled)
		return; // ignore the case of no change

	m_automaticEnabled = enabling;
	if (enabling)
	{
		// enabling automatic mode.
		// Clear all the counters so no data is valid
		for (Ultrasonic *u = m_firstSensor; u != NULL; u = u->m_nextSensor)
		{
			u->m_counter->Reset();
		}
		// Start round robin task
		wpi_assert(m_task.Verify() == false);	// should be false since was previously disabled
		m_task.Start();
	}
	else
	{
		// disabling automatic mode. Wait for background task to stop running.
		while (m_task.Verify())
			Wait(0.15);	// just a little longer than the ping time for round-robin to stop

		// clear all the counters (data now invalid) since automatic mode is stopped
		for (Ultrasonic *u = m_firstSensor; u != NULL; u = u->m_nextSensor)
		{
			u->m_counter->Reset();
		}
		m_task.Stop();
	}
}

/**
 * Single ping to ultrasonic sensor.
 * Send out a single ping to the ultrasonic sensor. This only works if automatic (round robin)
 * mode is disabled. A single ping is sent out, and the counter should count the semi-period
 * when it comes in. The counter is reset to make the current value invalid.
 */
void Ultrasonic::Ping()
{
	// TODO: Either assert or disable, not both.
	wpi_assert(!m_automaticEnabled);
	SetAutomaticMode(false); // turn off automatic round robin if pinging single sensor
	m_counter->Reset(); // reset the counter to zero (invalid data now)
	m_pingChannel->Pulse(kPingTime); // do the ping to start getting a single range
}

/**
 * Check if there is a valid range measurement.
 * The ranges are accumulated in a counter that will increment on each edge of the echo (return)
 * signal. If the count is not at least 2, then the range has not yet been measured, and is invalid.
 */
bool Ultrasonic::IsRangeValid()
{
	return m_counter->Get() > 1;
}

/**
 * Get the range in inches from the ultrasonic sensor.
 * @return double Range in inches of the target returned from the ultrasonic sensor. If there is
 * no valid value yet, i.e. at least one measurement hasn't completed, then return 0.
 */
double Ultrasonic::GetRangeInches()
{
	if (IsRangeValid())
		return m_counter->GetPeriod() * kSpeedOfSoundInchesPerSec / 2.0;
	else
		return 0;
}

/**
 * Get the range in millimeters from the ultrasonic sensor.
 * @return double Range in millimeters of the target returned by the ultrasonic sensor.
 * If there is no valid value yet, i.e. at least one measurement hasn't complted, then return 0.
 */
double Ultrasonic::GetRangeMM()
{
	return GetRangeInches() * 25.4;
}

/**
 * Get the range in the current DistanceUnit for the PIDSource base object.
 * 
 * @return The range in DistanceUnit
 */
double Ultrasonic::PIDGet()
{
	switch(m_units) 
	{
	case Ultrasonic::kInches:
		return GetRangeInches();
	case Ultrasonic::kMilliMeters:
		return GetRangeMM();
	default:
		return 0.0;
	}
}

/**
 * Set the current DistanceUnit that should be used for the PIDSource base object.
 * 
 * @param units The DistanceUnit that should be used.
 */
void Ultrasonic::SetDistanceUnits(DistanceUnit units)
{
	m_units = units;
}

/**
 * Get the current DistanceUnit that is used for the PIDSource base object.
 * 
 * @return The type of DistanceUnit that is being used.
 */
Ultrasonic::DistanceUnit Ultrasonic::GetDistanceUnits()
{
	return m_units;
}

void Ultrasonic::UpdateTable() {
	if (m_table != NULL) {
		m_table->PutNumber("Value", GetRangeInches());
	}
}

void Ultrasonic::StartLiveWindowMode() {
	
}

void Ultrasonic::StopLiveWindowMode() {
	
}

std::string Ultrasonic::GetSmartDashboardType() {
	return "Ultrasonic";
}

void Ultrasonic::InitTable(ITable *subTable) {
	m_table = subTable;
	UpdateTable();
}

ITable * Ultrasonic::GetTable() {
	return m_table;
}

