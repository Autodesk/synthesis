/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "AnalogInput.h"
#include "WPIErrors.h"
#include "LiveWindow/LiveWindow.h"

/**
 * Common initialization.
 */
void AnalogInput::InitAnalogInput(uint32_t channel)
{
    m_table = NULL;

	m_channel = channel;
	char buffer[50];
	int n = sprintf(buffer, "analog/%d", channel);
	m_impl = new SimFloatInput(buffer);

	LiveWindow::GetInstance()->AddSensor("AnalogInput", channel, this);
}

/**
 * Construct an analog input.
 * 
 * @param channel The channel number to represent.
 */
AnalogInput::AnalogInput(uint32_t channel)
{
	InitAnalogInput(channel);
}

/**
 * Channel destructor.
 */
AnalogInput::~AnalogInput()
{
}

/**
 * Get a scaled sample straight from this channel.
 * The value is scaled to units of Volts using the calibrated scaling data from GetLSBWeight() and GetOffset().
 * @return A scaled sample straight from this channel.
 */
float AnalogInput::GetVoltage()
{
    return m_impl->Get();
}

/**
 * Get a scaled sample from the output of the oversample and average engine for this channel.
 * The value is scaled to units of Volts using the calibrated scaling data from GetLSBWeight() and GetOffset().
 * Using oversampling will cause this value to be higher resolution, but it will update more slowly.
 * Using averaging will cause this value to be more stable, but it will update more slowly.
 * @return A scaled sample from the output of the oversample and average engine for this channel.
 */
float AnalogInput::GetAverageVoltage()
{
    return m_impl->Get();
}

/**
 * Get the channel number.
 * @return The channel number.
 */
uint32_t AnalogInput::GetChannel()
{
	return m_channel;
}

/**
 * Get the Average value for the PID Source base object.
 * 
 * @return The average voltage.
 */
double AnalogInput::PIDGet() 
{
	return GetAverageVoltage();
}

void AnalogInput::UpdateTable() {
	if (m_table != NULL) {
		m_table->PutNumber("Value", GetAverageVoltage());
	}
}

void AnalogInput::StartLiveWindowMode() {
	
}

void AnalogInput::StopLiveWindowMode() {
	
}

std::string AnalogInput::GetSmartDashboardType() {
	return "Analog Input";
}

void AnalogInput::InitTable(ITable *subTable) {
	m_table = subTable;
	UpdateTable();
}

ITable * AnalogInput::GetTable() {
	return m_table;
}
