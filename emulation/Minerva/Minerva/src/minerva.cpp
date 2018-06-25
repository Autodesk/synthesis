// Auto-generated HAL interface for emulation

#include <FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/nInterfaceGlobals.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAI.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAO.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAccel.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAccumulator.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAlarm.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAnalogTrigger.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tBIST.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tCounter.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tDIO.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tDMA.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tEncoder.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tGlobal.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tInterrupt.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tPWM.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tPower.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tRelay.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tSPI.h>
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tSysWatchdog.h>
#include <FRC_FPGA_ChipObject/tDMAChannelDescriptor.h>
#include <FRC_FPGA_ChipObject/tDMAManager.h>
#include <FRC_FPGA_ChipObject/tInterruptManager.h>
#include <FRC_FPGA_ChipObject/tSystem.h>
#include <FRC_FPGA_ChipObject/tSystemInterface.h>
#include <pthread.h>
#include <stdint.h>
#include <stdlib.h>
#include <wpi/SmallString.h>
#include <wpi/SmallVector.h>
#include <wpi/mutex.h>
#include <wpi/raw_ostream.h>
#include <array>
#include <chrono>
#include <cstddef>
#include <limits>
#include <memory>
#include <string>
#include <utility>
#include <vector>
#include "HAL/Accelerometer.h"
#include "HAL/AnalogAccumulator.h"
#include "HAL/AnalogGyro.h"
#include "HAL/AnalogInput.h"
#include "HAL/AnalogOutput.h"
#include "HAL/AnalogTrigger.h"
#include "HAL/CAN.h"
#include "HAL/CANAPI.h"
#include "HAL/ChipObject.h"
#include "HAL/Compressor.h"
#include "HAL/Constants.h"
#include "HAL/Counter.h"
#include "HAL/DIO.h"
#include "HAL/DriverStation.h"
#include "HAL/Errors.h"
#include "HAL/HAL.h"
#include "HAL/I2C.h"
#include "HAL/Interrupts.h"
#include "HAL/Notifier.h"
#include "HAL/PDP.h"
#include "HAL/PWM.h"
#include "HAL/Ports.h"
#include "HAL/Power.h"
#include "HAL/Relay.h"
#include "HAL/SPI.h"
#include "HAL/SerialPort.h"
#include "HAL/Solenoid.h"
#include "HAL/Threads.h"
#include "HAL/Types.h"
#include "HAL/UsageReporting.h"
#include "HAL/handles/HandlesInternal.h"

#include "channel.h"
#include "function_signature.h"  // ParameterValueInfo
#include "handler.h"
using namespace hal;

#ifdef __cplusplus
extern "C" {
#endif

void HAL_SetAccelerometerActive(HAL_Bool active) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool", active});
	callFunc("HAL_SetAccelerometerActive", parameters);
}

void HAL_SetAccelerometerRange(HAL_AccelerometerRange range) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AccelerometerRange", range});
	callFunc("HAL_SetAccelerometerRange", parameters);
}

double HAL_GetAccelerometerX() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<double> c;
	callFunc("HAL_GetAccelerometerX", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetAccelerometerY() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<double> c;
	callFunc("HAL_GetAccelerometerY", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetAccelerometerZ() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<double> c;
	callFunc("HAL_GetAccelerometerZ", parameters, c);
	double x;
	c.get(x);
	return x;
}

HAL_Bool HAL_IsAccumulatorChannel(HAL_AnalogInputHandle analogPortHandle,
								  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_IsAccumulatorChannel", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_InitAccumulator(HAL_AnalogInputHandle analogPortHandle,
						 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_InitAccumulator", parameters);
}

void HAL_ResetAccumulator(HAL_AnalogInputHandle analogPortHandle,
						  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ResetAccumulator", parameters);
}

void HAL_SetAccumulatorCenter(HAL_AnalogInputHandle analogPortHandle,
							  int32_t center, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t", center});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAccumulatorCenter", parameters);
}

void HAL_SetAccumulatorDeadband(HAL_AnalogInputHandle analogPortHandle,
								int32_t deadband, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t", deadband});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAccumulatorDeadband", parameters);
}

int64_t HAL_GetAccumulatorValue(HAL_AnalogInputHandle analogPortHandle,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int64_t> c;
	callFunc("HAL_GetAccumulatorValue", parameters, c);
	int64_t x;
	c.get(x);
	return x;
}

int64_t HAL_GetAccumulatorCount(HAL_AnalogInputHandle analogPortHandle,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int64_t> c;
	callFunc("HAL_GetAccumulatorCount", parameters, c);
	int64_t x;
	c.get(x);
	return x;
}

void HAL_GetAccumulatorOutput(HAL_AnalogInputHandle analogPortHandle,
							  int64_t* value, int64_t* count, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int64_t*", value});
	parameters.push_back({"int64_t*", count});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_GetAccumulatorOutput", parameters);
}

HAL_GyroHandle HAL_InitializeAnalogGyro(HAL_AnalogInputHandle handle,
										int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_GyroHandle> c;
	callFunc("HAL_InitializeAnalogGyro", parameters, c);
	HAL_GyroHandle x;
	c.get(x);
	return x;
}

void HAL_SetupAnalogGyro(HAL_GyroHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetupAnalogGyro", parameters);
}

void HAL_FreeAnalogGyro(HAL_GyroHandle handle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	callFunc("HAL_FreeAnalogGyro", parameters);
}

void HAL_SetAnalogGyroParameters(HAL_GyroHandle handle,
								 double voltsPerDegreePerSecond, double offset,
								 int32_t center, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"double", voltsPerDegreePerSecond});
	parameters.push_back({"double", offset});
	parameters.push_back({"int32_t", center});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogGyroParameters", parameters);
}

void HAL_SetAnalogGyroVoltsPerDegreePerSecond(HAL_GyroHandle handle,
											  double voltsPerDegreePerSecond,
											  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"double", voltsPerDegreePerSecond});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogGyroVoltsPerDegreePerSecond", parameters);
}

void HAL_ResetAnalogGyro(HAL_GyroHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ResetAnalogGyro", parameters);
}

void HAL_CalibrateAnalogGyro(HAL_GyroHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CalibrateAnalogGyro", parameters);
}

void HAL_SetAnalogGyroDeadband(HAL_GyroHandle handle, double volts,
							   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"double", volts});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogGyroDeadband", parameters);
}

double HAL_GetAnalogGyroAngle(HAL_GyroHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetAnalogGyroAngle", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetAnalogGyroRate(HAL_GyroHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetAnalogGyroRate", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetAnalogGyroOffset(HAL_GyroHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetAnalogGyroOffset", parameters, c);
	double x;
	c.get(x);
	return x;
}

int32_t HAL_GetAnalogGyroCenter(HAL_GyroHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAnalogGyroCenter", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

HAL_AnalogInputHandle HAL_InitializeAnalogInputPort(HAL_PortHandle portHandle,
													int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle", portHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_AnalogInputHandle> c;
	callFunc("HAL_InitializeAnalogInputPort", parameters, c);
	HAL_AnalogInputHandle x;
	c.get(x);
	return x;
}

void HAL_FreeAnalogInputPort(HAL_AnalogInputHandle analogPortHandle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	callFunc("HAL_FreeAnalogInputPort", parameters);
}

HAL_Bool HAL_CheckAnalogModule(int32_t module) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogModule", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_CheckAnalogInputChannel(int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogInputChannel", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_SetAnalogSampleRate(double samplesPerSecond, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double", samplesPerSecond});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogSampleRate", parameters);
}

double HAL_GetAnalogSampleRate(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetAnalogSampleRate", parameters, c);
	double x;
	c.get(x);
	return x;
}

void HAL_SetAnalogAverageBits(HAL_AnalogInputHandle analogPortHandle,
							  int32_t bits, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t", bits});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogAverageBits", parameters);
}

int32_t HAL_GetAnalogAverageBits(HAL_AnalogInputHandle analogPortHandle,
								 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAnalogAverageBits", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_SetAnalogOversampleBits(HAL_AnalogInputHandle analogPortHandle,
								 int32_t bits, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t", bits});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogOversampleBits", parameters);
}

int32_t HAL_GetAnalogOversampleBits(HAL_AnalogInputHandle analogPortHandle,
									int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAnalogOversampleBits", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetAnalogValue(HAL_AnalogInputHandle analogPortHandle,
						   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAnalogValue", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetAnalogAverageValue(HAL_AnalogInputHandle analogPortHandle,
								  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAnalogAverageValue", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetAnalogVoltsToValue(HAL_AnalogInputHandle analogPortHandle,
								  double voltage, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"double", voltage});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAnalogVoltsToValue", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

double HAL_GetAnalogVoltage(HAL_AnalogInputHandle analogPortHandle,
							int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetAnalogVoltage", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetAnalogAverageVoltage(HAL_AnalogInputHandle analogPortHandle,
								   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetAnalogAverageVoltage", parameters, c);
	double x;
	c.get(x);
	return x;
}

int32_t HAL_GetAnalogLSBWeight(HAL_AnalogInputHandle analogPortHandle,
							   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAnalogLSBWeight", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetAnalogOffset(HAL_AnalogInputHandle analogPortHandle,
							int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", analogPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAnalogOffset", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

HAL_AnalogOutputHandle HAL_InitializeAnalogOutputPort(HAL_PortHandle portHandle,
													  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle", portHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_AnalogOutputHandle> c;
	callFunc("HAL_InitializeAnalogOutputPort", parameters, c);
	HAL_AnalogOutputHandle x;
	c.get(x);
	return x;
}

void HAL_FreeAnalogOutputPort(HAL_AnalogOutputHandle analogOutputHandle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle", analogOutputHandle});
	callFunc("HAL_FreeAnalogOutputPort", parameters);
}

void HAL_SetAnalogOutput(HAL_AnalogOutputHandle analogOutputHandle,
						 double voltage, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle", analogOutputHandle});
	parameters.push_back({"double", voltage});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogOutput", parameters);
}

double HAL_GetAnalogOutput(HAL_AnalogOutputHandle analogOutputHandle,
						   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle", analogOutputHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetAnalogOutput", parameters, c);
	double x;
	c.get(x);
	return x;
}

HAL_Bool HAL_CheckAnalogOutputChannel(int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogOutputChannel", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_AnalogTriggerHandle HAL_InitializeAnalogTrigger(
	HAL_AnalogInputHandle portHandle, int32_t* index, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle", portHandle});
	parameters.push_back({"int32_t*", index});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_AnalogTriggerHandle> c;
	callFunc("HAL_InitializeAnalogTrigger", parameters, c);
	HAL_AnalogTriggerHandle x;
	c.get(x);
	return x;
}

void HAL_CleanAnalogTrigger(HAL_AnalogTriggerHandle analogTriggerHandle,
							int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle", analogTriggerHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CleanAnalogTrigger", parameters);
}

void HAL_SetAnalogTriggerLimitsRaw(HAL_AnalogTriggerHandle analogTriggerHandle,
								   int32_t lower, int32_t upper,
								   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle", analogTriggerHandle});
	parameters.push_back({"int32_t", lower});
	parameters.push_back({"int32_t", upper});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogTriggerLimitsRaw", parameters);
}

void HAL_SetAnalogTriggerLimitsVoltage(
	HAL_AnalogTriggerHandle analogTriggerHandle, double lower, double upper,
	int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle", analogTriggerHandle});
	parameters.push_back({"double", lower});
	parameters.push_back({"double", upper});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogTriggerLimitsVoltage", parameters);
}

void HAL_SetAnalogTriggerAveraged(HAL_AnalogTriggerHandle analogTriggerHandle,
								  HAL_Bool useAveragedValue, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle", analogTriggerHandle});
	parameters.push_back({"HAL_Bool", useAveragedValue});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogTriggerAveraged", parameters);
}

void HAL_SetAnalogTriggerFiltered(HAL_AnalogTriggerHandle analogTriggerHandle,
								  HAL_Bool useFilteredValue, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle", analogTriggerHandle});
	parameters.push_back({"HAL_Bool", useFilteredValue});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAnalogTriggerFiltered", parameters);
}

HAL_Bool HAL_GetAnalogTriggerInWindow(
	HAL_AnalogTriggerHandle analogTriggerHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle", analogTriggerHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerInWindow", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetAnalogTriggerTriggerState(
	HAL_AnalogTriggerHandle analogTriggerHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle", analogTriggerHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerTriggerState", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetAnalogTriggerOutput(HAL_AnalogTriggerHandle analogTriggerHandle,
									HAL_AnalogTriggerType type,
									int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle", analogTriggerHandle});
	parameters.push_back({"HAL_AnalogTriggerType", type});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerOutput", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_CAN_SendMessage(uint32_t messageID, const uint8_t* data,
						 uint8_t dataSize, int32_t periodMs, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"uint32_t", messageID});
	parameters.push_back({"const uint8_t*", data});
	parameters.push_back({"uint8_t", dataSize});
	parameters.push_back({"int32_t", periodMs});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CAN_SendMessage", parameters);
}

void HAL_CAN_ReceiveMessage(uint32_t* messageID, uint32_t messageIDMask,
							uint8_t* data, uint8_t* dataSize,
							uint32_t* timeStamp, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"uint32_t*", messageID});
	parameters.push_back({"uint32_t", messageIDMask});
	parameters.push_back({"uint8_t*", data});
	parameters.push_back({"uint8_t*", dataSize});
	parameters.push_back({"uint32_t*", timeStamp});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CAN_ReceiveMessage", parameters);
}

void HAL_CAN_OpenStreamSession(uint32_t* sessionHandle, uint32_t messageID,
							   uint32_t messageIDMask, uint32_t maxMessages,
							   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"uint32_t*", sessionHandle});
	parameters.push_back({"uint32_t", messageID});
	parameters.push_back({"uint32_t", messageIDMask});
	parameters.push_back({"uint32_t", maxMessages});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CAN_OpenStreamSession", parameters);
}

void HAL_CAN_CloseStreamSession(uint32_t sessionHandle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"uint32_t", sessionHandle});
	callFunc("HAL_CAN_CloseStreamSession", parameters);
}

void HAL_CAN_ReadStreamSession(uint32_t sessionHandle,
							   struct HAL_CANStreamMessage* messages,
							   uint32_t messagesToRead, uint32_t* messagesRead,
							   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"uint32_t", sessionHandle});
	parameters.push_back({"struct HAL_CANStreamMessage*", messages});
	parameters.push_back({"uint32_t", messagesToRead});
	parameters.push_back({"uint32_t*", messagesRead});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CAN_ReadStreamSession", parameters);
}

void HAL_CAN_GetCANStatus(float* percentBusUtilization, uint32_t* busOffCount,
						  uint32_t* txFullCount, uint32_t* receiveErrorCount,
						  uint32_t* transmitErrorCount, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"float*", percentBusUtilization});
	parameters.push_back({"uint32_t*", busOffCount});
	parameters.push_back({"uint32_t*", txFullCount});
	parameters.push_back({"uint32_t*", receiveErrorCount});
	parameters.push_back({"uint32_t*", transmitErrorCount});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CAN_GetCANStatus", parameters);
}

HAL_CANHandle HAL_InitializeCAN(HAL_CANManufacturer manufacturer,
								int32_t deviceId, HAL_CANDeviceType deviceType,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANManufacturer", manufacturer});
	parameters.push_back({"int32_t", deviceId});
	parameters.push_back({"HAL_CANDeviceType", deviceType});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_CANHandle> c;
	callFunc("HAL_InitializeCAN", parameters, c);
	HAL_CANHandle x;
	c.get(x);
	return x;
}

void HAL_CleanCAN(HAL_CANHandle handle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANHandle", handle});
	callFunc("HAL_CleanCAN", parameters);
}

void HAL_WriteCANPacket(HAL_CANHandle handle, const uint8_t* data,
						int32_t length, int32_t apiId, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANHandle", handle});
	parameters.push_back({"const uint8_t*", data});
	parameters.push_back({"int32_t", length});
	parameters.push_back({"int32_t", apiId});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_WriteCANPacket", parameters);
}

void HAL_WriteCANPacketRepeating(HAL_CANHandle handle, const uint8_t* data,
								 int32_t length, int32_t apiId,
								 int32_t repeatMs, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANHandle", handle});
	parameters.push_back({"const uint8_t*", data});
	parameters.push_back({"int32_t", length});
	parameters.push_back({"int32_t", apiId});
	parameters.push_back({"int32_t", repeatMs});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_WriteCANPacketRepeating", parameters);
}

void HAL_StopCANPacketRepeating(HAL_CANHandle handle, int32_t apiId,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANHandle", handle});
	parameters.push_back({"int32_t", apiId});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_StopCANPacketRepeating", parameters);
}

void HAL_ReadCANPacketNew(HAL_CANHandle handle, int32_t apiId, uint8_t* data,
						  int32_t* length, uint64_t* receivedTimestamp,
						  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANHandle", handle});
	parameters.push_back({"int32_t", apiId});
	parameters.push_back({"uint8_t*", data});
	parameters.push_back({"int32_t*", length});
	parameters.push_back({"uint64_t*", receivedTimestamp});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ReadCANPacketNew", parameters);
}

void HAL_ReadCANPacketLatest(HAL_CANHandle handle, int32_t apiId, uint8_t* data,
							 int32_t* length, uint64_t* receivedTimestamp,
							 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANHandle", handle});
	parameters.push_back({"int32_t", apiId});
	parameters.push_back({"uint8_t*", data});
	parameters.push_back({"int32_t*", length});
	parameters.push_back({"uint64_t*", receivedTimestamp});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ReadCANPacketLatest", parameters);
}

void HAL_ReadCANPacketTimeout(HAL_CANHandle handle, int32_t apiId,
							  uint8_t* data, int32_t* length,
							  uint64_t* receivedTimestamp, int32_t timeoutMs,
							  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANHandle", handle});
	parameters.push_back({"int32_t", apiId});
	parameters.push_back({"uint8_t*", data});
	parameters.push_back({"int32_t*", length});
	parameters.push_back({"uint64_t*", receivedTimestamp});
	parameters.push_back({"int32_t", timeoutMs});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ReadCANPacketTimeout", parameters);
}

void HAL_ReadCANPeriodicPacket(HAL_CANHandle handle, int32_t apiId,
							   uint8_t* data, int32_t* length,
							   uint64_t* receivedTimestamp, int32_t timeoutMs,
							   int32_t periodMs, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CANHandle", handle});
	parameters.push_back({"int32_t", apiId});
	parameters.push_back({"uint8_t*", data});
	parameters.push_back({"int32_t*", length});
	parameters.push_back({"uint64_t*", receivedTimestamp});
	parameters.push_back({"int32_t", timeoutMs});
	parameters.push_back({"int32_t", periodMs});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ReadCANPeriodicPacket", parameters);
}

HAL_CompressorHandle HAL_InitializeCompressor(int32_t module, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_CompressorHandle> c;
	callFunc("HAL_InitializeCompressor", parameters, c);
	HAL_CompressorHandle x;
	c.get(x);
	return x;
}

HAL_Bool HAL_CheckCompressorModule(int32_t module) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckCompressorModule", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCompressor(HAL_CompressorHandle compressorHandle,
						   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressor", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_SetCompressorClosedLoopControl(HAL_CompressorHandle compressorHandle,
										HAL_Bool value, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"HAL_Bool", value});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCompressorClosedLoopControl", parameters);
}

HAL_Bool HAL_GetCompressorClosedLoopControl(
	HAL_CompressorHandle compressorHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorClosedLoopControl", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCompressorPressureSwitch(HAL_CompressorHandle compressorHandle,
										 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorPressureSwitch", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

double HAL_GetCompressorCurrent(HAL_CompressorHandle compressorHandle,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetCompressorCurrent", parameters, c);
	double x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCompressorCurrentTooHighFault(
	HAL_CompressorHandle compressorHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorCurrentTooHighFault", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCompressorCurrentTooHighStickyFault(
	HAL_CompressorHandle compressorHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorCurrentTooHighStickyFault", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCompressorShortedStickyFault(
	HAL_CompressorHandle compressorHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorShortedStickyFault", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCompressorShortedFault(HAL_CompressorHandle compressorHandle,
									   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorShortedFault", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCompressorNotConnectedStickyFault(
	HAL_CompressorHandle compressorHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorNotConnectedStickyFault", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCompressorNotConnectedFault(
	HAL_CompressorHandle compressorHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle", compressorHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorNotConnectedFault", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

int32_t HAL_GetSystemClockTicksPerMicrosecond() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetSystemClockTicksPerMicrosecond", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

HAL_CounterHandle HAL_InitializeCounter(HAL_Counter_Mode mode, int32_t* index,
										int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Counter_Mode", mode});
	parameters.push_back({"int32_t*", index});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_CounterHandle> c;
	callFunc("HAL_InitializeCounter", parameters, c);
	HAL_CounterHandle x;
	c.get(x);
	return x;
}

void HAL_FreeCounter(HAL_CounterHandle counterHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_FreeCounter", parameters);
}

void HAL_SetCounterAverageSize(HAL_CounterHandle counterHandle, int32_t size,
							   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t", size});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterAverageSize", parameters);
}

void HAL_SetCounterUpSource(HAL_CounterHandle counterHandle,
							HAL_Handle digitalSourceHandle,
							HAL_AnalogTriggerType analogTriggerType,
							int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"HAL_Handle", digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType", analogTriggerType});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterUpSource", parameters);
}

void HAL_SetCounterUpSourceEdge(HAL_CounterHandle counterHandle,
								HAL_Bool risingEdge, HAL_Bool fallingEdge,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"HAL_Bool", risingEdge});
	parameters.push_back({"HAL_Bool", fallingEdge});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterUpSourceEdge", parameters);
}

void HAL_ClearCounterUpSource(HAL_CounterHandle counterHandle,
							  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ClearCounterUpSource", parameters);
}

void HAL_SetCounterDownSource(HAL_CounterHandle counterHandle,
							  HAL_Handle digitalSourceHandle,
							  HAL_AnalogTriggerType analogTriggerType,
							  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"HAL_Handle", digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType", analogTriggerType});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterDownSource", parameters);
}

void HAL_SetCounterDownSourceEdge(HAL_CounterHandle counterHandle,
								  HAL_Bool risingEdge, HAL_Bool fallingEdge,
								  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"HAL_Bool", risingEdge});
	parameters.push_back({"HAL_Bool", fallingEdge});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterDownSourceEdge", parameters);
}

void HAL_ClearCounterDownSource(HAL_CounterHandle counterHandle,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ClearCounterDownSource", parameters);
}

void HAL_SetCounterUpDownMode(HAL_CounterHandle counterHandle,
							  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterUpDownMode", parameters);
}

void HAL_SetCounterExternalDirectionMode(HAL_CounterHandle counterHandle,
										 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterExternalDirectionMode", parameters);
}

void HAL_SetCounterSemiPeriodMode(HAL_CounterHandle counterHandle,
								  HAL_Bool highSemiPeriod, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"HAL_Bool", highSemiPeriod});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterSemiPeriodMode", parameters);
}

void HAL_SetCounterPulseLengthMode(HAL_CounterHandle counterHandle,
								   double threshold, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"double", threshold});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterPulseLengthMode", parameters);
}

int32_t HAL_GetCounterSamplesToAverage(HAL_CounterHandle counterHandle,
									   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetCounterSamplesToAverage", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_SetCounterSamplesToAverage(HAL_CounterHandle counterHandle,
									int32_t samplesToAverage, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t", samplesToAverage});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterSamplesToAverage", parameters);
}

void HAL_ResetCounter(HAL_CounterHandle counterHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ResetCounter", parameters);
}

int32_t HAL_GetCounter(HAL_CounterHandle counterHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetCounter", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

double HAL_GetCounterPeriod(HAL_CounterHandle counterHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetCounterPeriod", parameters, c);
	double x;
	c.get(x);
	return x;
}

void HAL_SetCounterMaxPeriod(HAL_CounterHandle counterHandle, double maxPeriod,
							 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"double", maxPeriod});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterMaxPeriod", parameters);
}

void HAL_SetCounterUpdateWhenEmpty(HAL_CounterHandle counterHandle,
								   HAL_Bool enabled, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"HAL_Bool", enabled});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterUpdateWhenEmpty", parameters);
}

HAL_Bool HAL_GetCounterStopped(HAL_CounterHandle counterHandle,
							   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCounterStopped", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetCounterDirection(HAL_CounterHandle counterHandle,
								 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetCounterDirection", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_SetCounterReverseDirection(HAL_CounterHandle counterHandle,
									HAL_Bool reverseDirection,
									int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle", counterHandle});
	parameters.push_back({"HAL_Bool", reverseDirection});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetCounterReverseDirection", parameters);
}

HAL_DigitalHandle HAL_InitializeDIOPort(HAL_PortHandle portHandle,
										HAL_Bool input, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle", portHandle});
	parameters.push_back({"HAL_Bool", input});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_DigitalHandle> c;
	callFunc("HAL_InitializeDIOPort", parameters, c);
	HAL_DigitalHandle x;
	c.get(x);
	return x;
}

HAL_Bool HAL_CheckDIOChannel(int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckDIOChannel", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_FreeDIOPort(HAL_DigitalHandle dioPortHandle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	callFunc("HAL_FreeDIOPort", parameters);
}

HAL_DigitalPWMHandle HAL_AllocateDigitalPWM(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_DigitalPWMHandle> c;
	callFunc("HAL_AllocateDigitalPWM", parameters, c);
	HAL_DigitalPWMHandle x;
	c.get(x);
	return x;
}

void HAL_FreeDigitalPWM(HAL_DigitalPWMHandle pwmGenerator, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle", pwmGenerator});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_FreeDigitalPWM", parameters);
}

void HAL_SetDigitalPWMRate(double rate, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double", rate});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetDigitalPWMRate", parameters);
}

void HAL_SetDigitalPWMDutyCycle(HAL_DigitalPWMHandle pwmGenerator,
								double dutyCycle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle", pwmGenerator});
	parameters.push_back({"double", dutyCycle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetDigitalPWMDutyCycle", parameters);
}

void HAL_SetDigitalPWMOutputChannel(HAL_DigitalPWMHandle pwmGenerator,
									int32_t channel, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle", pwmGenerator});
	parameters.push_back({"int32_t", channel});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetDigitalPWMOutputChannel", parameters);
}

void HAL_SetDIO(HAL_DigitalHandle dioPortHandle, HAL_Bool value,
				int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	parameters.push_back({"HAL_Bool", value});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetDIO", parameters);
}

void HAL_SetDIODirection(HAL_DigitalHandle dioPortHandle, HAL_Bool input,
						 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	parameters.push_back({"HAL_Bool", input});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetDIODirection", parameters);
}

HAL_Bool HAL_GetDIO(HAL_DigitalHandle dioPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetDIO", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetDIODirection(HAL_DigitalHandle dioPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetDIODirection", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_Pulse(HAL_DigitalHandle dioPortHandle, double pulseLength,
			   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	parameters.push_back({"double", pulseLength});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_Pulse", parameters);
}

HAL_Bool HAL_IsPulsing(HAL_DigitalHandle dioPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_IsPulsing", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_IsAnyPulsing(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_IsAnyPulsing", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_SetFilterSelect(HAL_DigitalHandle dioPortHandle, int32_t filterIndex,
						 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	parameters.push_back({"int32_t", filterIndex});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetFilterSelect", parameters);
}

int32_t HAL_GetFilterSelect(HAL_DigitalHandle dioPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", dioPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetFilterSelect", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_SetFilterPeriod(int32_t filterIndex, int64_t value, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", filterIndex});
	parameters.push_back({"int64_t", value});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetFilterPeriod", parameters);
}

int64_t HAL_GetFilterPeriod(int32_t filterIndex, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", filterIndex});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int64_t> c;
	callFunc("HAL_GetFilterPeriod", parameters, c);
	int64_t x;
	c.get(x);
	return x;
}

int32_t HAL_SendError(HAL_Bool isError, int32_t errorCode, HAL_Bool isLVCode,
					  const char* details, const char* location,
					  const char* callStack, HAL_Bool printMsg) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool", isError});
	parameters.push_back({"int32_t", errorCode});
	parameters.push_back({"HAL_Bool", isLVCode});
	parameters.push_back({"const char*", details});
	parameters.push_back({"const char*", location});
	parameters.push_back({"const char*", callStack});
	parameters.push_back({"HAL_Bool", printMsg});
	minerva::Channel<int32_t> c;
	callFunc("HAL_SendError", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetControlWord(HAL_ControlWord* controlWord) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_ControlWord*", controlWord});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetControlWord", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

HAL_AllianceStationID HAL_GetAllianceStation(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_AllianceStationID> c;
	callFunc("HAL_GetAllianceStation", parameters, c);
	HAL_AllianceStationID x;
	c.get(x);
	return x;
}

int32_t HAL_GetJoystickAxes(int32_t joystickNum, HAL_JoystickAxes* axes) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	parameters.push_back({"HAL_JoystickAxes*", axes});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetJoystickAxes", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetJoystickPOVs(int32_t joystickNum, HAL_JoystickPOVs* povs) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	parameters.push_back({"HAL_JoystickPOVs*", povs});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetJoystickPOVs", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetJoystickButtons(int32_t joystickNum,
							   HAL_JoystickButtons* buttons) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	parameters.push_back({"HAL_JoystickButtons*", buttons});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetJoystickButtons", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetJoystickDescriptor(int32_t joystickNum,
								  HAL_JoystickDescriptor* desc) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	parameters.push_back({"HAL_JoystickDescriptor*", desc});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetJoystickDescriptor", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetJoystickIsXbox(int32_t joystickNum) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetJoystickIsXbox", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

int32_t HAL_GetJoystickType(int32_t joystickNum) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetJoystickType", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

char* HAL_GetJoystickName(int32_t joystickNum) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	minerva::Channel<char*> c;
	callFunc("HAL_GetJoystickName", parameters, c);
	char* x;
	c.get(x);
	return x;
}

void HAL_FreeJoystickName(char* name) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"char*", name});
	callFunc("HAL_FreeJoystickName", parameters);
}

int32_t HAL_GetJoystickAxisType(int32_t joystickNum, int32_t axis) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	parameters.push_back({"int32_t", axis});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetJoystickAxisType", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_SetJoystickOutputs(int32_t joystickNum, int64_t outputs,
							   int32_t leftRumble, int32_t rightRumble) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", joystickNum});
	parameters.push_back({"int64_t", outputs});
	parameters.push_back({"int32_t", leftRumble});
	parameters.push_back({"int32_t", rightRumble});
	minerva::Channel<int32_t> c;
	callFunc("HAL_SetJoystickOutputs", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

double HAL_GetMatchTime(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetMatchTime", parameters, c);
	double x;
	c.get(x);
	return x;
}

int HAL_GetMatchInfo(HAL_MatchInfo* info) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_MatchInfo*", info});
	minerva::Channel<int> c;
	callFunc("HAL_GetMatchInfo", parameters, c);
	int x;
	c.get(x);
	return x;
}

void HAL_FreeMatchInfo(HAL_MatchInfo* info) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_MatchInfo*", info});
	callFunc("HAL_FreeMatchInfo", parameters);
}

void HAL_ReleaseDSMutex() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ReleaseDSMutex", parameters);
}

bool HAL_IsNewControlData() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<bool> c;
	callFunc("HAL_IsNewControlData", parameters, c);
	bool x;
	c.get(x);
	return x;
}

void HAL_WaitForDSData() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_WaitForDSData", parameters);
}

HAL_Bool HAL_WaitForDSDataTimeout(double timeout) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double", timeout});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_WaitForDSDataTimeout", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_InitializeDriverStation() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_InitializeDriverStation", parameters);
}

void HAL_ObserveUserProgramStarting() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramStarting", parameters);
}

void HAL_ObserveUserProgramDisabled() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramDisabled", parameters);
}

void HAL_ObserveUserProgramAutonomous() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramAutonomous", parameters);
}

void HAL_ObserveUserProgramTeleop() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramTeleop", parameters);
}

void HAL_ObserveUserProgramTest() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramTest", parameters);
}

HAL_EncoderHandle HAL_InitializeEncoder(
	HAL_Handle digitalSourceHandleA, HAL_AnalogTriggerType analogTriggerTypeA,
	HAL_Handle digitalSourceHandleB, HAL_AnalogTriggerType analogTriggerTypeB,
	HAL_Bool reverseDirection, HAL_EncoderEncodingType encodingType,
	int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Handle", digitalSourceHandleA});
	parameters.push_back({"HAL_AnalogTriggerType", analogTriggerTypeA});
	parameters.push_back({"HAL_Handle", digitalSourceHandleB});
	parameters.push_back({"HAL_AnalogTriggerType", analogTriggerTypeB});
	parameters.push_back({"HAL_Bool", reverseDirection});
	parameters.push_back({"HAL_EncoderEncodingType", encodingType});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_EncoderHandle> c;
	callFunc("HAL_InitializeEncoder", parameters, c);
	HAL_EncoderHandle x;
	c.get(x);
	return x;
}

void HAL_FreeEncoder(HAL_EncoderHandle encoderHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_FreeEncoder", parameters);
}

int32_t HAL_GetEncoder(HAL_EncoderHandle encoderHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetEncoder", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetEncoderRaw(HAL_EncoderHandle encoderHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetEncoderRaw", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetEncoderEncodingScale(HAL_EncoderHandle encoderHandle,
									int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetEncoderEncodingScale", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_ResetEncoder(HAL_EncoderHandle encoderHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ResetEncoder", parameters);
}

double HAL_GetEncoderPeriod(HAL_EncoderHandle encoderHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetEncoderPeriod", parameters, c);
	double x;
	c.get(x);
	return x;
}

void HAL_SetEncoderMaxPeriod(HAL_EncoderHandle encoderHandle, double maxPeriod,
							 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"double", maxPeriod});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetEncoderMaxPeriod", parameters);
}

HAL_Bool HAL_GetEncoderStopped(HAL_EncoderHandle encoderHandle,
							   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetEncoderStopped", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetEncoderDirection(HAL_EncoderHandle encoderHandle,
								 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetEncoderDirection", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

double HAL_GetEncoderDistance(HAL_EncoderHandle encoderHandle,
							  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetEncoderDistance", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetEncoderRate(HAL_EncoderHandle encoderHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetEncoderRate", parameters, c);
	double x;
	c.get(x);
	return x;
}

void HAL_SetEncoderMinRate(HAL_EncoderHandle encoderHandle, double minRate,
						   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"double", minRate});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetEncoderMinRate", parameters);
}

void HAL_SetEncoderDistancePerPulse(HAL_EncoderHandle encoderHandle,
									double distancePerPulse, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"double", distancePerPulse});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetEncoderDistancePerPulse", parameters);
}

void HAL_SetEncoderReverseDirection(HAL_EncoderHandle encoderHandle,
									HAL_Bool reverseDirection,
									int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"HAL_Bool", reverseDirection});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetEncoderReverseDirection", parameters);
}

void HAL_SetEncoderSamplesToAverage(HAL_EncoderHandle encoderHandle,
									int32_t samplesToAverage, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t", samplesToAverage});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetEncoderSamplesToAverage", parameters);
}

int32_t HAL_GetEncoderSamplesToAverage(HAL_EncoderHandle encoderHandle,
									   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetEncoderSamplesToAverage", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_SetEncoderIndexSource(HAL_EncoderHandle encoderHandle,
							   HAL_Handle digitalSourceHandle,
							   HAL_AnalogTriggerType analogTriggerType,
							   HAL_EncoderIndexingType type, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"HAL_Handle", digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType", analogTriggerType});
	parameters.push_back({"HAL_EncoderIndexingType", type});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetEncoderIndexSource", parameters);
}

int32_t HAL_GetEncoderFPGAIndex(HAL_EncoderHandle encoderHandle,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetEncoderFPGAIndex", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

double HAL_GetEncoderDecodingScaleFactor(HAL_EncoderHandle encoderHandle,
										 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetEncoderDecodingScaleFactor", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetEncoderDistancePerPulse(HAL_EncoderHandle encoderHandle,
									  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetEncoderDistancePerPulse", parameters, c);
	double x;
	c.get(x);
	return x;
}

HAL_EncoderEncodingType HAL_GetEncoderEncodingType(
	HAL_EncoderHandle encoderHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle", encoderHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_EncoderEncodingType> c;
	callFunc("HAL_GetEncoderEncodingType", parameters, c);
	HAL_EncoderEncodingType x;
	c.get(x);
	return x;
}

int HAL_LoadOneExtension(const char* library) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"const char*", library});
	minerva::Channel<int> c;
	callFunc("HAL_LoadOneExtension", parameters, c);
	int x;
	c.get(x);
	return x;
}

int HAL_LoadExtensions() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int> c;
	callFunc("HAL_LoadExtensions", parameters, c);
	int x;
	c.get(x);
	return x;
}

const char* HAL_GetErrorMessage(int32_t code) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", code});
	minerva::Channel<const char*> c;
	callFunc("HAL_GetErrorMessage", parameters, c);
	const char* x;
	c.get(x);
	return x;
}

int32_t HAL_GetFPGAVersion(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetFPGAVersion", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int64_t HAL_GetFPGARevision(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int64_t> c;
	callFunc("HAL_GetFPGARevision", parameters, c);
	int64_t x;
	c.get(x);
	return x;
}

HAL_RuntimeType HAL_GetRuntimeType() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<HAL_RuntimeType> c;
	callFunc("HAL_GetRuntimeType", parameters, c);
	HAL_RuntimeType x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetFPGAButton(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetFPGAButton", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetSystemActive(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetSystemActive", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetBrownedOut(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetBrownedOut", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_BaseInitialize(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_BaseInitialize", parameters);
}

HAL_PortHandle HAL_GetPort(int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_PortHandle> c;
	callFunc("HAL_GetPort", parameters, c);
	HAL_PortHandle x;
	c.get(x);
	return x;
}

HAL_PortHandle HAL_GetPortWithModule(int32_t module, int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_PortHandle> c;
	callFunc("HAL_GetPortWithModule", parameters, c);
	HAL_PortHandle x;
	c.get(x);
	return x;
}

uint64_t HAL_GetFPGATime(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<uint64_t> c;
	callFunc("HAL_GetFPGATime", parameters, c);
	uint64_t x;
	c.get(x);
	return x;
}

HAL_Bool HAL_Initialize(int32_t timeout, int32_t mode) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", timeout});
	parameters.push_back({"int32_t", mode});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_Initialize", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

int64_t HAL_Report(int32_t resource, int32_t instanceNumber, int32_t context,
				   const char* feature) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", resource});
	parameters.push_back({"int32_t", instanceNumber});
	parameters.push_back({"int32_t", context});
	parameters.push_back({"const char*", feature});
	minerva::Channel<int64_t> c;
	callFunc("HAL_Report", parameters, c);
	int64_t x;
	c.get(x);
	return x;
}

void HAL_InitializeI2C(HAL_I2CPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_InitializeI2C", parameters);
}

int32_t HAL_TransactionI2C(HAL_I2CPort port, int32_t deviceAddress,
						   const uint8_t* dataToSend, int32_t sendSize,
						   uint8_t* dataReceived, int32_t receiveSize) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort", port});
	parameters.push_back({"int32_t", deviceAddress});
	parameters.push_back({"const uint8_t*", dataToSend});
	parameters.push_back({"int32_t", sendSize});
	parameters.push_back({"uint8_t*", dataReceived});
	parameters.push_back({"int32_t", receiveSize});
	minerva::Channel<int32_t> c;
	callFunc("HAL_TransactionI2C", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_WriteI2C(HAL_I2CPort port, int32_t deviceAddress,
					 const uint8_t* dataToSend, int32_t sendSize) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort", port});
	parameters.push_back({"int32_t", deviceAddress});
	parameters.push_back({"const uint8_t*", dataToSend});
	parameters.push_back({"int32_t", sendSize});
	minerva::Channel<int32_t> c;
	callFunc("HAL_WriteI2C", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_ReadI2C(HAL_I2CPort port, int32_t deviceAddress, uint8_t* buffer,
					int32_t count) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort", port});
	parameters.push_back({"int32_t", deviceAddress});
	parameters.push_back({"uint8_t*", buffer});
	parameters.push_back({"int32_t", count});
	minerva::Channel<int32_t> c;
	callFunc("HAL_ReadI2C", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_CloseI2C(HAL_I2CPort port) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort", port});
	callFunc("HAL_CloseI2C", parameters);
}

HAL_InterruptHandle HAL_InitializeInterrupts(HAL_Bool watcher,
											 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool", watcher});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_InterruptHandle> c;
	callFunc("HAL_InitializeInterrupts", parameters, c);
	HAL_InterruptHandle x;
	c.get(x);
	return x;
}

int64_t HAL_WaitForInterrupt(HAL_InterruptHandle interruptHandle,
							 double timeout, HAL_Bool ignorePrevious,
							 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"double", timeout});
	parameters.push_back({"HAL_Bool", ignorePrevious});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int64_t> c;
	callFunc("HAL_WaitForInterrupt", parameters, c);
	int64_t x;
	c.get(x);
	return x;
}

void HAL_EnableInterrupts(HAL_InterruptHandle interruptHandle,
						  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_EnableInterrupts", parameters);
}

void HAL_DisableInterrupts(HAL_InterruptHandle interruptHandle,
						   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_DisableInterrupts", parameters);
}

double HAL_ReadInterruptRisingTimestamp(HAL_InterruptHandle interruptHandle,
										int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_ReadInterruptRisingTimestamp", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_ReadInterruptFallingTimestamp(HAL_InterruptHandle interruptHandle,
										 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_ReadInterruptFallingTimestamp", parameters, c);
	double x;
	c.get(x);
	return x;
}

void HAL_RequestInterrupts(HAL_InterruptHandle interruptHandle,
						   HAL_Handle digitalSourceHandle,
						   HAL_AnalogTriggerType analogTriggerType,
						   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"HAL_Handle", digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType", analogTriggerType});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_RequestInterrupts", parameters);
}

void HAL_AttachInterruptHandler(HAL_InterruptHandle interruptHandle,
								HAL_InterruptHandlerFunction handler,
								void* param, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"HAL_InterruptHandlerFunction", handler});
	parameters.push_back({"void*", param});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_AttachInterruptHandler", parameters);
}

void HAL_AttachInterruptHandlerThreaded(HAL_InterruptHandle interruptHandle,
										HAL_InterruptHandlerFunction handler,
										void* param, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"HAL_InterruptHandlerFunction", handler});
	parameters.push_back({"void*", param});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_AttachInterruptHandlerThreaded", parameters);
}

void HAL_SetInterruptUpSourceEdge(HAL_InterruptHandle interruptHandle,
								  HAL_Bool risingEdge, HAL_Bool fallingEdge,
								  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle", interruptHandle});
	parameters.push_back({"HAL_Bool", risingEdge});
	parameters.push_back({"HAL_Bool", fallingEdge});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetInterruptUpSourceEdge", parameters);
}

HAL_NotifierHandle HAL_InitializeNotifier(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_NotifierHandle> c;
	callFunc("HAL_InitializeNotifier", parameters, c);
	HAL_NotifierHandle x;
	c.get(x);
	return x;
}

void HAL_StopNotifier(HAL_NotifierHandle notifierHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle", notifierHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_StopNotifier", parameters);
}

void HAL_CleanNotifier(HAL_NotifierHandle notifierHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle", notifierHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CleanNotifier", parameters);
}

void HAL_UpdateNotifierAlarm(HAL_NotifierHandle notifierHandle,
							 uint64_t triggerTime, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle", notifierHandle});
	parameters.push_back({"uint64_t", triggerTime});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_UpdateNotifierAlarm", parameters);
}

void HAL_CancelNotifierAlarm(HAL_NotifierHandle notifierHandle,
							 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle", notifierHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CancelNotifierAlarm", parameters);
}

uint64_t HAL_WaitForNotifierAlarm(HAL_NotifierHandle notifierHandle,
								  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle", notifierHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<uint64_t> c;
	callFunc("HAL_WaitForNotifierAlarm", parameters, c);
	uint64_t x;
	c.get(x);
	return x;
}

HAL_PDPHandle HAL_InitializePDP(int32_t module, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_PDPHandle> c;
	callFunc("HAL_InitializePDP", parameters, c);
	HAL_PDPHandle x;
	c.get(x);
	return x;
}

void HAL_CleanPDP(HAL_PDPHandle handle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	callFunc("HAL_CleanPDP", parameters);
}

HAL_Bool HAL_CheckPDPChannel(int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckPDPChannel", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_CheckPDPModule(int32_t module) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckPDPModule", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

double HAL_GetPDPTemperature(HAL_PDPHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetPDPTemperature", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetPDPVoltage(HAL_PDPHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetPDPVoltage", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetPDPChannelCurrent(HAL_PDPHandle handle, int32_t channel,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	parameters.push_back({"int32_t", channel});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetPDPChannelCurrent", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetPDPTotalCurrent(HAL_PDPHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetPDPTotalCurrent", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetPDPTotalPower(HAL_PDPHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetPDPTotalPower", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetPDPTotalEnergy(HAL_PDPHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetPDPTotalEnergy", parameters, c);
	double x;
	c.get(x);
	return x;
}

void HAL_ResetPDPTotalEnergy(HAL_PDPHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ResetPDPTotalEnergy", parameters);
}

void HAL_ClearPDPStickyFaults(HAL_PDPHandle handle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PDPHandle", handle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ClearPDPStickyFaults", parameters);
}

int32_t HAL_GetNumAccumulators() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumAccumulators", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumAnalogTriggers() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogTriggers", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumAnalogInputs() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogInputs", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumAnalogOutputs() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogOutputs", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumCounters() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumCounters", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumDigitalHeaders() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalHeaders", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumPWMHeaders() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumPWMHeaders", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumDigitalChannels() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalChannels", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumPWMChannels() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumPWMChannels", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumDigitalPWMOutputs() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalPWMOutputs", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumEncoders() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumEncoders", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumInterrupts() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumInterrupts", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumRelayChannels() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumRelayChannels", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumRelayHeaders() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumRelayHeaders", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumPCMModules() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumPCMModules", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumSolenoidChannels() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumSolenoidChannels", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumPDPModules() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumPDPModules", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetNumPDPChannels() {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetNumPDPChannels", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

double HAL_GetVinVoltage(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetVinVoltage", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetVinCurrent(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetVinCurrent", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetUserVoltage6V(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetUserVoltage6V", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetUserCurrent6V(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetUserCurrent6V", parameters, c);
	double x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetUserActive6V(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive6V", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

int32_t HAL_GetUserCurrentFaults6V(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults6V", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

double HAL_GetUserVoltage5V(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetUserVoltage5V", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetUserCurrent5V(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetUserCurrent5V", parameters, c);
	double x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetUserActive5V(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive5V", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

int32_t HAL_GetUserCurrentFaults5V(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults5V", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

double HAL_GetUserVoltage3V3(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetUserVoltage3V3", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetUserCurrent3V3(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetUserCurrent3V3", parameters, c);
	double x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetUserActive3V3(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive3V3", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

int32_t HAL_GetUserCurrentFaults3V3(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults3V3", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

HAL_DigitalHandle HAL_InitializePWMPort(HAL_PortHandle portHandle,
										int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle", portHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_DigitalHandle> c;
	callFunc("HAL_InitializePWMPort", parameters, c);
	HAL_DigitalHandle x;
	c.get(x);
	return x;
}

void HAL_FreePWMPort(HAL_DigitalHandle pwmPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_FreePWMPort", parameters);
}

HAL_Bool HAL_CheckPWMChannel(int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckPWMChannel", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_SetPWMConfig(HAL_DigitalHandle pwmPortHandle, double maxPwm,
					  double deadbandMaxPwm, double centerPwm,
					  double deadbandMinPwm, double minPwm, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"double", maxPwm});
	parameters.push_back({"double", deadbandMaxPwm});
	parameters.push_back({"double", centerPwm});
	parameters.push_back({"double", deadbandMinPwm});
	parameters.push_back({"double", minPwm});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetPWMConfig", parameters);
}

void HAL_SetPWMConfigRaw(HAL_DigitalHandle pwmPortHandle, int32_t maxPwm,
						 int32_t deadbandMaxPwm, int32_t centerPwm,
						 int32_t deadbandMinPwm, int32_t minPwm,
						 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t", maxPwm});
	parameters.push_back({"int32_t", deadbandMaxPwm});
	parameters.push_back({"int32_t", centerPwm});
	parameters.push_back({"int32_t", deadbandMinPwm});
	parameters.push_back({"int32_t", minPwm});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetPWMConfigRaw", parameters);
}

void HAL_GetPWMConfigRaw(HAL_DigitalHandle pwmPortHandle, int32_t* maxPwm,
						 int32_t* deadbandMaxPwm, int32_t* centerPwm,
						 int32_t* deadbandMinPwm, int32_t* minPwm,
						 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t*", maxPwm});
	parameters.push_back({"int32_t*", deadbandMaxPwm});
	parameters.push_back({"int32_t*", centerPwm});
	parameters.push_back({"int32_t*", deadbandMinPwm});
	parameters.push_back({"int32_t*", minPwm});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_GetPWMConfigRaw", parameters);
}

void HAL_SetPWMEliminateDeadband(HAL_DigitalHandle pwmPortHandle,
								 HAL_Bool eliminateDeadband, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"HAL_Bool", eliminateDeadband});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetPWMEliminateDeadband", parameters);
}

HAL_Bool HAL_GetPWMEliminateDeadband(HAL_DigitalHandle pwmPortHandle,
									 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetPWMEliminateDeadband", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_SetPWMRaw(HAL_DigitalHandle pwmPortHandle, int32_t value,
				   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t", value});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetPWMRaw", parameters);
}

void HAL_SetPWMSpeed(HAL_DigitalHandle pwmPortHandle, double speed,
					 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"double", speed});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetPWMSpeed", parameters);
}

void HAL_SetPWMPosition(HAL_DigitalHandle pwmPortHandle, double position,
						int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"double", position});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetPWMPosition", parameters);
}

void HAL_SetPWMDisabled(HAL_DigitalHandle pwmPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetPWMDisabled", parameters);
}

int32_t HAL_GetPWMRaw(HAL_DigitalHandle pwmPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetPWMRaw", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

double HAL_GetPWMSpeed(HAL_DigitalHandle pwmPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetPWMSpeed", parameters, c);
	double x;
	c.get(x);
	return x;
}

double HAL_GetPWMPosition(HAL_DigitalHandle pwmPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<double> c;
	callFunc("HAL_GetPWMPosition", parameters, c);
	double x;
	c.get(x);
	return x;
}

void HAL_LatchPWMZero(HAL_DigitalHandle pwmPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_LatchPWMZero", parameters);
}

void HAL_SetPWMPeriodScale(HAL_DigitalHandle pwmPortHandle, int32_t squelchMask,
						   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle", pwmPortHandle});
	parameters.push_back({"int32_t", squelchMask});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetPWMPeriodScale", parameters);
}

int32_t HAL_GetPWMLoopTiming(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetPWMLoopTiming", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

uint64_t HAL_GetPWMCycleStartTime(int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*", status});
	minerva::Channel<uint64_t> c;
	callFunc("HAL_GetPWMCycleStartTime", parameters, c);
	uint64_t x;
	c.get(x);
	return x;
}

HAL_RelayHandle HAL_InitializeRelayPort(HAL_PortHandle portHandle, HAL_Bool fwd,
										int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle", portHandle});
	parameters.push_back({"HAL_Bool", fwd});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_RelayHandle> c;
	callFunc("HAL_InitializeRelayPort", parameters, c);
	HAL_RelayHandle x;
	c.get(x);
	return x;
}

void HAL_FreeRelayPort(HAL_RelayHandle relayPortHandle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle", relayPortHandle});
	callFunc("HAL_FreeRelayPort", parameters);
}

HAL_Bool HAL_CheckRelayChannel(int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckRelayChannel", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_SetRelay(HAL_RelayHandle relayPortHandle, HAL_Bool on,
				  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle", relayPortHandle});
	parameters.push_back({"HAL_Bool", on});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetRelay", parameters);
}

HAL_Bool HAL_GetRelay(HAL_RelayHandle relayPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle", relayPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetRelay", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_InitializeSerialPort(HAL_SerialPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_InitializeSerialPort", parameters);
}

void HAL_InitializeSerialPortDirect(HAL_SerialPort port, const char* portName,
									int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"const char*", portName});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_InitializeSerialPortDirect", parameters);
}

void HAL_SetSerialBaudRate(HAL_SerialPort port, int32_t baud, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t", baud});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialBaudRate", parameters);
}

void HAL_SetSerialDataBits(HAL_SerialPort port, int32_t bits, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t", bits});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialDataBits", parameters);
}

void HAL_SetSerialParity(HAL_SerialPort port, int32_t parity, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t", parity});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialParity", parameters);
}

void HAL_SetSerialStopBits(HAL_SerialPort port, int32_t stopBits,
						   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t", stopBits});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialStopBits", parameters);
}

void HAL_SetSerialWriteMode(HAL_SerialPort port, int32_t mode,
							int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t", mode});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialWriteMode", parameters);
}

void HAL_SetSerialFlowControl(HAL_SerialPort port, int32_t flow,
							  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t", flow});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialFlowControl", parameters);
}

void HAL_SetSerialTimeout(HAL_SerialPort port, double timeout,
						  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"double", timeout});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialTimeout", parameters);
}

void HAL_EnableSerialTermination(HAL_SerialPort port, char terminator,
								 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"char", terminator});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_EnableSerialTermination", parameters);
}

void HAL_DisableSerialTermination(HAL_SerialPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_DisableSerialTermination", parameters);
}

void HAL_SetSerialReadBufferSize(HAL_SerialPort port, int32_t size,
								 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t", size});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialReadBufferSize", parameters);
}

void HAL_SetSerialWriteBufferSize(HAL_SerialPort port, int32_t size,
								  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t", size});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSerialWriteBufferSize", parameters);
}

int32_t HAL_GetSerialBytesReceived(HAL_SerialPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetSerialBytesReceived", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_ReadSerial(HAL_SerialPort port, char* buffer, int32_t count,
					   int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"char*", buffer});
	parameters.push_back({"int32_t", count});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_ReadSerial", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_WriteSerial(HAL_SerialPort port, const char* buffer, int32_t count,
						int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"const char*", buffer});
	parameters.push_back({"int32_t", count});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_WriteSerial", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_FlushSerial(HAL_SerialPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_FlushSerial", parameters);
}

void HAL_ClearSerial(HAL_SerialPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ClearSerial", parameters);
}

void HAL_CloseSerial(HAL_SerialPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_CloseSerial", parameters);
}

HAL_SolenoidHandle HAL_InitializeSolenoidPort(HAL_PortHandle portHandle,
											  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle", portHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_SolenoidHandle> c;
	callFunc("HAL_InitializeSolenoidPort", parameters, c);
	HAL_SolenoidHandle x;
	c.get(x);
	return x;
}

void HAL_FreeSolenoidPort(HAL_SolenoidHandle solenoidPortHandle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle", solenoidPortHandle});
	callFunc("HAL_FreeSolenoidPort", parameters);
}

HAL_Bool HAL_CheckSolenoidModule(int32_t module) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckSolenoidModule", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_CheckSolenoidChannel(int32_t channel) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", channel});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_CheckSolenoidChannel", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetSolenoid(HAL_SolenoidHandle solenoidPortHandle,
						 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle", solenoidPortHandle});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetSolenoid", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

int32_t HAL_GetAllSolenoids(int32_t module, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetAllSolenoids", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_SetSolenoid(HAL_SolenoidHandle solenoidPortHandle, HAL_Bool value,
					 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle", solenoidPortHandle});
	parameters.push_back({"HAL_Bool", value});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSolenoid", parameters);
}

void HAL_SetAllSolenoids(int32_t module, int32_t state, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t", state});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetAllSolenoids", parameters);
}

int32_t HAL_GetPCMSolenoidBlackList(int32_t module, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetPCMSolenoidBlackList", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetPCMSolenoidVoltageStickyFault(int32_t module, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetPCMSolenoidVoltageStickyFault", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_GetPCMSolenoidVoltageFault(int32_t module, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_GetPCMSolenoidVoltageFault", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

void HAL_ClearAllPCMStickyFaults(int32_t module, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t", module});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ClearAllPCMStickyFaults", parameters);
}

void HAL_SetOneShotDuration(HAL_SolenoidHandle solenoidPortHandle,
							int32_t durMS, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle", solenoidPortHandle});
	parameters.push_back({"int32_t", durMS});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetOneShotDuration", parameters);
}

void HAL_FireOneShot(HAL_SolenoidHandle solenoidPortHandle, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle", solenoidPortHandle});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_FireOneShot", parameters);
}

void HAL_InitializeSPI(HAL_SPIPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_InitializeSPI", parameters);
}

int32_t HAL_TransactionSPI(HAL_SPIPort port, const uint8_t* dataToSend,
						   uint8_t* dataReceived, int32_t size) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"const uint8_t*", dataToSend});
	parameters.push_back({"uint8_t*", dataReceived});
	parameters.push_back({"int32_t", size});
	minerva::Channel<int32_t> c;
	callFunc("HAL_TransactionSPI", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_WriteSPI(HAL_SPIPort port, const uint8_t* dataToSend,
					 int32_t sendSize) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"const uint8_t*", dataToSend});
	parameters.push_back({"int32_t", sendSize});
	minerva::Channel<int32_t> c;
	callFunc("HAL_WriteSPI", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_ReadSPI(HAL_SPIPort port, uint8_t* buffer, int32_t count) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"uint8_t*", buffer});
	parameters.push_back({"int32_t", count});
	minerva::Channel<int32_t> c;
	callFunc("HAL_ReadSPI", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_CloseSPI(HAL_SPIPort port) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	callFunc("HAL_CloseSPI", parameters);
}

void HAL_SetSPISpeed(HAL_SPIPort port, int32_t speed) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t", speed});
	callFunc("HAL_SetSPISpeed", parameters);
}

void HAL_SetSPIOpts(HAL_SPIPort port, HAL_Bool msbFirst,
					HAL_Bool sampleOnTrailing, HAL_Bool clkIdleHigh) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"HAL_Bool", msbFirst});
	parameters.push_back({"HAL_Bool", sampleOnTrailing});
	parameters.push_back({"HAL_Bool", clkIdleHigh});
	callFunc("HAL_SetSPIOpts", parameters);
}

void HAL_SetSPIChipSelectActiveHigh(HAL_SPIPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSPIChipSelectActiveHigh", parameters);
}

void HAL_SetSPIChipSelectActiveLow(HAL_SPIPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSPIChipSelectActiveLow", parameters);
}

int32_t HAL_GetSPIHandle(HAL_SPIPort port) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetSPIHandle", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

void HAL_SetSPIHandle(HAL_SPIPort port, int32_t handle) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t", handle});
	callFunc("HAL_SetSPIHandle", parameters);
}

void HAL_InitSPIAuto(HAL_SPIPort port, int32_t bufferSize, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t", bufferSize});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_InitSPIAuto", parameters);
}

void HAL_FreeSPIAuto(HAL_SPIPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_FreeSPIAuto", parameters);
}

void HAL_StartSPIAutoRate(HAL_SPIPort port, double period, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"double", period});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_StartSPIAutoRate", parameters);
}

void HAL_StartSPIAutoTrigger(HAL_SPIPort port, HAL_Handle digitalSourceHandle,
							 HAL_AnalogTriggerType analogTriggerType,
							 HAL_Bool triggerRising, HAL_Bool triggerFalling,
							 int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"HAL_Handle", digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType", analogTriggerType});
	parameters.push_back({"HAL_Bool", triggerRising});
	parameters.push_back({"HAL_Bool", triggerFalling});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_StartSPIAutoTrigger", parameters);
}

void HAL_StopSPIAuto(HAL_SPIPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_StopSPIAuto", parameters);
}

void HAL_SetSPIAutoTransmitData(HAL_SPIPort port, const uint8_t* dataToSend,
								int32_t dataSize, int32_t zeroSize,
								int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"const uint8_t*", dataToSend});
	parameters.push_back({"int32_t", dataSize});
	parameters.push_back({"int32_t", zeroSize});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_SetSPIAutoTransmitData", parameters);
}

void HAL_ForceSPIAutoRead(HAL_SPIPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t*", status});
	callFunc("HAL_ForceSPIAutoRead", parameters);
}

int32_t HAL_ReadSPIAutoReceivedData(HAL_SPIPort port, uint8_t* buffer,
									int32_t numToRead, double timeout,
									int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"uint8_t*", buffer});
	parameters.push_back({"int32_t", numToRead});
	parameters.push_back({"double", timeout});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_ReadSPIAutoReceivedData", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetSPIAutoDroppedCount(HAL_SPIPort port, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort", port});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetSPIAutoDroppedCount", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetThreadPriority(NativeThreadHandle handle, HAL_Bool* isRealTime,
							  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"NativeThreadHandle", handle});
	parameters.push_back({"HAL_Bool*", isRealTime});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetThreadPriority", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

int32_t HAL_GetCurrentThreadPriority(HAL_Bool* isRealTime, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool*", isRealTime});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<int32_t> c;
	callFunc("HAL_GetCurrentThreadPriority", parameters, c);
	int32_t x;
	c.get(x);
	return x;
}

HAL_Bool HAL_SetThreadPriority(NativeThreadHandle handle, HAL_Bool realTime,
							   int32_t priority, int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"NativeThreadHandle", handle});
	parameters.push_back({"HAL_Bool", realTime});
	parameters.push_back({"int32_t", priority});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_SetThreadPriority", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

HAL_Bool HAL_SetCurrentThreadPriority(HAL_Bool realTime, int32_t priority,
									  int32_t* status) {
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool", realTime});
	parameters.push_back({"int32_t", priority});
	parameters.push_back({"int32_t*", status});
	minerva::Channel<HAL_Bool> c;
	callFunc("HAL_SetCurrentThreadPriority", parameters, c);
	HAL_Bool x;
	c.get(x);
	return x;
}

#ifdef __cplusplus
}
#endif
