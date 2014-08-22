/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef DIGITAL_INPUT_H_
#define DIGITAL_INPUT_H_

class DigitalModule;

#include "DigitalSource.h"
#include "LiveWindow/LiveWindowSendable.h"

/**
 * Class to read a digital input.
 * This class will read digital inputs and return the current value on the channel. Other devices
 * such as encoders, gear tooth sensors, etc. that are implemented elsewhere will automatically
 * allocate digital inputs and outputs as required. This class is only for devices like switches
 * etc. that aren't implemented anywhere else.
 */
class DigitalInput : public DigitalSource, public LiveWindowSendable {
public:
	explicit DigitalInput(uint32_t channel);
	DigitalInput(uint8_t moduleNumber, uint32_t channel);
	virtual ~DigitalInput();
	uint32_t Get();
	uint32_t GetChannel();

	// Digital Source Interface
	virtual uint32_t GetChannelForRouting();
	virtual uint32_t GetModuleForRouting();
	virtual bool GetAnalogTriggerForRouting();
	
	// Interruptable Interface
	virtual void RequestInterrupts(tInterruptHandler handler, void *param=NULL); ///< Asynchronus handler version.
	virtual void RequestInterrupts();		///< Synchronus Wait version.
	void SetUpSourceEdge(bool risingEdge, bool fallingEdge);

	void UpdateTable();
	void StartLiveWindowMode();
	void StopLiveWindowMode();
	std::string GetSmartDashboardType();
	void InitTable(ITable *subTable);
	ITable * GetTable();

private:
	void InitDigitalInput(uint8_t moduleNumber, uint32_t channel);
	uint32_t m_channel;
	DigitalModule *m_module;
	bool m_lastValue;
	
	ITable *m_table;
};

#endif

