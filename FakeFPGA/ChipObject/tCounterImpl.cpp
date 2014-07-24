#include "tCounterImpl.h"
#include "NiFpgaState.h"

namespace nFPGA {
	
	tCounter_Impl::tCounter_Impl(NiFpgaState *state, unsigned char sys_index) {
		this->state = state;
		this->sys_index = sys_index;

		this->counterConfig.value = 0;
		this->counterConfig.Enable = false;

		this->counterOutput.value = 0;

		this->timerConfig.value = 0;
		this->timerOutput.value = 0;

		// TODO Actually Implement
	}

	tCounter_Impl::~tCounter_Impl() {
		if (this->state->counter[this->sys_index] == this) {
			this->state->counter[this->sys_index] = NULL;
		}
	}

	tSystemInterface *tCounter_Impl::getSystemInterface() {
		return state;
	}

	unsigned char tCounter_Impl::getSystemIndex() {
		return sys_index;
	}

#pragma region readOutput
	tCounter_Impl::tOutput tCounter_Impl::readOutput(tRioStatusCode *status) {*status = NiFpga_Status_Success;
	return counterOutput;
	}
	bool tCounter_Impl::readOutput_Direction(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterOutput.Direction; 
	}
	signed int tCounter_Impl::readOutput_Value(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterOutput.Value; 
	}
#pragma endregion

#pragma region writeConfig
	void tCounter_Impl::writeConfig(tConfig value, tRioStatusCode *status) {
		*status = NiFpga_Status_Success;
		counterConfig = value;
	}
	void tCounter_Impl::writeConfig_UpSource_Channel(unsigned char value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.UpSource_Channel = value; 
	}
	void tCounter_Impl::writeConfig_UpSource_Module(unsigned char value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.UpSource_Module = value; 
	}
	void tCounter_Impl::writeConfig_UpSource_AnalogTrigger(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.UpSource_AnalogTrigger = value; 
	}
	void tCounter_Impl::writeConfig_DownSource_Channel(unsigned char value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.DownSource_Channel = value; 
	}
	void tCounter_Impl::writeConfig_DownSource_Module(unsigned char value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.DownSource_Module = value; 
	}
	void tCounter_Impl::writeConfig_DownSource_AnalogTrigger(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.DownSource_AnalogTrigger = value; 
	}
	void tCounter_Impl::writeConfig_IndexSource_Channel(unsigned char value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.IndexSource_Channel = value; 
	}
	void tCounter_Impl::writeConfig_IndexSource_Module(unsigned char value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.IndexSource_Module = value; 
	}
	void tCounter_Impl::writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.IndexSource_AnalogTrigger = value; 
	}
	void tCounter_Impl::writeConfig_IndexActiveHigh(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.IndexActiveHigh = value; 
	}
	void tCounter_Impl::writeConfig_UpRisingEdge(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.UpRisingEdge = value; 
	}
	void tCounter_Impl::writeConfig_UpFallingEdge(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.UpFallingEdge = value; 
	}
	void tCounter_Impl::writeConfig_DownRisingEdge(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.DownRisingEdge = value; 
	}
	void tCounter_Impl::writeConfig_DownFallingEdge(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.DownFallingEdge = value; 
	}
	void tCounter_Impl::writeConfig_Mode(unsigned char value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.Mode = value; 
	}
	void tCounter_Impl::writeConfig_PulseLengthThreshold(unsigned short value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.PulseLengthThreshold = value; 
	}
	void tCounter_Impl::writeConfig_Enable(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		counterConfig.Enable = value; 
	}
#pragma endregion

#pragma region readConfig
	tCounter_Impl::tConfig tCounter_Impl::readConfig(tRioStatusCode *status) {
		*status = NiFpga_Status_Success;
		return counterConfig;
	}
	unsigned char tCounter_Impl::readConfig_UpSource_Channel(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.UpSource_Channel; 
	}
	unsigned char tCounter_Impl::readConfig_UpSource_Module(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.UpSource_Module; 
	}
	bool tCounter_Impl::readConfig_UpSource_AnalogTrigger(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.UpSource_AnalogTrigger; 
	}
	unsigned char tCounter_Impl::readConfig_DownSource_Channel(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.DownSource_Channel; 
	}
	unsigned char tCounter_Impl::readConfig_DownSource_Module(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.DownSource_Module; 
	}
	bool tCounter_Impl::readConfig_DownSource_AnalogTrigger(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.DownSource_AnalogTrigger; 
	}
	unsigned char tCounter_Impl::readConfig_IndexSource_Channel(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.IndexSource_Channel; 
	}
	unsigned char tCounter_Impl::readConfig_IndexSource_Module(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.IndexSource_Module; 
	}
	bool tCounter_Impl::readConfig_IndexSource_AnalogTrigger(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.IndexSource_AnalogTrigger; 
	}
	bool tCounter_Impl::readConfig_IndexActiveHigh(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.IndexActiveHigh; 
	}
	bool tCounter_Impl::readConfig_UpRisingEdge(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.UpRisingEdge; 
	}
	bool tCounter_Impl::readConfig_UpFallingEdge(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.UpFallingEdge; 
	}
	bool tCounter_Impl::readConfig_DownRisingEdge(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.DownRisingEdge; 
	}
	bool tCounter_Impl::readConfig_DownFallingEdge(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.DownFallingEdge; 
	}
	unsigned char tCounter_Impl::readConfig_Mode(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.Mode; 
	}
	unsigned short tCounter_Impl::readConfig_PulseLengthThreshold(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.PulseLengthThreshold; 
	}
	bool tCounter_Impl::readConfig_Enable(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return counterConfig.Enable; 
	}
#pragma endregion

#pragma region readTimerOutput
	tCounter_Impl::tTimerOutput tCounter_Impl::readTimerOutput(tRioStatusCode *status) {
		*status = NiFpga_Status_Success;
		return timerOutput;
	}
	unsigned int tCounter_Impl::readTimerOutput_Period(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return timerOutput.Period; 
	}
	signed char tCounter_Impl::readTimerOutput_Count(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return timerOutput.Count; 
	}
	bool tCounter_Impl::readTimerOutput_Stalled(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return timerOutput.Stalled; 
	}
#pragma endregion

	void tCounter_Impl::strobeReset(tRioStatusCode *status) {
		*status = NiFpga_Status_Success;
		// TODO
	}

#pragma region writeTimerConfig
	void tCounter_Impl::writeTimerConfig(tTimerConfig value, tRioStatusCode *status){
		*status = NiFpga_Status_Success;
		timerConfig = value;
	}
	void tCounter_Impl::writeTimerConfig_StallPeriod(unsigned int value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		timerConfig.StallPeriod = value; 
	}
	void tCounter_Impl::writeTimerConfig_AverageSize(unsigned char value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		timerConfig.AverageSize = value; 
	}
	void tCounter_Impl::writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		timerConfig.UpdateWhenEmpty = value; 
	}
#pragma endregion

#pragma region readTimerConfig
	tCounter_Impl::tTimerConfig tCounter_Impl::readTimerConfig(tRioStatusCode *status){
		*status = NiFpga_Status_Success;
		return timerConfig;
	}
	unsigned int tCounter_Impl::readTimerConfig_StallPeriod(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return timerConfig.StallPeriod; 
	}
	unsigned char tCounter_Impl::readTimerConfig_AverageSize(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return timerConfig.AverageSize; 
	}
	bool tCounter_Impl::readTimerConfig_UpdateWhenEmpty(tRioStatusCode *status) 
	{
		*status = NiFpga_Status_Success;
		return timerConfig.UpdateWhenEmpty; 
	}
#pragma endregion
}