/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Counter.h"
#include "AnalogTrigger.h"
#include "DigitalInput.h"
#include "NetworkCommunication/UsageReporting.h"
#include "Resource.h"
#include "WPIErrors.h"

static Resource *counters = NULL;

/**
 * Create an instance of a counter object.
 * This creates a ChipObject counter and initializes status variables appropriately
 */
void Counter::InitCounter(Mode mode)
{
	m_table = NULL;
	Resource::CreateResourceObject(&counters, tCounter::kNumSystems);
	uint32_t index = counters->Allocate("Counter");
	if (index == ~0ul)
	{
		CloneError(counters);
		return;
	}
	m_index = index;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter = tCounter::create(m_index, &localStatus);
	m_counter->writeConfig_Mode(mode, &localStatus);
	m_upSource = NULL;
	m_downSource = NULL;
	m_allocatedUpSource = false;
	m_allocatedDownSource = false;
	m_counter->writeTimerConfig_AverageSize(1, &localStatus);
	wpi_setError(localStatus);

	nUsageReporting::report(nUsageReporting::kResourceType_Counter, index, mode);
}

/**
 * Create an instance of a counter where no sources are selected.
 * Then they all must be selected by calling functions to specify the upsource and the downsource
 * independently.
 */
Counter::Counter() :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
}

/**
 * Create an instance of a counter from a Digital Input.
 * This is used if an existing digital input is to be shared by multiple other objects such
 * as encoders.
 */
Counter::Counter(DigitalSource *source) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
	SetUpSource(source);
	ClearDownSource();
}

Counter::Counter(DigitalSource &source) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
	SetUpSource(&source);
	ClearDownSource();
}

/**
 * Create an instance of a Counter object.
 * Create an up-Counter instance given a channel. The default digital module is assumed.
 */
Counter::Counter(uint32_t channel) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
	SetUpSource(channel);
	ClearDownSource();
}

/**
 * Create an instance of a Counter object.
 * Create an instance of an up-Counter given a digital module and a channel.
 * @param moduleNumber The digital module (1 or 2).
 * @param channel The channel in the digital module
 */
Counter::Counter(uint8_t moduleNumber, uint32_t channel) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
	SetUpSource(moduleNumber, channel);
	ClearDownSource();
}

/**
 * Create an instance of a Counter object.
 * Create an instance of a simple up-Counter given an analog trigger.
 * Use the trigger state output from the analog trigger.
 */
Counter::Counter(AnalogTrigger *trigger) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
	SetUpSource(trigger->CreateOutput(AnalogTriggerOutput::kState));
	ClearDownSource();
	m_allocatedUpSource = true;
}

Counter::Counter(AnalogTrigger &trigger) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
	SetUpSource(trigger.CreateOutput(AnalogTriggerOutput::kState));
	ClearDownSource();
	m_allocatedUpSource = true;
}

Counter::Counter(EncodingType encodingType, DigitalSource *upSource, DigitalSource *downSource, bool inverted) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	if (encodingType != k1X && encodingType != k2X)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "Counter only supports 1X and 2X quadrature decoding.");
		return;
	}
	InitCounter(kExternalDirection);
	SetUpSource(upSource);
	SetDownSource(downSource);
	tRioStatusCode localStatus = NiFpga_Status_Success;

	if (encodingType == k1X)
	{
		SetUpSourceEdge(true, false);
		m_counter->writeTimerConfig_AverageSize(1, &localStatus);
	}
	else
	{
		SetUpSourceEdge(true, true);
		m_counter->writeTimerConfig_AverageSize(2, &localStatus);
	}

	wpi_setError(localStatus);
	SetDownSourceEdge(inverted, true);
}

/**
 * Delete the Counter object.
 */
Counter::~Counter()
{
	SetUpdateWhenEmpty(true);
	if (m_allocatedUpSource)
	{
		delete m_upSource;
		m_upSource = NULL;
	}
	if (m_allocatedDownSource)
	{
		delete m_downSource;
		m_downSource = NULL;
	}
	delete m_counter;
	m_counter = NULL;
	counters->Free(m_index);
}

/**
 * Set the up source for the counter as digital input channel and slot.
 *
 * @param moduleNumber The digital module (1 or 2).
 * @param channel The digital channel (1..14).
 */
void Counter::SetUpSource(uint8_t moduleNumber, uint32_t channel)
{
	if (StatusIsFatal()) return;
	SetUpSource(new DigitalInput(moduleNumber, channel));
	m_allocatedUpSource = true;
}

/**
 * Set the upsource for the counter as a digital input channel.
 * The slot will be the default digital module slot.
 */
void Counter::SetUpSource(uint32_t channel)
{
	if (StatusIsFatal()) return;
	SetUpSource(GetDefaultDigitalModule(), channel);
	m_allocatedUpSource = true;
}

/**
 * Set the up counting source to be an analog trigger.
 * @param analogTrigger The analog trigger object that is used for the Up Source
 * @param triggerType The analog trigger output that will trigger the counter.
 */
void Counter::SetUpSource(AnalogTrigger *analogTrigger, AnalogTriggerOutput::Type triggerType)
{
	if (StatusIsFatal()) return;
	SetUpSource(analogTrigger->CreateOutput(triggerType));
	m_allocatedUpSource = true;
}

/**
 * Set the up counting source to be an analog trigger.
 * @param analogTrigger The analog trigger object that is used for the Up Source
 * @param triggerType The analog trigger output that will trigger the counter.
 */
void Counter::SetUpSource(AnalogTrigger &analogTrigger, AnalogTriggerOutput::Type triggerType)
{
	SetUpSource(&analogTrigger, triggerType);
}

/**
 * Set the source object that causes the counter to count up.
 * Set the up counting DigitalSource.
 */
void Counter::SetUpSource(DigitalSource *source)
{
	if (StatusIsFatal()) return;
	if (m_allocatedUpSource)
	{
		delete m_upSource;
		m_upSource = NULL;
		m_allocatedUpSource = false;
	}
	m_upSource = source;
	if (m_upSource->StatusIsFatal())
	{
		CloneError(m_upSource);
	}
	else
	{
		tRioStatusCode localStatus = NiFpga_Status_Success;
		m_counter->writeConfig_UpSource_Module(source->GetModuleForRouting(), &localStatus);
		m_counter->writeConfig_UpSource_Channel(source->GetChannelForRouting(), &localStatus);
		m_counter->writeConfig_UpSource_AnalogTrigger(source->GetAnalogTriggerForRouting(), &localStatus);
	
		if(m_counter->readConfig_Mode(&localStatus) == kTwoPulse ||
				m_counter->readConfig_Mode(&localStatus) == kExternalDirection)
		{
			SetUpSourceEdge(true, false);
		}
		m_counter->strobeReset(&localStatus);
		wpi_setError(localStatus);
	}
}

/**
 * Set the source object that causes the counter to count up.
 * Set the up counting DigitalSource.
 */
void Counter::SetUpSource(DigitalSource &source)
{
	SetUpSource(&source);
}

/**
 * Set the edge sensitivity on an up counting source.
 * Set the up source to either detect rising edges or falling edges.
 */
void Counter::SetUpSourceEdge(bool risingEdge, bool fallingEdge)
{
	if (StatusIsFatal()) return;
	if (m_upSource == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "Must set non-NULL UpSource before setting UpSourceEdge");
	}
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeConfig_UpRisingEdge(risingEdge, &localStatus);
	m_counter->writeConfig_UpFallingEdge(fallingEdge, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Disable the up counting source to the counter.
 */
void Counter::ClearUpSource()
{
	if (StatusIsFatal()) return;
	if (m_allocatedUpSource)
	{
		delete m_upSource;
		m_upSource = NULL;
		m_allocatedUpSource = false;
	}
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool state = m_counter->readConfig_Enable(&localStatus);
	m_counter->writeConfig_Enable(false, &localStatus);
	m_counter->writeConfig_UpFallingEdge(false, &localStatus);
	m_counter->writeConfig_UpRisingEdge(false, &localStatus);
	// Index 0 of digital is always 0.
	m_counter->writeConfig_UpSource_Channel(0, &localStatus);
	m_counter->writeConfig_UpSource_AnalogTrigger(false, &localStatus);
	m_counter->writeConfig_Enable(state, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Set the down counting source to be a digital input channel.
 * The slot will be set to the default digital module slot.
 */
void Counter::SetDownSource(uint32_t channel)
{
	if (StatusIsFatal()) return;
	SetDownSource(new DigitalInput(channel));
	m_allocatedDownSource = true;
}

/**
 * Set the down counting source to be a digital input slot and channel.
 *
 * @param moduleNumber The digital module (1 or 2).
 * @param channel The digital channel (1..14).
 */
void Counter::SetDownSource(uint8_t moduleNumber, uint32_t channel)
{
	if (StatusIsFatal()) return;
	SetDownSource(new DigitalInput(moduleNumber, channel));
	m_allocatedDownSource = true;
}

/**
 * Set the down counting source to be an analog trigger.
 * @param analogTrigger The analog trigger object that is used for the Down Source
 * @param triggerType The analog trigger output that will trigger the counter.
 */
void Counter::SetDownSource(AnalogTrigger *analogTrigger, AnalogTriggerOutput::Type triggerType)
{
	if (StatusIsFatal()) return;
	SetDownSource(analogTrigger->CreateOutput(triggerType));
	m_allocatedDownSource = true;
}

/**
 * Set the down counting source to be an analog trigger.
 * @param analogTrigger The analog trigger object that is used for the Down Source
 * @param triggerType The analog trigger output that will trigger the counter.
 */
void Counter::SetDownSource(AnalogTrigger &analogTrigger, AnalogTriggerOutput::Type triggerType)
{
	SetDownSource(&analogTrigger, triggerType);
}

/**
 * Set the source object that causes the counter to count down.
 * Set the down counting DigitalSource.
 */
void Counter::SetDownSource(DigitalSource *source)
{
	if (StatusIsFatal()) return;
	if (m_allocatedDownSource)
	{
		delete m_downSource;
		m_downSource = NULL;
		m_allocatedDownSource = false;
	}
	m_downSource = source;
	if (m_downSource->StatusIsFatal())
	{
		CloneError(m_downSource);
	}
	else
	{
		tRioStatusCode localStatus = NiFpga_Status_Success;
		unsigned char mode = m_counter->readConfig_Mode(&localStatus);
		if (mode != kTwoPulse && mode != kExternalDirection)
		{
			wpi_setWPIErrorWithContext(ParameterOutOfRange, "Counter only supports DownSource in TwoPulse and ExternalDirection modes.");
			return;
		}
		m_counter->writeConfig_DownSource_Module(source->GetModuleForRouting(), &localStatus);
		m_counter->writeConfig_DownSource_Channel(source->GetChannelForRouting(), &localStatus);
		m_counter->writeConfig_DownSource_AnalogTrigger(source->GetAnalogTriggerForRouting(), &localStatus);
	
		SetDownSourceEdge(true, false);
		m_counter->strobeReset(&localStatus);
		wpi_setError(localStatus);
	}
}

/**
 * Set the source object that causes the counter to count down.
 * Set the down counting DigitalSource.
 */
void Counter::SetDownSource(DigitalSource &source)
{
	SetDownSource(&source);
}

/**
 * Set the edge sensitivity on a down counting source.
 * Set the down source to either detect rising edges or falling edges.
 */
void Counter::SetDownSourceEdge(bool risingEdge, bool fallingEdge)
{
	if (StatusIsFatal()) return;
	if (m_downSource == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "Must set non-NULL DownSource before setting DownSourceEdge");
	}
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeConfig_DownRisingEdge(risingEdge, &localStatus);
	m_counter->writeConfig_DownFallingEdge(fallingEdge, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Disable the down counting source to the counter.
 */
void Counter::ClearDownSource()
{
	if (StatusIsFatal()) return;
	if (m_allocatedDownSource)
	{
		delete m_downSource;
		m_downSource = NULL;
		m_allocatedDownSource = false;
	}
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool state = m_counter->readConfig_Enable(&localStatus);
	m_counter->writeConfig_Enable(false, &localStatus);
	m_counter->writeConfig_DownFallingEdge(false, &localStatus);
	m_counter->writeConfig_DownRisingEdge(false, &localStatus);
	// Index 0 of digital is always 0.
	m_counter->writeConfig_DownSource_Channel(0, &localStatus);
	m_counter->writeConfig_DownSource_AnalogTrigger(false, &localStatus);
	m_counter->writeConfig_Enable(state, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Set standard up / down counting mode on this counter.
 * Up and down counts are sourced independently from two inputs.
 */
void Counter::SetUpDownCounterMode()
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeConfig_Mode(kTwoPulse, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Set external direction mode on this counter.
 * Counts are sourced on the Up counter input.
 * The Down counter input represents the direction to count.
 */
void Counter::SetExternalDirectionMode()
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeConfig_Mode(kExternalDirection, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Set Semi-period mode on this counter.
 * Counts up on both rising and falling edges. 
 */
void Counter::SetSemiPeriodMode(bool highSemiPeriod)
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeConfig_Mode(kSemiperiod, &localStatus);
	m_counter->writeConfig_UpRisingEdge(highSemiPeriod, &localStatus);
	SetUpdateWhenEmpty(false);
	wpi_setError(localStatus);
}

/**
 * Configure the counter to count in up or down based on the length of the input pulse.
 * This mode is most useful for direction sensitive gear tooth sensors.
 * @param threshold The pulse length beyond which the counter counts the opposite direction.  Units are seconds.
 */
void Counter::SetPulseLengthMode(float threshold)
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeConfig_Mode(kPulseLength, &localStatus);
	m_counter->writeConfig_PulseLengthThreshold((uint32_t)(threshold * 1.0e6) * kSystemClockTicksPerMicrosecond, &localStatus);
	wpi_setError(localStatus);
}

    
    /**
     * Get the Samples to Average which specifies the number of samples of the timer to 
     * average when calculating the period. Perform averaging to account for 
     * mechanical imperfections or as oversampling to increase resolution.
     * @return SamplesToAverage The number of samples being averaged (from 1 to 127)
     */
    int Counter::GetSamplesToAverage()
    {
    	tRioStatusCode localStatus = NiFpga_Status_Success;
        return m_counter->readTimerConfig_AverageSize(&localStatus);
    	wpi_setError(localStatus);
    }

/**
 * Set the Samples to Average which specifies the number of samples of the timer to 
 * average when calculating the period. Perform averaging to account for 
 * mechanical imperfections or as oversampling to increase resolution.
 * @param samplesToAverage The number of samples to average from 1 to 127.
 */
    void Counter::SetSamplesToAverage (int samplesToAverage) {
    	tRioStatusCode localStatus = NiFpga_Status_Success;
    	if (samplesToAverage < 1 || samplesToAverage > 127)
    	{
    		wpi_setWPIErrorWithContext(ParameterOutOfRange, "Average counter values must be between 1 and 127");
    	}
    	m_counter->writeTimerConfig_AverageSize(samplesToAverage, &localStatus);
    	wpi_setError(localStatus);
    }

/**
 * Start the Counter counting.
 * This enables the counter and it starts accumulating counts from the associated
 * input channel. The counter value is not reset on starting, and still has the previous value.
 */
void Counter::Start()
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeConfig_Enable(1, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Read the current counter value.
 * Read the value at this instant. It may still be running, so it reflects the current value. Next
 * time it is read, it might have a different value.
 */
int32_t Counter::Get()
{
	if (StatusIsFatal()) return 0;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	int32_t value = m_counter->readOutput_Value(&localStatus);
	wpi_setError(localStatus);
	return value;
}

/**
 * Reset the Counter to zero.
 * Set the counter value to zero. This doesn't effect the running state of the counter, just sets
 * the current value to zero.
 */
void Counter::Reset()
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->strobeReset(&localStatus);
	wpi_setError(localStatus);
}

/**
 * Stop the Counter.
 * Stops the counting but doesn't effect the current value.
 */
void Counter::Stop()
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeConfig_Enable(0, &localStatus);
	wpi_setError(localStatus);
}

/*
 * Get the Period of the most recent count.
 * Returns the time interval of the most recent count. This can be used for velocity calculations
 * to determine shaft speed.
 * @returns The period of the last two pulses in units of seconds.
 */
double Counter::GetPeriod()
{
	if (StatusIsFatal()) return 0.0;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	tCounter::tTimerOutput output = m_counter->readTimerOutput(&localStatus);
	double period;
	if (output.Stalled)
	{
		// Return infinity
		double zero = 0.0;
		period = 1.0 / zero;
	}
	else
	{
		// output.Period is a fixed point number that counts by 2 (24 bits, 25 integer bits)
		period = (double)(output.Period << 1) / (double)output.Count;
	}
	wpi_setError(localStatus);
	return period * 1.0e-6;
}

/**
 * Set the maximum period where the device is still considered "moving".
 * Sets the maximum period where the device is considered moving. This value is used to determine
 * the "stopped" state of the counter using the GetStopped method.
 * @param maxPeriod The maximum period where the counted device is considered moving in
 * seconds.
 */
void Counter::SetMaxPeriod(double maxPeriod)
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeTimerConfig_StallPeriod((uint32_t)(maxPeriod * 1.0e6), &localStatus);
	wpi_setError(localStatus);
}

/**
 * Select whether you want to continue updating the event timer output when there are no samples captured.
 * The output of the event timer has a buffer of periods that are averaged and posted to
 * a register on the FPGA.  When the timer detects that the event source has stopped
 * (based on the MaxPeriod) the buffer of samples to be averaged is emptied.  If you
 * enable the update when empty, you will be notified of the stopped source and the event
 * time will report 0 samples.  If you disable update when empty, the most recent average
 * will remain on the output until a new sample is acquired.  You will never see 0 samples
 * output (except when there have been no events since an FPGA reset) and you will likely not
 * see the stopped bit become true (since it is updated at the end of an average and there are
 * no samples to average).
 */
void Counter::SetUpdateWhenEmpty(bool enabled)
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_counter->writeTimerConfig_UpdateWhenEmpty(enabled, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Determine if the clock is stopped.
 * Determine if the clocked input is stopped based on the MaxPeriod value set using the
 * SetMaxPeriod method. If the clock exceeds the MaxPeriod, then the device (and counter) are
 * assumed to be stopped and it returns true.
 * @return Returns true if the most recent counter period exceeds the MaxPeriod value set by
 * SetMaxPeriod.
 */
bool Counter::GetStopped()
{
	if (StatusIsFatal()) return false;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	return m_counter->readTimerOutput_Stalled(&localStatus);
	wpi_setError(localStatus);
}

/**
 * The last direction the counter value changed.
 * @return The last direction the counter value changed.
 */
bool Counter::GetDirection()
{
	if (StatusIsFatal()) return false;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool value = m_counter->readOutput_Direction(&localStatus);
	wpi_setError(localStatus);
	return value;
}

/**
 * Set the Counter to return reversed sensing on the direction.
 * This allows counters to change the direction they are counting in the case of 1X and 2X
 * quadrature encoding only. Any other counter mode isn't supported.
 * @param reverseDirection true if the value counted should be negated.
 */
void Counter::SetReverseDirection(bool reverseDirection)
{
	if (StatusIsFatal()) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	if (m_counter->readConfig_Mode(&localStatus) == kExternalDirection)
	{
		if (reverseDirection)
			SetDownSourceEdge(true, true);
		else
			SetDownSourceEdge(false, true);
	}
	wpi_setError(localStatus);
}


void Counter::UpdateTable() {
	if (m_table != NULL) {
		m_table->PutNumber("Value", Get());
	}
}

void Counter::StartLiveWindowMode() {
	
}

void Counter::StopLiveWindowMode() {
	
}

std::string Counter::GetSmartDashboardType() {
	return "Counter";
}

void Counter::InitTable(ITable *subTable) {
	m_table = subTable;
	UpdateTable();
}

ITable * Counter::GetTable() {
	return m_table;
}

