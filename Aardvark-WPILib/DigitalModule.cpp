/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "DigitalModule.h"
#include "I2C.h"
#include "PWM.h"
#include "Resource.h"
#include "OSAL/Synchronized.h"
#include "WPIErrors.h"
#include <math.h>
#include "OSAL/Task.h"

static Resource *DIOChannels = NULL;
static Resource *DO_PWMGenerators[tDIO::kNumSystems] = {NULL};

/**
 * Get an instance of an Digital Module.
 * Singleton digital module creation where a module is allocated on the first use
 * and the same module is returned on subsequent uses.
 *
 * @param moduleNumber The digital module to get (1 or 2).
 */
DigitalModule* DigitalModule::GetInstance(uint8_t moduleNumber)
{
	if (CheckDigitalModule(moduleNumber))
	{
		return (DigitalModule *)GetModule(nLoadOut::kModuleType_Digital, moduleNumber);
	}

	// If this wasn't caught before now, make sure we say what's wrong before we crash
	char buf[64];
	sprintf_s(buf, "Digital Module %d", moduleNumber);
	wpi_setGlobalWPIErrorWithContext(ModuleIndexOutOfRange, buf);

	return NULL;
}

/**
 * Create a new instance of an digital module.
 * Create an instance of the digital module object. Initialize all the parameters
 * to reasonable values on start.
 * Setting a global value on an digital module can be done only once unless subsequent
 * values are set the previously set value.
 * Digital modules are a singleton, so the constructor is never called outside of this class.
 *
 * @param moduleNumber The digital module to create (1 or 2).
 */
DigitalModule::DigitalModule(uint8_t moduleNumber)
	: Module(nLoadOut::kModuleType_Digital, moduleNumber)
	, m_fpgaDIO (NULL)
{
	Resource::CreateResourceObject(&DIOChannels, tDIO::kNumSystems * kDigitalChannels);
	Resource::CreateResourceObject(&DO_PWMGenerators[m_moduleNumber - 1], tDIO::kNumDO_PWMDutyCycleElements);
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaDIO = tDIO::create(m_moduleNumber - 1, &localStatus);
	wpi_setError(localStatus);

	// Make sure that the 9403 IONode has had a chance to initialize before continuing.
	while(m_fpgaDIO->readLoopTiming(&localStatus) == 0) Sleep(1);//taskDelay(1);
	
	if (m_fpgaDIO->readLoopTiming(&localStatus) != kExpectedLoopTiming)
	{
		char err[128];
		sprintf_s(err, "DIO LoopTiming: %d, expecting: %lu\n", m_fpgaDIO->readLoopTiming(&localStatus), kExpectedLoopTiming);
		wpi_setWPIErrorWithContext(LoopTimingError, err);
	}
	
    //Calculate the length, in ms, of one DIO loop
    double loopTime = m_fpgaDIO->readLoopTiming(&localStatus)/(kSystemClockTicksPerMicrosecond*1e3);
    
	m_fpgaDIO->writePWMConfig_Period((uint16_t) (PWM::kDefaultPwmPeriod/loopTime + .5), &localStatus);
	m_fpgaDIO->writePWMConfig_MinHigh((uint16_t) ((PWM::kDefaultPwmCenter-PWM::kDefaultPwmStepsDown*loopTime)/loopTime + .5), &localStatus);

	// Ensure that PWM output values are set to OFF
	for (uint32_t pwm_index = 1; pwm_index <= kPwmChannels; pwm_index++)
	{
		SetPWM(pwm_index, PWM::kPwmDisabled);
		SetPWMPeriodScale(pwm_index, 3); // Set all to 4x by default.
	}

	// Turn off all relay outputs.
	m_fpgaDIO->writeSlowValue_RelayFwd(0, &localStatus);
	m_fpgaDIO->writeSlowValue_RelayRev(0, &localStatus);
	wpi_setError(localStatus);

	// Create a semaphore to protect changes to the digital output values
//	m_digitalSemaphore = semMCreate(SEM_Q_PRIORITY | SEM_DELETE_SAFE | SEM_INVERSION_SAFE);

	// Create a semaphore to protect changes to the relay values
//	m_relaySemaphore = semMCreate(SEM_Q_PRIORITY | SEM_DELETE_SAFE | SEM_INVERSION_SAFE);

	// Create a semaphore to protect changes to the DO PWM config
//	m_doPwmSemaphore = semMCreate(SEM_Q_PRIORITY | SEM_DELETE_SAFE | SEM_INVERSION_SAFE);

	AddToSingletonList();
}

DigitalModule::~DigitalModule()
{
	//semDelete(m_doPwmSemaphore);
	//m_doPwmSemaphore = NULL;
	//semDelete(m_relaySemaphore);
	//m_relaySemaphore = NULL;
	//semDelete(m_digitalSemaphore);
	//m_digitalSemaphore = NULL;
	delete m_fpgaDIO;
}

/**
 * Set a PWM channel to the desired value. The values range from 0 to 255 and the period is controlled
 * by the PWM Period and MinHigh registers.
 * 
 * @param channel The PWM channel to set.
 * @param value The PWM value to set.
 */
void DigitalModule::SetPWM(uint32_t channel, uint8_t value)
{
	CheckPWMChannel(channel);
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaDIO->writePWMValue(channel - 1, value, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Get a value from a PWM channel. The values range from 0 to 255.
 * 
 * @param channel The PWM channel to read from.
 * @return The raw PWM value.
 */
uint8_t DigitalModule::GetPWM(uint32_t channel)
{
	CheckPWMChannel(channel);
	tRioStatusCode localStatus = NiFpga_Status_Success;
	return m_fpgaDIO->readPWMValue(channel - 1, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Set how how often the PWM signal is squelched, thus scaling the period.
 * 
 * @param channel The PWM channel to configure.
 * @param squelchMask The 2-bit mask of outputs to squelch.
 */
void DigitalModule::SetPWMPeriodScale(uint32_t channel, uint32_t squelchMask)
{
	CheckPWMChannel(channel);
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaDIO->writePWMPeriodScale(channel - 1, squelchMask, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Set the state of a relay.
 * Set the state of a relay output to be forward. Relays have two outputs and each is
 * independently set to 0v or 12v.
 */
void DigitalModule::SetRelayForward(uint32_t channel, bool on)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	CheckRelayChannel(channel);
	{
		Synchronized sync(m_relaySemaphore);
		uint8_t forwardRelays = m_fpgaDIO->readSlowValue_RelayFwd(&localStatus);
		if (on)
			forwardRelays |= 1 << (channel - 1);
		else
			forwardRelays &= ~(1 << (channel - 1));
		m_fpgaDIO->writeSlowValue_RelayFwd(forwardRelays, &localStatus);
	}
	wpi_setError(localStatus);
}

/**
 * Set the state of a relay.
 * Set the state of a relay output to be reverse. Relays have two outputs and each is
 * independently set to 0v or 12v.
 */
void DigitalModule::SetRelayReverse(uint32_t channel, bool on)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	CheckRelayChannel(channel);
	{
		Synchronized sync(m_relaySemaphore);
		uint8_t reverseRelays = m_fpgaDIO->readSlowValue_RelayRev(&localStatus);
		if (on)
			reverseRelays |= 1 << (channel - 1);
		else
			reverseRelays &= ~(1 << (channel - 1));
		m_fpgaDIO->writeSlowValue_RelayRev(reverseRelays, &localStatus);
	}
	wpi_setError(localStatus);
}

/**
 * Get the current state of the forward relay channel
 */
bool DigitalModule::GetRelayForward(uint32_t channel)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint8_t forwardRelays = m_fpgaDIO->readSlowValue_RelayFwd(&localStatus);
	wpi_setError(localStatus);
	return (forwardRelays & (1 << (channel - 1))) != 0;
}

/**
 * Get the current state of all of the forward relay channels on this module.
 */
uint8_t DigitalModule::GetRelayForward()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint8_t forwardRelays = m_fpgaDIO->readSlowValue_RelayFwd(&localStatus);
	wpi_setError(localStatus);
	return forwardRelays;
}

/**
 * Get the current state of the reverse relay channel
 */
bool DigitalModule::GetRelayReverse(uint32_t channel)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint8_t reverseRelays = m_fpgaDIO->readSlowValue_RelayRev(&localStatus);
	wpi_setError(localStatus);
	return (reverseRelays & (1 << (channel - 1))) != 0;
	
}

/**
 * Get the current state of all of the reverse relay channels on this module.
 */
uint8_t DigitalModule::GetRelayReverse()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint8_t reverseRelays = m_fpgaDIO->readSlowValue_RelayRev(&localStatus);
	wpi_setError(localStatus);
	return reverseRelays;	
}


/**
 * Allocate Digital I/O channels.
 * Allocate channels so that they are not accidently reused. Also the direction is set at the
 * time of the allocation.
 * 
 * @param channel The Digital I/O channel
 * @param input If true open as input; if false open as output
 * @return Was successfully allocated
 */
bool DigitalModule::AllocateDIO(uint32_t channel, bool input)
{
	char buf[64];
	sprintf_s(buf, 64, "DIO %lu (Module %d)", channel, m_moduleNumber);
	if (DIOChannels->Allocate(kDigitalChannels * (m_moduleNumber - 1) + channel - 1, buf) == ~0ul) return false;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	{
		Synchronized sync(m_digitalSemaphore);
		uint32_t bitToSet = 1 << (RemapDigitalChannel(channel - 1));
		uint32_t outputEnable = m_fpgaDIO->readOutputEnable(&localStatus);
		uint32_t outputEnableValue;
		if (input)
		{
			outputEnableValue = outputEnable & (~bitToSet); // clear the bit for read
		}
		else
		{
			outputEnableValue = outputEnable | bitToSet; // set the bit for write
		}
		m_fpgaDIO->writeOutputEnable(outputEnableValue, &localStatus);
	}
	wpi_setError(localStatus);
	return true;
}

/**
 * Free the resource associated with a digital I/O channel.
 * 
 * @param channel The Digital I/O channel to free
 */
void DigitalModule::FreeDIO(uint32_t channel)
{
	DIOChannels->Free(kDigitalChannels * (m_moduleNumber - 1) + channel - 1);
}

/**
 * Write a digital I/O bit to the FPGA.
 * Set a single value on a digital I/O channel.
 * 
 * @param channel The Digital I/O channel
 * @param value The state to set the digital channel (if it is configured as an output)
 */
void DigitalModule::SetDIO(uint32_t channel, short value)
{
	if (value != 0 && value != 1)
	{
		wpi_setWPIError(NonBinaryDigitalValue);
		if (value != 0)
			value = 1;
	}
	tRioStatusCode localStatus = NiFpga_Status_Success;
	{
		Synchronized sync(m_digitalSemaphore);
		uint16_t currentDIO = m_fpgaDIO->readDO(&localStatus);
		if(value == 0)
		{
			currentDIO = currentDIO & ~(1 << RemapDigitalChannel(channel - 1));
		}
		else if (value == 1)
		{
			currentDIO = currentDIO | (1 << RemapDigitalChannel(channel - 1));
		} 
		m_fpgaDIO->writeDO(currentDIO, &localStatus);
	}
	wpi_setError(localStatus);
}

/**
 * Read a digital I/O bit from the FPGA.
 * Get a single value from a digital I/O channel.
 * 
 * @param channel The digital I/O channel
 * @return The state of the specified channel
 */
bool DigitalModule::GetDIO(uint32_t channel)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint32_t currentDIO = m_fpgaDIO->readDI(&localStatus);
	wpi_setError(localStatus);

	//Shift 00000001 over channel-1 places.
	//AND it against the currentDIO
	//if it == 0, then return false
	//else return true
	return ((currentDIO >> RemapDigitalChannel(channel - 1)) & 1) != 0;
}

/**
 * Read the state of all the Digital I/O lines from the FPGA
 * These are not remapped to logical order.  They are still in hardware order.
 */
uint16_t DigitalModule::GetDIO()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint32_t currentDIO = m_fpgaDIO->readDI(&localStatus);
	wpi_setError(localStatus);
	return currentDIO;
}

/**
 * Read the direction of a the Digital I/O lines
 * A 1 bit means output and a 0 bit means input.
 * 
 * @param channel The digital I/O channel
 * @return The direction of the specified channel
 */
bool DigitalModule::GetDIODirection(uint32_t channel)
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint32_t currentOutputEnable = m_fpgaDIO->readOutputEnable(&localStatus);
	wpi_setError(localStatus);
	
	//Shift 00000001 over channel-1 places.
	//AND it against the currentOutputEnable
	//if it == 0, then return false
	//else return true
	return ((currentOutputEnable >> RemapDigitalChannel(channel - 1)) & 1) != 0;
}

/**
 * Read the direction of all the Digital I/O lines from the FPGA
 * A 1 bit means output and a 0 bit means input.
 * These are not remapped to logical order.  They are still in hardware order.
 */
uint16_t DigitalModule::GetDIODirection()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint32_t currentOutputEnable = m_fpgaDIO->readOutputEnable(&localStatus);
	wpi_setError(localStatus);
	return currentOutputEnable;
}

/**
 * Generate a single pulse.
 * Write a pulse to the specified digital output channel. There can only be a single pulse going at any time.
 * 
 * @param channel The Digital Output channel that the pulse should be output on
 * @param pulseLength The active length of the pulse (in seconds)
 */
void DigitalModule::Pulse(uint32_t channel, float pulseLength)
{
	uint16_t mask = 1 << RemapDigitalChannel(channel - 1);
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_fpgaDIO->writePulseLength((uint8_t)(1.0e9 * pulseLength / (m_fpgaDIO->readLoopTiming(&localStatus) * 25)), &localStatus);
	m_fpgaDIO->writePulse(mask, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Check a DIO line to see if it is currently generating a pulse.
 * 
 * @return A pulse is in progress
 */
bool DigitalModule::IsPulsing(uint32_t channel)
{
	uint16_t mask = 1 << RemapDigitalChannel(channel - 1);
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint16_t pulseRegister = m_fpgaDIO->readPulse(&localStatus);
	wpi_setError(localStatus);
	return (pulseRegister & mask) != 0;
}

/**
 * Check if any DIO line is currently generating a pulse.
 * 
 * @return A pulse on some line is in progress
 */
bool DigitalModule::IsPulsing()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint16_t pulseRegister = m_fpgaDIO->readPulse(&localStatus);
	wpi_setError(localStatus);
	return pulseRegister != 0;
}

/**
 * Allocate a DO PWM Generator.
 * Allocate PWM generators so that they are not accidently reused.
 * 
 * @return PWM Generator refnum
 */
uint32_t DigitalModule::AllocateDO_PWM()
{
	char buf[64];
	sprintf_s(buf, 64, "DO_PWM (Module: %d)", m_moduleNumber);
	return DO_PWMGenerators[(m_moduleNumber - 1)]->Allocate(buf);
}

/**
 * Free the resource associated with a DO PWM generator.
 * 
 * @param pwmGenerator The pwmGen to free that was allocated with AllocateDO_PWM()
 */
void DigitalModule::FreeDO_PWM(uint32_t pwmGenerator)
{
	if (pwmGenerator == ~0ul) return;
	DO_PWMGenerators[(m_moduleNumber - 1)]->Free(pwmGenerator);
}

/**
 * Change the frequency of the DO PWM generator.
 * 
 * The valid range is from 0.6 Hz to 19 kHz.  The frequency resolution is logarithmic.
 * 
 * @param rate The frequency to output all digital output PWM signals on this module.
 */
void DigitalModule::SetDO_PWMRate(float rate)
{
	// Currently rounding in the log rate domain... heavy weight toward picking a higher freq.
	// TODO: Round in the linear rate domain.
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint8_t pwmPeriodPower = (uint8_t)(log(1.0 / (m_fpgaDIO->readLoopTiming(&localStatus) * 0.25E-6 * rate))/log(2.0) + 0.5);
	m_fpgaDIO->writeDO_PWMConfig_PeriodPower(pwmPeriodPower, &localStatus);
	wpi_setError(localStatus);
}

/**
 * Configure which DO channel the PWM siganl is output on
 * 
 * @param pwmGenerator The generator index reserved by AllocateDO_PWM()
 * @param channel The Digital Output channel to output on
 */
void DigitalModule::SetDO_PWMOutputChannel(uint32_t pwmGenerator, uint32_t channel)
{
	if (pwmGenerator == ~0ul) return;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	switch(pwmGenerator)
	{
	case 0:
		m_fpgaDIO->writeDO_PWMConfig_OutputSelect_0(RemapDigitalChannel(channel - 1), &localStatus);
		break;
	case 1:
		m_fpgaDIO->writeDO_PWMConfig_OutputSelect_1(RemapDigitalChannel(channel - 1), &localStatus);
		break;
	case 2:
		m_fpgaDIO->writeDO_PWMConfig_OutputSelect_2(RemapDigitalChannel(channel - 1), &localStatus);
		break;
	case 3:
		m_fpgaDIO->writeDO_PWMConfig_OutputSelect_3(RemapDigitalChannel(channel - 1), &localStatus);
		break;
	}
	wpi_setError(localStatus);
}

/**
 * Configure the duty-cycle of the PWM generator
 * 
 * @param pwmGenerator The generator index reserved by AllocateDO_PWM()
 * @param dutyCycle The percent duty cycle to output [0..1].
 */
void DigitalModule::SetDO_PWMDutyCycle(uint32_t pwmGenerator, float dutyCycle)
{
	if (pwmGenerator == ~0ul) return;
	if (dutyCycle > 1.0) dutyCycle = 1.0;
	if (dutyCycle < 0.0) dutyCycle = 0.0;
	float rawDutyCycle = 256.0 * dutyCycle;
	if (rawDutyCycle > 255.5) rawDutyCycle = 255.5;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	{
		Synchronized sync(m_doPwmSemaphore);
		uint8_t pwmPeriodPower = m_fpgaDIO->readDO_PWMConfig_PeriodPower(&localStatus);
		if (pwmPeriodPower < 4)
		{
			// The resolution of the duty cycle drops close to the highest frequencies.
			rawDutyCycle = rawDutyCycle / pow(2.0, 4 - pwmPeriodPower);
		}
		m_fpgaDIO->writeDO_PWMDutyCycle(pwmGenerator, (uint8_t)rawDutyCycle, &localStatus);
	}
	wpi_setError(localStatus);
}

/**
 * Get the loop timing of the Digital Module
 * 
 * @return The loop time
 */
uint16_t DigitalModule::GetLoopTiming()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint16_t timing = m_fpgaDIO->readLoopTiming(&localStatus);
	wpi_setError(localStatus);
	
	return timing;
}

/**
 * Return a pointer to an I2C object for this digital module
 * The caller is responsible for deleting the pointer.
 * 
 * @param address The address of the device on the I2C bus
 * @return A pointer to an I2C object to talk to the device at address
 */
I2C* DigitalModule::GetI2C(uint32_t address)
{
	return new I2C(this, address);
}


