/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Counter.h"
#include "AnalogTrigger.h"
#include "DigitalInput.h"
//#include "NetworkCommunication/UsageReporting.h"
#include "Resource.h"
#include "WPIErrors.h"

/**
 * Create an instance of a counter object.
 * This creates a ChipObject counter and initializes status variables appropriately
 *
 * The counter will start counting immediately.
 * @param mode The counter mode
 */
void Counter::InitCounter(Mode mode)
{
	m_table = NULL;

	int32_t status = 0;
	m_index = 0;
	m_counter = initializeCounter(mode, &m_index, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));

	m_upSource = NULL;
	m_downSource = NULL;
	m_allocatedUpSource = false;
	m_allocatedDownSource = false;

	SetMaxPeriod(.5);

	HALReport(HALUsageReporting::kResourceType_Counter, m_index, mode);
}

/**
 * Create an instance of a counter where no sources are selected.
 * They all must be selected by calling functions to specify the upsource and the downsource
 * independently.
 *
 * The counter will start counting immediately.
 */
Counter::Counter() :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
}

/**
 * Create an instance of a counter from a Digital Source (such as a Digital Input).
 * This is used if an existing digital input is to be shared by multiple other objects such
 * as encoders or if the Digital Source is not a Digital Input channel (such as an Analog Trigger).
 *
 * The counter will start counting immediately.
 * @param source A pointer to the existing DigitalSource object. It will be set as the Up Source.
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

/**
 * Create an instance of a counter from a Digital Source (such as a Digital Input).
 * This is used if an existing digital input is to be shared by multiple other objects such
 * as encoders or if the Digital Source is not a Digital Input channel (such as an Analog Trigger).
 *
 * The counter will start counting immediately.
 * @param source A reference to the existing DigitalSource object. It will be set as the Up Source.
 */
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
 * Create an up-Counter instance given a channel.
 *
 * The counter will start counting immediately.
 * @param channel The DIO channel to use as the up source. 0-9 are on-board, 10-25 are on the MXP 
 */
Counter::Counter(int32_t channel) :
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
 * Create an instance of a simple up-Counter given an analog trigger.
 * Use the trigger state output from the analog trigger.
 *
 * The counter will start counting immediately.
 * @param trigger The pointer to the existing AnalogTrigger object.
 */
Counter::Counter(AnalogTrigger *trigger) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
	SetUpSource(trigger->CreateOutput(kState));
	ClearDownSource();
	m_allocatedUpSource = true;
}

/**
 * Create an instance of a Counter object.
 * Create an instance of a simple up-Counter given an analog trigger.
 * Use the trigger state output from the analog trigger.
 *
 * The counter will start counting immediately.
 * @param trigger The reference to the existing AnalogTrigger object.
 */
Counter::Counter(AnalogTrigger &trigger) :
	m_upSource(NULL),
	m_downSource(NULL),
	m_counter(NULL)
{
	InitCounter();
	SetUpSource(trigger.CreateOutput(kState));
	ClearDownSource();
	m_allocatedUpSource = true;
}

/**
 * Create an instance of a Counter object.
 * Creates a full up-down counter given two Digital Sources
 * @param encodingType The quadrature decoding mode (1x or 2x)
 * @param upSource The pointer to the DigitalSource to set as the up source
 * @param downSource The pointer to the DigitalSource to set as the down source
 * @param inverted True to invert the output (reverse the direction)
 */
	
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
	int32_t status = 0;

	if (encodingType == k1X)
	{
		SetUpSourceEdge(true, false);
		setCounterAverageSize(m_counter, 1, &status);
	}
	else
	{
		SetUpSourceEdge(true, true);
		setCounterAverageSize(m_counter, 2, &status);
	}

	wpi_setErrorWithContext(status, getHALErrorMessage(status));
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

	int32_t status = 0;
	freeCounter(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
	m_counter = NULL;
}

/**
 * Set the upsource for the counter as a digital input channel.
 * @param channel The DIO channel to use as the up source. 0-9 are on-board, 10-25 are on the MXP 
 */
void Counter::SetUpSource(int32_t channel)
{
    if (StatusIsFatal()) return;
    SetUpSource(new DigitalInput(channel));
    m_allocatedUpSource = true;
}

/**
 * Set the up counting source to be an analog trigger.
 * @param analogTrigger The analog trigger object that is used for the Up Source
 * @param triggerType The analog trigger output that will trigger the counter.
 */
void Counter::SetUpSource(AnalogTrigger *analogTrigger, AnalogTriggerType triggerType)
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
void Counter::SetUpSource(AnalogTrigger &analogTrigger, AnalogTriggerType triggerType)
{
	SetUpSource(&analogTrigger, triggerType);
}

/**
 * Set the source object that causes the counter to count up.
 * Set the up counting DigitalSource.
 * @param source Pointer to the DigitalSource object to set as the up source
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
		int32_t status = 0;
		setCounterUpSource(m_counter, source->GetChannelForRouting(),
		                   source->GetAnalogTriggerForRouting(), &status);
		wpi_setErrorWithContext(status, getHALErrorMessage(status));
	}
}

/**
 * Set the source object that causes the counter to count up.
 * Set the up counting DigitalSource.
 * @param source Reference to the DigitalSource object to set as the up source
 */
void Counter::SetUpSource(DigitalSource &source)
{
	SetUpSource(&source);
}

/**
 * Set the edge sensitivity on an up counting source.
 * Set the up source to either detect rising edges or falling edges or both.
 * @param risingEdge True to trigger on rising edges
 * @param fallingEdge True to trigger on falling edges
 */
void Counter::SetUpSourceEdge(bool risingEdge, bool fallingEdge)
{
	if (StatusIsFatal()) return;
	if (m_upSource == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "Must set non-NULL UpSource before setting UpSourceEdge");
	}
	int32_t status = 0;
	setCounterUpSourceEdge(m_counter, risingEdge, fallingEdge, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
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
	int32_t status = 0;
	clearCounterUpSource(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
}

/**
 * Set the down counting source to be a digital input channel.
 * @param channel The DIO channel to use as the up source. 0-9 are on-board, 10-25 are on the MXP 
 */
void Counter::SetDownSource(int32_t channel)
{
	if (StatusIsFatal()) return;
	SetDownSource(new DigitalInput(channel));
	m_allocatedDownSource = true;
}

/**
 * Set the down counting source to be an analog trigger.
 * @param analogTrigger The analog trigger object that is used for the Down Source
 * @param triggerType The analog trigger output that will trigger the counter.
 */
void Counter::SetDownSource(AnalogTrigger *analogTrigger, AnalogTriggerType triggerType)
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
void Counter::SetDownSource(AnalogTrigger &analogTrigger, AnalogTriggerType triggerType)
{
	SetDownSource(&analogTrigger, triggerType);
}

/**
 * Set the source object that causes the counter to count down.
 * Set the down counting DigitalSource.
 * @param source Pointer to the DigitalSource object to set as the down source
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
		int32_t status = 0;
		setCounterDownSource(m_counter, source->GetChannelForRouting(),
		                     source->GetAnalogTriggerForRouting(), &status);
		wpi_setErrorWithContext(status, getHALErrorMessage(status));
	}
}

/**
 * Set the source object that causes the counter to count down.
 * Set the down counting DigitalSource.
 * @param source Reference to the DigitalSource object to set as the down source
 */
void Counter::SetDownSource(DigitalSource &source)
{
	SetDownSource(&source);
}

/**
 * Set the edge sensitivity on a down counting source.
 * Set the down source to either detect rising edges or falling edges.
 * @param risingEdge True to trigger on rising edges
 * @param fallingEdge True to trigger on falling edges
 */
void Counter::SetDownSourceEdge(bool risingEdge, bool fallingEdge)
{
	if (StatusIsFatal()) return;
	if (m_downSource == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "Must set non-NULL DownSource before setting DownSourceEdge");
	}
	int32_t status = 0;
	setCounterDownSourceEdge(m_counter, risingEdge, fallingEdge, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
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
	int32_t status = 0;
	clearCounterDownSource(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
}

/**
 * Set standard up / down counting mode on this counter.
 * Up and down counts are sourced independently from two inputs.
 */
void Counter::SetUpDownCounterMode()
{
	if (StatusIsFatal()) return;
	int32_t status = 0;
	setCounterUpDownMode(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
}

/**
 * Set external direction mode on this counter.
 * Counts are sourced on the Up counter input.
 * The Down counter input represents the direction to count.
 */
void Counter::SetExternalDirectionMode()
{
	if (StatusIsFatal()) return;
	int32_t status = 0;
	setCounterExternalDirectionMode(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
}

/**
 * Set Semi-period mode on this counter.
 * Counts up on both rising and falling edges.
 */
void Counter::SetSemiPeriodMode(bool highSemiPeriod)
{
	if (StatusIsFatal()) return;
	int32_t status = 0;
	setCounterSemiPeriodMode(m_counter, highSemiPeriod, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
}

/**
 * Configure the counter to count in up or down based on the length of the input pulse.
 * This mode is most useful for direction sensitive gear tooth sensors.
 * @param threshold The pulse length beyond which the counter counts the opposite direction.  Units are seconds.
 */
void Counter::SetPulseLengthMode(float threshold)
{
	if (StatusIsFatal()) return;
	int32_t status = 0;
	setCounterPulseLengthMode(m_counter, threshold, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
}


/**
 * Get the Samples to Average which specifies the number of samples of the timer to
 * average when calculating the period. Perform averaging to account for
 * mechanical imperfections or as oversampling to increase resolution.
 * @return SamplesToAverage The number of samples being averaged (from 1 to 127)
 */
int Counter::GetSamplesToAverage()
{
	int32_t status = 0;
	int32_t samples = getCounterSamplesToAverage(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
	return samples;
}

/**
 * Set the Samples to Average which specifies the number of samples of the timer to
 * average when calculating the period. Perform averaging to account for
 * mechanical imperfections or as oversampling to increase resolution.
 * @param samplesToAverage The number of samples to average from 1 to 127.
 */
void Counter::SetSamplesToAverage (int samplesToAverage) {
	if (samplesToAverage < 1 || samplesToAverage > 127)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "Average counter values must be between 1 and 127");
	}
	int32_t status = 0;
	setCounterSamplesToAverage(m_counter, samplesToAverage, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
}

/**
 * Read the current counter value.
 * Read the value at this instant. It may still be running, so it reflects the current value. Next
 * time it is read, it might have a different value.
 */
int32_t Counter::Get()
{
	if (StatusIsFatal()) return 0;
	int32_t status = 0;
	int32_t value = getCounter(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
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
	int32_t status = 0;
	resetCounter(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
}

/**
 * Get the Period of the most recent count.
 * Returns the time interval of the most recent count. This can be used for velocity calculations
 * to determine shaft speed.
 * @returns The period between the last two pulses in units of seconds.
 */
double Counter::GetPeriod()
{
	if (StatusIsFatal()) return 0.0;
	int32_t status = 0;
	double value = getCounterPeriod(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
	return value;
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
	int32_t status = 0;
	setCounterMaxPeriod(m_counter, maxPeriod, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
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
 * @param enabled True to enable update when empty
 */
void Counter::SetUpdateWhenEmpty(bool enabled)
{
	if (StatusIsFatal()) return;
	int32_t status = 0;
	setCounterUpdateWhenEmpty(m_counter, enabled, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
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
	int32_t status = 0;
	bool value = getCounterStopped(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
	return value;
}

/**
 * The last direction the counter value changed.
 * @return The last direction the counter value changed.
 */
bool Counter::GetDirection()
{
	if (StatusIsFatal()) return false;
	int32_t status = 0;
	bool value = getCounterDirection(m_counter, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
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
	int32_t status = 0;
	setCounterReverseDirection(m_counter, reverseDirection, &status);
	wpi_setErrorWithContext(status, getHALErrorMessage(status));
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
