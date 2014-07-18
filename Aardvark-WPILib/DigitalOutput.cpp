/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "DigitalOutput.h"
#include "DigitalModule.h"
#include "NetworkCommunication/UsageReporting.h"
#include "Resource.h"
#include "WPIErrors.h"

extern Resource *interruptsResource;

/**
 * Create an instance of a DigitalOutput.
 * Creates a digital output given a slot and channel. Common creation routine
 * for all constructors.
 */
void DigitalOutput::InitDigitalOutput(uint8_t moduleNumber, uint32_t channel)
{
	m_table = NULL;
	char buf[64];
	if (!CheckDigitalModule(moduleNumber))
	{
		snprintf(buf, 64, "Digital Module %d", moduleNumber);
		wpi_setWPIErrorWithContext(ModuleIndexOutOfRange, buf);
		return;
	}
	if (!CheckDigitalChannel(channel))
	{
		snprintf(buf, 64, "Digital Channel %lu", channel);
		wpi_setWPIErrorWithContext(ChannelIndexOutOfRange, buf);
		return;
	}
	m_channel = channel;
	m_pwmGenerator = ~0ul;
	m_module = DigitalModule::GetInstance(moduleNumber);
	m_module->AllocateDIO(m_channel, false);

	nUsageReporting::report(nUsageReporting::kResourceType_DigitalOutput, channel, moduleNumber - 1);
}

/**
 * Create an instance of a digital output.
 * Create a digital output given a channel. The default module is used.
 *
 * @param channel The digital channel (1..14).
 */
DigitalOutput::DigitalOutput(uint32_t channel)
{
	InitDigitalOutput(GetDefaultDigitalModule(), channel);
}

/**
 * Create an instance of a digital output.
 * Create an instance of a digital output given a module number and channel.
 *
 * @param moduleNumber The digital module (1 or 2).
 * @param channel The digital channel (1..14).
 */
DigitalOutput::DigitalOutput(uint8_t moduleNumber, uint32_t channel)
{
	InitDigitalOutput(moduleNumber, channel);
}

/**
 * Free the resources associated with a digital output.
 */
DigitalOutput::~DigitalOutput()
{
	if (StatusIsFatal()) return;
	// Disable the PWM in case it was running.
	DisablePWM();
	m_module->FreeDIO(m_channel);
}

/**
 * Set the value of a digital output.
 * Set the value of a digital output to either one (true) or zero (false).
 */
void DigitalOutput::Set(uint32_t value)
{
	if (StatusIsFatal()) return;
	m_module->SetDIO(m_channel, value);
}

/**
 * @return The GPIO channel number that this object represents.
 */
uint32_t DigitalOutput::GetChannel()
{
	return m_channel;
}

/**
 * Output a single pulse on the digital output line.
 * Send a single pulse on the digital output line where the pulse diration is specified in seconds.
 * Maximum pulse length is 0.0016 seconds.
 * @param length The pulselength in seconds
 */
void DigitalOutput::Pulse(float length)
{
	if (StatusIsFatal()) return;
	m_module->Pulse(m_channel, length);
}

/**
 * Determine if the pulse is still going.
 * Determine if a previously started pulse is still going.
 */
bool DigitalOutput::IsPulsing()
{
	if (StatusIsFatal()) return false;
	return m_module->IsPulsing(m_channel);
}

/**
 * Change the PWM frequency of the PWM output on a Digital Output line.
 * 
 * The valid range is from 0.6 Hz to 19 kHz.  The frequency resolution is logarithmic.
 * 
 * There is only one PWM frequency per digital module.
 * 
 * @param rate The frequency to output all digital output PWM signals on this module.
 */
void DigitalOutput::SetPWMRate(float rate)
{
	if (StatusIsFatal()) return;
	m_module->SetDO_PWMRate(rate);
}

/**
 * Enable a PWM Output on this line.
 * 
 * Allocate one of the 4 DO PWM generator resources from this module.
 * 
 * Supply the initial duty-cycle to output so as to avoid a glitch when first starting.
 * 
 * The resolution of the duty cycle is 8-bit for low frequencies (1kHz or less)
 * but is reduced the higher the frequency of the PWM signal is.
 * 
 * @param initialDutyCycle The duty-cycle to start generating. [0..1]
 */
void DigitalOutput::EnablePWM(float initialDutyCycle)
{
	if (StatusIsFatal()) return;
	if (m_pwmGenerator != ~0ul) return;
	m_pwmGenerator = m_module->AllocateDO_PWM();
	m_module->SetDO_PWMDutyCycle(m_pwmGenerator, initialDutyCycle);
	m_module->SetDO_PWMOutputChannel(m_pwmGenerator, m_channel);
}

/**
 * Change this line from a PWM output back to a static Digital Output line.
 * 
 * Free up one of the 4 DO PWM generator resources that were in use.
 */
void DigitalOutput::DisablePWM()
{
	if (StatusIsFatal()) return;
	// Disable the output by routing to a dead bit.
	m_module->SetDO_PWMOutputChannel(m_pwmGenerator, kDigitalChannels);
	m_module->FreeDO_PWM(m_pwmGenerator);
	m_pwmGenerator = ~0ul;
}

/**
 * Change the duty-cycle that is being generated on the line.
 * 
 * The resolution of the duty cycle is 8-bit for low frequencies (1kHz or less)
 * but is reduced the higher the frequency of the PWM signal is.
 * 
 * @param dutyCycle The duty-cycle to change to. [0..1]
 */
void DigitalOutput::UpdateDutyCycle(float dutyCycle)
{
	if (StatusIsFatal()) return;
	m_module->SetDO_PWMDutyCycle(m_pwmGenerator, dutyCycle);
}

/**
 * @return The value to be written to the channel field of a routing mux.
 */
uint32_t DigitalOutput::GetChannelForRouting()
{
	return DigitalModule::RemapDigitalChannel(GetChannel() - 1);
}

/**
 * @return The value to be written to the module field of a routing mux.
 */
uint32_t DigitalOutput::GetModuleForRouting()
{
	if (StatusIsFatal()) return 0;
	return m_module->GetNumber() - 1;
}

/**
 * @return The value to be written to the analog trigger field of a routing mux.
 */
bool DigitalOutput::GetAnalogTriggerForRouting()
{
	return false;
}

/**
 * Request interrupts asynchronously on this digital output.
 * @param handler The address of the interrupt handler function of type tInterruptHandler that
 * will be called whenever there is an interrupt on the digitial output port.
 * Request interrupts in synchronus mode where the user program interrupt handler will be
 * called when an interrupt occurs.
 * The default is interrupt on rising edges only.
 */
void DigitalOutput::RequestInterrupts(tInterruptHandler handler, void *param)
{
	if (StatusIsFatal()) return;
	uint32_t index = interruptsResource->Allocate("Sync Interrupt");
	if (index == ~0ul)
	{
		CloneError(interruptsResource);
		return;
	}
	m_interruptIndex = index;

	// Creates a manager too
	AllocateInterrupts(false);

	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_interrupt->writeConfig_WaitForAck(false, &localStatus);
	m_interrupt->writeConfig_Source_AnalogTrigger(GetAnalogTriggerForRouting(), &localStatus);
	m_interrupt->writeConfig_Source_Channel(GetChannelForRouting(), &localStatus);
	m_interrupt->writeConfig_Source_Module(GetModuleForRouting(), &localStatus);
	SetUpSourceEdge(true, false);

	m_manager->registerHandler(handler, param, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Request interrupts synchronously on this digital output.
 * Request interrupts in synchronus mode where the user program will have to explicitly
 * wait for the interrupt to occur.
 * The default is interrupt on rising edges only.
 */
void DigitalOutput::RequestInterrupts()
{
	if (StatusIsFatal()) return;
	uint32_t index = interruptsResource->Allocate("Sync Interrupt");
	if (index == ~0ul)
	{
		CloneError(interruptsResource);
		return;
	}
	m_interruptIndex = index;

	AllocateInterrupts(true);

	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_interrupt->writeConfig_Source_AnalogTrigger(GetAnalogTriggerForRouting(), &localStatus);
	m_interrupt->writeConfig_Source_Channel(GetChannelForRouting(), &localStatus);
	m_interrupt->writeConfig_Source_Module(GetModuleForRouting(), &localStatus);
	SetUpSourceEdge(true, false);
	wpi_setError(localStatus);
}

void DigitalOutput::SetUpSourceEdge(bool risingEdge, bool fallingEdge)
{
	if (StatusIsFatal()) return;
	if (m_interrupt == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "You must call RequestInterrupts before SetUpSourceEdge");
		return;
	}
	tRioStatusCode localStatus = NiFpga_Status_Success;
	if (m_interrupt != NULL)
	{
		m_interrupt->writeConfig_RisingEdge(risingEdge, &localStatus);
		m_interrupt->writeConfig_FallingEdge(fallingEdge, &localStatus);
	}
	wpi_setError(localStatus);
}

void DigitalOutput::ValueChanged(ITable* source, const std::string& key, EntryValue value, bool isNew) {
	Set(value.b);
}

void DigitalOutput::UpdateTable() {
}

void DigitalOutput::StartLiveWindowMode() {
	if (m_table != NULL) {
		m_table->AddTableListener("Value", this, true);
	}
}

void DigitalOutput::StopLiveWindowMode() {
	if (m_table != NULL) {
		m_table->RemoveTableListener(this);
	}
}

std::string DigitalOutput::GetSmartDashboardType() {
	return "Digital Output";
}

void DigitalOutput::InitTable(ITable *subTable) {
	m_table = subTable;
	UpdateTable();
}

ITable * DigitalOutput::GetTable() {
	return m_table;
}


