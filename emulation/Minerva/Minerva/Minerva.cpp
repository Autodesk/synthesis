//Auto-generated HAL interface for emulation

#pragma once

#include "visa/visa.h"
#include "AnalogInternal.h"
#include "ConstantsInternal.h"
#include "DigitalInternal.h"
#include "EncoderInternal.h"
#include "FPGAEncoder.h"
#include "FRC_NetworkCommunication/CANSessionMux.h"	//CAN Comm
#include "FRC_NetworkCommunication/CANSessionMux.h"
#include "HAL/Accelerometer.h"
#include "HAL/AnalogAccumulator.h"
#include "HAL/AnalogGyro.h"
#include "HAL/AnalogInput.h"
#include "HAL/AnalogOutput.h"
#include "HAL/AnalogTrigger.h"
#include "HAL/ChipObject.h"
#include "HAL/Compressor.h"
#include "HAL/Constants.h"
#include "HAL/Counter.h"
#include "HAL/DIO.h"
#include "HAL/DriverStation.h"
#include "HAL/Encoder.h"
#include "HAL/Errors.h"
#include "HAL/HAL.h"
#include "HAL/I2C.h"
#include "HAL/Interrupts.h"
#include "HAL/Notifier.h"
#include "HAL/OSSerialPort.h"
#include "HAL/PDP.h"
#include "HAL/PWM.h"
#include "HAL/Ports.h"
#include "HAL/Power.h"
#include "HAL/Relay.h"
#include "HAL/SPI.h"
#include "HAL/SerialPort.h"
#include "HAL/Solenoid.h"
#include "HAL/Threads.h"
#include "HAL/cpp/NotifierInternal.h"
#include "HAL/cpp/SerialHelper.h"
#include "HAL/cpp/make_unique.h"
#include "HAL/cpp/priority_condition_variable.h"
#include "HAL/cpp/priority_mutex.h"
#include "HAL/handles/HandlesInternal.h"
#include "HAL/handles/IndexedHandleResource.h"
#include "HAL/handles/LimitedClassedHandleResource.h"
#include "HAL/handles/LimitedHandleResource.h"
#include "HAL/handles/UnlimitedHandleResource.h"
#include "PCMInternal.h"
#include "PortsInternal.h"
#include "ctre/CtreCanNode.h"
#include "ctre/PCM.h"
#include "ctre/PCM.h"
#include "ctre/PDP.h"
#include "ctre/PDP.h"
#include "ctre/ctre.h"
#include "support/SafeThread.h"
#include "visa/visa.h"
#include <FRC_NetworkCommunication/AICalibration.h>
#include <FRC_NetworkCommunication/CANSessionMux.h>
#include <FRC_NetworkCommunication/FRCComm.h>
#include <FRC_NetworkCommunication/LoadOut.h>
#include <algorithm>
#include <atomic>
#include <cassert>
#include <chrono>
#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <fcntl.h>
#include <fstream>
#include <i2clib/i2c-lib.h>
#include <limits>
#include <llvm/StringRef.h>
#include <llvm/raw_ostream.h>
#include <memory>
#include <mutex>
#include <pthread.h>
#include <sched.h>
#include <signal.h>  // linux for kill
#include <spilib/spi-lib.h>
#include <stdint.h>
#include <string.h> // memset
#include <string>
#include <support/SafeThread.h>
#include <sys/ioctl.h>
#include <sys/prctl.h>
#include <termios.h>
#include <thread>
#include <unistd.h>

#include <FunctionSignature.h> // ParameterValueInfo

using namespace hal;

#ifdef __cplusplus
extern "C" {
#endif

void HAL_SetAccelerometerActive(HAL_Bool active){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool",active});
	callFunc("HAL_SetAccelerometerActive",params);
}

void HAL_SetAccelerometerRange(HAL_AccelerometerRange range){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AccelerometerRange",range});
	callFunc("HAL_SetAccelerometerRange",params);
}

double HAL_GetAccelerometerX(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<double> c;
	callFunc("HAL_GetAccelerometerX",params,c);
	return c.get();
}

double HAL_GetAccelerometerY(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<double> c;
	callFunc("HAL_GetAccelerometerY",params,c);
	return c.get();
}

double HAL_GetAccelerometerZ(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<double> c;
	callFunc("HAL_GetAccelerometerZ",params,c);
	return c.get();
}

HAL_Bool HAL_IsAccumulatorChannel(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_IsAccumulatorChannel",params,c);
	return c.get();
}

void HAL_InitAccumulator(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitAccumulator",params);
}

void HAL_ResetAccumulator(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetAccumulator",params);
}

void HAL_SetAccumulatorCenter(HAL_AnalogInputHandle analogPortHandle, int32_t center, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t",center});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAccumulatorCenter",params);
}

void HAL_SetAccumulatorDeadband(HAL_AnalogInputHandle analogPortHandle, int32_t deadband, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t",deadband});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAccumulatorDeadband",params);
}

int64_t HAL_GetAccumulatorValue(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetAccumulatorValue",params,c);
	return c.get();
}

int64_t HAL_GetAccumulatorCount(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetAccumulatorCount",params,c);
	return c.get();
}

void HAL_GetAccumulatorOutput(HAL_AnalogInputHandle analogPortHandle, int64_t* value, int64_t* count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int64_t*",value});
	parameters.push_back({"int64_t*",count});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_GetAccumulatorOutput",params);
}

HAL_GyroHandle HAL_InitializeAnalogGyro(HAL_AnalogInputHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_GyroHandle> c;
	callFunc("HAL_InitializeAnalogGyro",params,c);
	return c.get();
}

void HAL_SetupAnalogGyro(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetupAnalogGyro",params);
}

void HAL_FreeAnalogGyro(HAL_GyroHandle handle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	callFunc("HAL_FreeAnalogGyro",params);
}

void HAL_SetAnalogGyroParameters(HAL_GyroHandle handle, double voltsPerDegreePerSecond, double offset, int32_t center, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"double",voltsPerDegreePerSecond});
	parameters.push_back({"double",offset});
	parameters.push_back({"int32_t",center});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogGyroParameters",params);
}

void HAL_SetAnalogGyroVoltsPerDegreePerSecond(HAL_GyroHandle handle, double voltsPerDegreePerSecond, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"double",voltsPerDegreePerSecond});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogGyroVoltsPerDegreePerSecond",params);
}

void HAL_ResetAnalogGyro(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetAnalogGyro",params);
}

void HAL_CalibrateAnalogGyro(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CalibrateAnalogGyro",params);
}

void HAL_SetAnalogGyroDeadband(HAL_GyroHandle handle, double volts, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"double",volts});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogGyroDeadband",params);
}

double HAL_GetAnalogGyroAngle(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogGyroAngle",params,c);
	return c.get();
}

double HAL_GetAnalogGyroRate(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogGyroRate",params,c);
	return c.get();
}

double HAL_GetAnalogGyroOffset(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogGyroOffset",params,c);
	return c.get();
}

int32_t HAL_GetAnalogGyroCenter(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogGyroCenter",params,c);
	return c.get();
}

HAL_AnalogInputHandle HAL_InitializeAnalogInputPort(HAL_PortHandle portHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_AnalogInputHandle> c;
	callFunc("HAL_InitializeAnalogInputPort",params,c);
	return c.get();
}

void HAL_FreeAnalogInputPort(HAL_AnalogInputHandle analogPortHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	callFunc("HAL_FreeAnalogInputPort",params);
}

HAL_Bool HAL_CheckAnalogModule(int32_t module){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogModule",params,c);
	return c.get();
}

HAL_Bool HAL_CheckAnalogInputChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogInputChannel",params,c);
	return c.get();
}

void HAL_SetAnalogSampleRate(double samplesPerSecond, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double",samplesPerSecond});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogSampleRate",params);
}

double HAL_GetAnalogSampleRate(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogSampleRate",params,c);
	return c.get();
}

void HAL_SetAnalogAverageBits(HAL_AnalogInputHandle analogPortHandle, int32_t bits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t",bits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogAverageBits",params);
}

int32_t HAL_GetAnalogAverageBits(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogAverageBits",params,c);
	return c.get();
}

void HAL_SetAnalogOversampleBits(HAL_AnalogInputHandle analogPortHandle, int32_t bits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t",bits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogOversampleBits",params);
}

int32_t HAL_GetAnalogOversampleBits(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogOversampleBits",params,c);
	return c.get();
}

int32_t HAL_GetAnalogValue(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogValue",params,c);
	return c.get();
}

int32_t HAL_GetAnalogAverageValue(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogAverageValue",params,c);
	return c.get();
}

int32_t HAL_GetAnalogVoltsToValue(HAL_AnalogInputHandle analogPortHandle, double voltage, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"double",voltage});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogVoltsToValue",params,c);
	return c.get();
}

double HAL_GetAnalogVoltage(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogVoltage",params,c);
	return c.get();
}

double HAL_GetAnalogAverageVoltage(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogAverageVoltage",params,c);
	return c.get();
}

int32_t HAL_GetAnalogLSBWeight(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogLSBWeight",params,c);
	return c.get();
}

int32_t HAL_GetAnalogOffset(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogOffset",params,c);
	return c.get();
}

HAL_AnalogOutputHandle HAL_InitializeAnalogOutputPort(HAL_PortHandle portHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_AnalogOutputHandle> c;
	callFunc("HAL_InitializeAnalogOutputPort",params,c);
	return c.get();
}

void HAL_FreeAnalogOutputPort(HAL_AnalogOutputHandle analogOutputHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle",analogOutputHandle});
	callFunc("HAL_FreeAnalogOutputPort",params);
}

void HAL_SetAnalogOutput(HAL_AnalogOutputHandle analogOutputHandle, double voltage, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle",analogOutputHandle});
	parameters.push_back({"double",voltage});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogOutput",params);
}

double HAL_GetAnalogOutput(HAL_AnalogOutputHandle analogOutputHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle",analogOutputHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogOutput",params,c);
	return c.get();
}

HAL_Bool HAL_CheckAnalogOutputChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogOutputChannel",params,c);
	return c.get();
}

HAL_AnalogTriggerHandle HAL_InitializeAnalogTrigger(HAL_AnalogInputHandle portHandle, int32_t* index, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",portHandle});
	parameters.push_back({"int32_t*",index});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_AnalogTriggerHandle> c;
	callFunc("HAL_InitializeAnalogTrigger",params,c);
	return c.get();
}

void HAL_CleanAnalogTrigger(HAL_AnalogTriggerHandle analogTriggerHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CleanAnalogTrigger",params);
}

void HAL_SetAnalogTriggerLimitsRaw(HAL_AnalogTriggerHandle analogTriggerHandle, int32_t lower, int32_t upper, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"int32_t",lower});
	parameters.push_back({"int32_t",upper});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogTriggerLimitsRaw",params);
}

void HAL_SetAnalogTriggerLimitsVoltage(HAL_AnalogTriggerHandle analogTriggerHandle, double lower, double upper, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"double",lower});
	parameters.push_back({"double",upper});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogTriggerLimitsVoltage",params);
}

void HAL_SetAnalogTriggerAveraged(HAL_AnalogTriggerHandle analogTriggerHandle, HAL_Bool useAveragedValue, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"HAL_Bool",useAveragedValue});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogTriggerAveraged",params);
}

void HAL_SetAnalogTriggerFiltered(HAL_AnalogTriggerHandle analogTriggerHandle, HAL_Bool useFilteredValue, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"HAL_Bool",useFilteredValue});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogTriggerFiltered",params);
}

HAL_Bool HAL_GetAnalogTriggerInWindow(HAL_AnalogTriggerHandle analogTriggerHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerInWindow",params,c);
	return c.get();
}

HAL_Bool HAL_GetAnalogTriggerTriggerState(HAL_AnalogTriggerHandle analogTriggerHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerTriggerState",params,c);
	return c.get();
}

HAL_Bool HAL_GetAnalogTriggerOutput(HAL_AnalogTriggerHandle analogTriggerHandle, HAL_AnalogTriggerType type, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"HAL_AnalogTriggerType",type});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerOutput",params,c);
	return c.get();
}

HAL_CompressorHandle HAL_InitializeCompressor(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_CompressorHandle> c;
	callFunc("HAL_InitializeCompressor",params,c);
	return c.get();
}

HAL_Bool HAL_CheckCompressorModule(int32_t module){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckCompressorModule",params,c);
	return c.get();
}

HAL_Bool HAL_GetCompressor(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressor",params,c);
	return c.get();
}

void HAL_SetCompressorClosedLoopControl(HAL_CompressorHandle compressorHandle, HAL_Bool value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"HAL_Bool",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCompressorClosedLoopControl",params);
}

HAL_Bool HAL_GetCompressorClosedLoopControl(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorClosedLoopControl",params,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorPressureSwitch(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorPressureSwitch",params,c);
	return c.get();
}

double HAL_GetCompressorCurrent(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetCompressorCurrent",params,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorCurrentTooHighFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorCurrentTooHighFault",params,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorCurrentTooHighStickyFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorCurrentTooHighStickyFault",params,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorShortedStickyFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorShortedStickyFault",params,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorShortedFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorShortedFault",params,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorNotConnectedStickyFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorNotConnectedStickyFault",params,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorNotConnectedFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorNotConnectedFault",params,c);
	return c.get();
}

int32_t HAL_GetSystemClockTicksPerMicrosecond(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetSystemClockTicksPerMicrosecond",params,c);
	return c.get();
}

HAL_CounterHandle HAL_InitializeCounter(HAL_Counter_Mode mode, int32_t* index, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Counter_Mode",mode});
	parameters.push_back({"int32_t*",index});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_CounterHandle> c;
	callFunc("HAL_InitializeCounter",params,c);
	return c.get();
}

void HAL_FreeCounter(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreeCounter",params);
}

void HAL_SetCounterAverageSize(HAL_CounterHandle counterHandle, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterAverageSize",params);
}

void HAL_SetCounterUpSource(HAL_CounterHandle counterHandle, HAL_Handle digitalSourceHandle, HAL_AnalogTriggerType analogTriggerType, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Handle",digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerType});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterUpSource",params);
}

void HAL_SetCounterUpSourceEdge(HAL_CounterHandle counterHandle, HAL_Bool risingEdge, HAL_Bool fallingEdge, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",risingEdge});
	parameters.push_back({"HAL_Bool",fallingEdge});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterUpSourceEdge",params);
}

void HAL_ClearCounterUpSource(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearCounterUpSource",params);
}

void HAL_SetCounterDownSource(HAL_CounterHandle counterHandle, HAL_Handle digitalSourceHandle, HAL_AnalogTriggerType analogTriggerType, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Handle",digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerType});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterDownSource",params);
}

void HAL_SetCounterDownSourceEdge(HAL_CounterHandle counterHandle, HAL_Bool risingEdge, HAL_Bool fallingEdge, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",risingEdge});
	parameters.push_back({"HAL_Bool",fallingEdge});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterDownSourceEdge",params);
}

void HAL_ClearCounterDownSource(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearCounterDownSource",params);
}

void HAL_SetCounterUpDownMode(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterUpDownMode",params);
}

void HAL_SetCounterExternalDirectionMode(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterExternalDirectionMode",params);
}

void HAL_SetCounterSemiPeriodMode(HAL_CounterHandle counterHandle, HAL_Bool highSemiPeriod, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",highSemiPeriod});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterSemiPeriodMode",params);
}

void HAL_SetCounterPulseLengthMode(HAL_CounterHandle counterHandle, double threshold, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"double",threshold});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterPulseLengthMode",params);
}

int32_t HAL_GetCounterSamplesToAverage(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetCounterSamplesToAverage",params,c);
	return c.get();
}

void HAL_SetCounterSamplesToAverage(HAL_CounterHandle counterHandle, int32_t samplesToAverage, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t",samplesToAverage});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterSamplesToAverage",params);
}

void HAL_ResetCounter(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetCounter",params);
}

int32_t HAL_GetCounter(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetCounter",params,c);
	return c.get();
}

double HAL_GetCounterPeriod(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetCounterPeriod",params,c);
	return c.get();
}

void HAL_SetCounterMaxPeriod(HAL_CounterHandle counterHandle, double maxPeriod, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"double",maxPeriod});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterMaxPeriod",params);
}

void HAL_SetCounterUpdateWhenEmpty(HAL_CounterHandle counterHandle, HAL_Bool enabled, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",enabled});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterUpdateWhenEmpty",params);
}

HAL_Bool HAL_GetCounterStopped(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCounterStopped",params,c);
	return c.get();
}

HAL_Bool HAL_GetCounterDirection(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCounterDirection",params,c);
	return c.get();
}

void HAL_SetCounterReverseDirection(HAL_CounterHandle counterHandle, HAL_Bool reverseDirection, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",reverseDirection});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterReverseDirection",params);
}

HAL_NotifierHandle HAL_InitializeNotifierNonThreadedUnsafe(HAL_NotifierProcessFunction process, void* param, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierProcessFunction",process});
	parameters.push_back({"void*",param});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_NotifierHandle> c;
	callFunc("HAL_InitializeNotifierNonThreadedUnsafe",params,c);
	return c.get();
}

HAL_DigitalHandle HAL_InitializeDIOPort(HAL_PortHandle portHandle, HAL_Bool input, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"HAL_Bool",input});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_DigitalHandle> c;
	callFunc("HAL_InitializeDIOPort",params,c);
	return c.get();
}

HAL_Bool HAL_CheckDIOChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckDIOChannel",params,c);
	return c.get();
}

void HAL_FreeDIOPort(HAL_DigitalHandle dioPortHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	callFunc("HAL_FreeDIOPort",params);
}

HAL_DigitalPWMHandle HAL_AllocateDigitalPWM(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_DigitalPWMHandle> c;
	callFunc("HAL_AllocateDigitalPWM",params,c);
	return c.get();
}

void HAL_FreeDigitalPWM(HAL_DigitalPWMHandle pwmGenerator, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle",pwmGenerator});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreeDigitalPWM",params);
}

void HAL_SetDigitalPWMRate(double rate, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double",rate});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetDigitalPWMRate",params);
}

void HAL_SetDigitalPWMDutyCycle(HAL_DigitalPWMHandle pwmGenerator, double dutyCycle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle",pwmGenerator});
	parameters.push_back({"double",dutyCycle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetDigitalPWMDutyCycle",params);
}

void HAL_SetDigitalPWMOutputChannel(HAL_DigitalPWMHandle pwmGenerator, int32_t channel, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle",pwmGenerator});
	parameters.push_back({"int32_t",channel});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetDigitalPWMOutputChannel",params);
}

void HAL_SetDIO(HAL_DigitalHandle dioPortHandle, HAL_Bool value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"HAL_Bool",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetDIO",params);
}

HAL_Bool HAL_GetDIO(HAL_DigitalHandle dioPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetDIO",params,c);
	return c.get();
}

HAL_Bool HAL_GetDIODirection(HAL_DigitalHandle dioPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetDIODirection",params,c);
	return c.get();
}

void HAL_Pulse(HAL_DigitalHandle dioPortHandle, double pulseLength, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"double",pulseLength});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_Pulse",params);
}

HAL_Bool HAL_IsPulsing(HAL_DigitalHandle dioPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_IsPulsing",params,c);
	return c.get();
}

HAL_Bool HAL_IsAnyPulsing(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_IsAnyPulsing",params,c);
	return c.get();
}

void HAL_SetFilterSelect(HAL_DigitalHandle dioPortHandle, int32_t filterIndex, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t",filterIndex});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetFilterSelect",params);
}

int32_t HAL_GetFilterSelect(HAL_DigitalHandle dioPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetFilterSelect",params,c);
	return c.get();
}

void HAL_SetFilterPeriod(int32_t filterIndex, int64_t value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",filterIndex});
	parameters.push_back({"int64_t",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetFilterPeriod",params);
}

int64_t HAL_GetFilterPeriod(int32_t filterIndex, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",filterIndex});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetFilterPeriod",params,c);
	return c.get();
}

int32_t HAL_SetErrorData(const char* errors, int32_t errorsLength, int32_t waitMs){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"const char*",errors});
	parameters.push_back({"int32_t",errorsLength});
	parameters.push_back({"int32_t",waitMs});
	Channel<int32_t> c;
	callFunc("HAL_SetErrorData",params,c);
	return c.get();
}

int32_t HAL_SendError(HAL_Bool isError, int32_t errorCode, HAL_Bool isLVCode, const char* details, const char* location, const char* callStack, HAL_Bool printMsg){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool",isError});
	parameters.push_back({"int32_t",errorCode});
	parameters.push_back({"HAL_Bool",isLVCode});
	parameters.push_back({"const char*",details});
	parameters.push_back({"const char*",location});
	parameters.push_back({"const char*",callStack});
	parameters.push_back({"HAL_Bool",printMsg});
	Channel<int32_t> c;
	callFunc("HAL_SendError",params,c);
	return c.get();
}

int32_t HAL_GetControlWord(HAL_ControlWord* controlWord){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_ControlWord*",controlWord});
	Channel<int32_t> c;
	callFunc("HAL_GetControlWord",params,c);
	return c.get();
}

HAL_AllianceStationID HAL_GetAllianceStation(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_AllianceStationID> c;
	callFunc("HAL_GetAllianceStation",params,c);
	return c.get();
}

int32_t HAL_GetJoystickAxes(int32_t joystickNum, HAL_JoystickAxes* axes){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"HAL_JoystickAxes*",axes});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickAxes",params,c);
	return c.get();
}

int32_t HAL_GetJoystickPOVs(int32_t joystickNum, HAL_JoystickPOVs* povs){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"HAL_JoystickPOVs*",povs});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickPOVs",params,c);
	return c.get();
}

int32_t HAL_GetJoystickButtons(int32_t joystickNum, HAL_JoystickButtons* buttons){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"HAL_JoystickButtons*",buttons});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickButtons",params,c);
	return c.get();
}

int32_t HAL_GetJoystickDescriptor(int32_t joystickNum, HAL_JoystickDescriptor* desc){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"HAL_JoystickDescriptor*",desc});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickDescriptor",params,c);
	return c.get();
}

HAL_Bool HAL_GetJoystickIsXbox(int32_t joystickNum){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetJoystickIsXbox",params,c);
	return c.get();
}

int32_t HAL_GetJoystickType(int32_t joystickNum){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickType",params,c);
	return c.get();
}

char* HAL_GetJoystickName(int32_t joystickNum){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	Channel<char*> c;
	callFunc("HAL_GetJoystickName",params,c);
	return c.get();
}

void HAL_FreeJoystickName(char* name){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"char*",name});
	callFunc("HAL_FreeJoystickName",params);
}

int32_t HAL_GetJoystickAxisType(int32_t joystickNum, int32_t axis){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"int32_t",axis});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickAxisType",params,c);
	return c.get();
}

int32_t HAL_SetJoystickOutputs(int32_t joystickNum, int64_t outputs, int32_t leftRumble, int32_t rightRumble){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"int64_t",outputs});
	parameters.push_back({"int32_t",leftRumble});
	parameters.push_back({"int32_t",rightRumble});
	Channel<int32_t> c;
	callFunc("HAL_SetJoystickOutputs",params,c);
	return c.get();
}

double HAL_GetMatchTime(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetMatchTime",params,c);
	return c.get();
}

void HAL_ReleaseDSMutex(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ReleaseDSMutex",params);
}

bool HAL_IsNewControlData(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<bool> c;
	callFunc("HAL_IsNewControlData",params,c);
	return c.get();
}

void HAL_WaitForDSData(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_WaitForDSData",params);
}

HAL_Bool HAL_WaitForDSDataTimeout(double timeout){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double",timeout});
	Channel<HAL_Bool> c;
	callFunc("HAL_WaitForDSDataTimeout",params,c);
	return c.get();
}

void HAL_InitializeDriverStation(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_InitializeDriverStation",params);
}

void HAL_ObserveUserProgramStarting(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramStarting",params);
}

void HAL_ObserveUserProgramDisabled(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramDisabled",params);
}

void HAL_ObserveUserProgramAutonomous(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramAutonomous",params);
}

void HAL_ObserveUserProgramTeleop(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramTeleop",params);
}

void HAL_ObserveUserProgramTest(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramTest",params);
}

HAL_EncoderHandle HAL_InitializeEncoder(HAL_Handle digitalSourceHandleA, HAL_AnalogTriggerType analogTriggerTypeA, HAL_Handle digitalSourceHandleB, HAL_AnalogTriggerType analogTriggerTypeB, HAL_Bool reverseDirection, HAL_EncoderEncodingType encodingType, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Handle",digitalSourceHandleA});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerTypeA});
	parameters.push_back({"HAL_Handle",digitalSourceHandleB});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerTypeB});
	parameters.push_back({"HAL_Bool",reverseDirection});
	parameters.push_back({"HAL_EncoderEncodingType",encodingType});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_EncoderHandle> c;
	callFunc("HAL_InitializeEncoder",params,c);
	return c.get();
}

void HAL_FreeEncoder(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreeEncoder",params);
}

int32_t HAL_GetEncoder(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoder",params,c);
	return c.get();
}

int32_t HAL_GetEncoderRaw(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoderRaw",params,c);
	return c.get();
}

int32_t HAL_GetEncoderEncodingScale(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoderEncodingScale",params,c);
	return c.get();
}

void HAL_ResetEncoder(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetEncoder",params);
}

double HAL_GetEncoderPeriod(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderPeriod",params,c);
	return c.get();
}

void HAL_SetEncoderMaxPeriod(HAL_EncoderHandle encoderHandle, double maxPeriod, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"double",maxPeriod});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderMaxPeriod",params);
}

HAL_Bool HAL_GetEncoderStopped(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetEncoderStopped",params,c);
	return c.get();
}

HAL_Bool HAL_GetEncoderDirection(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetEncoderDirection",params,c);
	return c.get();
}

double HAL_GetEncoderDistance(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderDistance",params,c);
	return c.get();
}

double HAL_GetEncoderRate(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderRate",params,c);
	return c.get();
}

void HAL_SetEncoderMinRate(HAL_EncoderHandle encoderHandle, double minRate, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"double",minRate});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderMinRate",params);
}

void HAL_SetEncoderDistancePerPulse(HAL_EncoderHandle encoderHandle, double distancePerPulse, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"double",distancePerPulse});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderDistancePerPulse",params);
}

void HAL_SetEncoderReverseDirection(HAL_EncoderHandle encoderHandle, HAL_Bool reverseDirection, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"HAL_Bool",reverseDirection});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderReverseDirection",params);
}

void HAL_SetEncoderSamplesToAverage(HAL_EncoderHandle encoderHandle, int32_t samplesToAverage, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t",samplesToAverage});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderSamplesToAverage",params);
}

int32_t HAL_GetEncoderSamplesToAverage(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoderSamplesToAverage",params,c);
	return c.get();
}

void HAL_SetEncoderIndexSource(HAL_EncoderHandle encoderHandle, HAL_Handle digitalSourceHandle, HAL_AnalogTriggerType analogTriggerType, HAL_EncoderIndexingType type, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"HAL_Handle",digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerType});
	parameters.push_back({"HAL_EncoderIndexingType",type});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderIndexSource",params);
}

int32_t HAL_GetEncoderFPGAIndex(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoderFPGAIndex",params,c);
	return c.get();
}

double HAL_GetEncoderDecodingScaleFactor(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderDecodingScaleFactor",params,c);
	return c.get();
}

double HAL_GetEncoderDistancePerPulse(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderDistancePerPulse",params,c);
	return c.get();
}

HAL_EncoderEncodingType HAL_GetEncoderEncodingType(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_EncoderEncodingType> c;
	callFunc("HAL_GetEncoderEncodingType",params,c);
	return c.get();
}

const char* HAL_GetErrorMessage(int32_t code){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",code});
	Channel<const char*> c;
	callFunc("HAL_GetErrorMessage",params,c);
	return c.get();
}

int32_t HAL_GetFPGAVersion(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetFPGAVersion",params,c);
	return c.get();
}

int64_t HAL_GetFPGARevision(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetFPGARevision",params,c);
	return c.get();
}

HAL_RuntimeType HAL_GetRuntimeType(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<HAL_RuntimeType> c;
	callFunc("HAL_GetRuntimeType",params,c);
	return c.get();
}

HAL_Bool HAL_GetFPGAButton(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetFPGAButton",params,c);
	return c.get();
}

HAL_Bool HAL_GetSystemActive(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetSystemActive",params,c);
	return c.get();
}

HAL_Bool HAL_GetBrownedOut(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetBrownedOut",params,c);
	return c.get();
}

void HAL_BaseInitialize(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_BaseInitialize",params);
}

HAL_PortHandle HAL_GetPort(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_PortHandle> c;
	callFunc("HAL_GetPort",params,c);
	return c.get();
}

HAL_PortHandle HAL_GetPortWithModule(int32_t module, int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t",channel});
	Channel<HAL_PortHandle> c;
	callFunc("HAL_GetPortWithModule",params,c);
	return c.get();
}

uint64_t HAL_GetFPGATime(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<uint64_t> c;
	callFunc("HAL_GetFPGATime",params,c);
	return c.get();
}

HAL_Bool HAL_Initialize(int32_t timeout, int32_t mode){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",timeout});
	parameters.push_back({"int32_t",mode});
	Channel<HAL_Bool> c;
	callFunc("HAL_Initialize",params,c);
	return c.get();
}

int64_t HAL_Report(int32_t resource, int32_t instanceNumber, int32_t context = 0, const char* feature = nullptr){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",resource});
	parameters.push_back({"int32_t",instanceNumber});
	parameters.push_back({"int32_t context =",0});
	parameters.push_back({"const char* feature =",nullptr});
	Channel<int64_t> c;
	callFunc("HAL_Report",params,c);
	return c.get();
}

int64_t HAL_Report(int32_t resource, int32_t instanceNumber, int32_t context, const char* feature){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",resource});
	parameters.push_back({"int32_t",instanceNumber});
	parameters.push_back({"int32_t",context});
	parameters.push_back({"const char*",feature});
	Channel<int64_t> c;
	callFunc("HAL_Report",params,c);
	return c.get();
}

static inline HAL_HandleEnum getHandleType(HAL_Handle handle) { // mask first 8 bits and cast to enum return static_cast<HAL_HandleEnum>((handle >> 24) & 0xff){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Handle handle) { // mask first 8 bits and cast to enum return static_cast<HAL_HandleEnum>((handle >> 24) &",0xff});
	Channel<static inline> c;
	callFunc("HAL_HandleEnum getHandleType",params,c);
	return c.get();
}

void HAL_InitializeI2C(HAL_I2CPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializeI2C",params);
}

int32_t HAL_TransactionI2C(HAL_I2CPort port, int32_t deviceAddress, uint8_t* dataToSend, int32_t sendSize, uint8_t* dataReceived, int32_t receiveSize){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	parameters.push_back({"int32_t",deviceAddress});
	parameters.push_back({"uint8_t*",dataToSend});
	parameters.push_back({"int32_t",sendSize});
	parameters.push_back({"uint8_t*",dataReceived});
	parameters.push_back({"int32_t",receiveSize});
	Channel<int32_t> c;
	callFunc("HAL_TransactionI2C",params,c);
	return c.get();
}

int32_t HAL_WriteI2C(HAL_I2CPort port, int32_t deviceAddress, uint8_t* dataToSend, int32_t sendSize){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	parameters.push_back({"int32_t",deviceAddress});
	parameters.push_back({"uint8_t*",dataToSend});
	parameters.push_back({"int32_t",sendSize});
	Channel<int32_t> c;
	callFunc("HAL_WriteI2C",params,c);
	return c.get();
}

int32_t HAL_ReadI2C(HAL_I2CPort port, int32_t deviceAddress, uint8_t* buffer, int32_t count){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	parameters.push_back({"int32_t",deviceAddress});
	parameters.push_back({"uint8_t*",buffer});
	parameters.push_back({"int32_t",count});
	Channel<int32_t> c;
	callFunc("HAL_ReadI2C",params,c);
	return c.get();
}

void HAL_CloseI2C(HAL_I2CPort port){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	callFunc("HAL_CloseI2C",params);
}

HAL_InterruptHandle HAL_InitializeInterrupts(HAL_Bool watcher, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool",watcher});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_InterruptHandle> c;
	callFunc("HAL_InitializeInterrupts",params,c);
	return c.get();
}

void HAL_CleanInterrupts(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CleanInterrupts",params);
}

int64_t HAL_WaitForInterrupt(HAL_InterruptHandle interruptHandle, double timeout, HAL_Bool ignorePrevious, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"double",timeout});
	parameters.push_back({"HAL_Bool",ignorePrevious});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_WaitForInterrupt",params,c);
	return c.get();
}

void HAL_EnableInterrupts(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_EnableInterrupts",params);
}

void HAL_DisableInterrupts(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_DisableInterrupts",params);
}

double HAL_ReadInterruptRisingTimestamp(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_ReadInterruptRisingTimestamp",params,c);
	return c.get();
}

double HAL_ReadInterruptFallingTimestamp(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_ReadInterruptFallingTimestamp",params,c);
	return c.get();
}

void HAL_RequestInterrupts(HAL_InterruptHandle interruptHandle, HAL_Handle digitalSourceHandle, HAL_AnalogTriggerType analogTriggerType, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"HAL_Handle",digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerType});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_RequestInterrupts",params);
}

void HAL_AttachInterruptHandler(HAL_InterruptHandle interruptHandle, HAL_InterruptHandlerFunction handler, void* param, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"HAL_InterruptHandlerFunction",handler});
	parameters.push_back({"void*",param});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_AttachInterruptHandler",params);
}

void HAL_AttachInterruptHandlerThreaded(HAL_InterruptHandle interruptHandle, HAL_InterruptHandlerFunction handler, void* param, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"HAL_InterruptHandlerFunction",handler});
	parameters.push_back({"void*",param});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_AttachInterruptHandlerThreaded",params);
}

void HAL_SetInterruptUpSourceEdge(HAL_InterruptHandle interruptHandle, HAL_Bool risingEdge, HAL_Bool fallingEdge, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"HAL_Bool",risingEdge});
	parameters.push_back({"HAL_Bool",fallingEdge});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetInterruptUpSourceEdge",params);
}

HAL_NotifierHandle HAL_InitializeNotifier(HAL_NotifierProcessFunction process, void* param, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierProcessFunction",process});
	parameters.push_back({"void*",param});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_NotifierHandle> c;
	callFunc("HAL_InitializeNotifier",params,c);
	return c.get();
}

void HAL_CleanNotifier(HAL_NotifierHandle notifierHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle",notifierHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CleanNotifier",params);
}

void* HAL_GetNotifierParam(HAL_NotifierHandle notifierHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle",notifierHandle});
	parameters.push_back({"int32_t*",status});
	Channel<void*> c;
	callFunc("HAL_GetNotifierParam",params,c);
	return c.get();
}

void HAL_UpdateNotifierAlarm(HAL_NotifierHandle notifierHandle, uint64_t triggerTime, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle",notifierHandle});
	parameters.push_back({"uint64_t",triggerTime});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_UpdateNotifierAlarm",params);
}

void HAL_StopNotifierAlarm(HAL_NotifierHandle notifierHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle",notifierHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_StopNotifierAlarm",params);
}

void HAL_InitializeOSSerialPort(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializeOSSerialPort",params);
}

void HAL_SetOSSerialBaudRate(HAL_SerialPort port, int32_t baud, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",baud});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialBaudRate",params);
}

void HAL_SetOSSerialDataBits(HAL_SerialPort port, int32_t bits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",bits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialDataBits",params);
}

void HAL_SetOSSerialParity(HAL_SerialPort port, int32_t parity, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",parity});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialParity",params);
}

void HAL_SetOSSerialStopBits(HAL_SerialPort port, int32_t stopBits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",stopBits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialStopBits",params);
}

void HAL_SetOSSerialWriteMode(HAL_SerialPort port, int32_t mode, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",mode});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialWriteMode",params);
}

void HAL_SetOSSerialFlowControl(HAL_SerialPort port, int32_t flow, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",flow});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialFlowControl",params);
}

void HAL_SetOSSerialTimeout(HAL_SerialPort port, double timeout, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"double",timeout});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialTimeout",params);
}

void HAL_EnableOSSerialTermination(HAL_SerialPort port, char terminator, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"char",terminator});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_EnableOSSerialTermination",params);
}

void HAL_DisableOSSerialTermination(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_DisableOSSerialTermination",params);
}

void HAL_SetOSSerialReadBufferSize(HAL_SerialPort port, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialReadBufferSize",params);
}

void HAL_SetOSSerialWriteBufferSize(HAL_SerialPort port, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialWriteBufferSize",params);
}

int32_t HAL_GetOSSerialBytesReceived(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetOSSerialBytesReceived",params,c);
	return c.get();
}

int32_t HAL_ReadOSSerial(HAL_SerialPort port, char* buffer, int32_t count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"char*",buffer});
	parameters.push_back({"int32_t",count});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_ReadOSSerial",params,c);
	return c.get();
}

int32_t HAL_WriteOSSerial(HAL_SerialPort port, const char* buffer, int32_t count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"const char*",buffer});
	parameters.push_back({"int32_t",count});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_WriteOSSerial",params,c);
	return c.get();
}

void HAL_FlushOSSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FlushOSSerial",params);
}

void HAL_ClearOSSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearOSSerial",params);
}

void HAL_CloseOSSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CloseOSSerial",params);
}

void HAL_InitializePDP(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializePDP",params);
}

HAL_Bool HAL_CheckPDPChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckPDPChannel",params,c);
	return c.get();
}

HAL_Bool HAL_CheckPDPModule(int32_t module){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckPDPModule",params,c);
	return c.get();
}

double HAL_GetPDPTemperature(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPTemperature",params,c);
	return c.get();
}

double HAL_GetPDPVoltage(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPVoltage",params,c);
	return c.get();
}

double HAL_GetPDPChannelCurrent(int32_t module, int32_t channel, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t",channel});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPChannelCurrent",params,c);
	return c.get();
}

double HAL_GetPDPTotalCurrent(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPTotalCurrent",params,c);
	return c.get();
}

double HAL_GetPDPTotalPower(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPTotalPower",params,c);
	return c.get();
}

double HAL_GetPDPTotalEnergy(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPTotalEnergy",params,c);
	return c.get();
}

void HAL_ResetPDPTotalEnergy(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetPDPTotalEnergy",params);
}

void HAL_ClearPDPStickyFaults(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearPDPStickyFaults",params);
}

int32_t HAL_GetNumAccumulators(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumAccumulators",params,c);
	return c.get();
}

int32_t HAL_GetNumAnalogTriggers(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogTriggers",params,c);
	return c.get();
}

int32_t HAL_GetNumAnalogInputs(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogInputs",params,c);
	return c.get();
}

int32_t HAL_GetNumAnalogOutputs(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogOutputs",params,c);
	return c.get();
}

int32_t HAL_GetNumCounters(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumCounters",params,c);
	return c.get();
}

int32_t HAL_GetNumDigitalHeaders(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalHeaders",params,c);
	return c.get();
}

int32_t HAL_GetNumPWMHeaders(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPWMHeaders",params,c);
	return c.get();
}

int32_t HAL_GetNumDigitalChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalChannels",params,c);
	return c.get();
}

int32_t HAL_GetNumPWMChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPWMChannels",params,c);
	return c.get();
}

int32_t HAL_GetNumDigitalPWMOutputs(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalPWMOutputs",params,c);
	return c.get();
}

int32_t HAL_GetNumEncoders(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumEncoders",params,c);
	return c.get();
}

int32_t HAL_GetNumInterrupts(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumInterrupts",params,c);
	return c.get();
}

int32_t HAL_GetNumRelayChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumRelayChannels",params,c);
	return c.get();
}

int32_t HAL_GetNumRelayHeaders(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumRelayHeaders",params,c);
	return c.get();
}

int32_t HAL_GetNumPCMModules(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPCMModules",params,c);
	return c.get();
}

int32_t HAL_GetNumSolenoidChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumSolenoidChannels",params,c);
	return c.get();
}

int32_t HAL_GetNumPDPModules(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPDPModules",params,c);
	return c.get();
}

int32_t HAL_GetNumPDPChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPDPChannels",params,c);
	return c.get();
}

double HAL_GetVinVoltage(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetVinVoltage",params,c);
	return c.get();
}

double HAL_GetVinCurrent(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetVinCurrent",params,c);
	return c.get();
}

double HAL_GetUserVoltage6V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserVoltage6V",params,c);
	return c.get();
}

double HAL_GetUserCurrent6V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserCurrent6V",params,c);
	return c.get();
}

HAL_Bool HAL_GetUserActive6V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive6V",params,c);
	return c.get();
}

int32_t HAL_GetUserCurrentFaults6V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults6V",params,c);
	return c.get();
}

double HAL_GetUserVoltage5V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserVoltage5V",params,c);
	return c.get();
}

double HAL_GetUserCurrent5V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserCurrent5V",params,c);
	return c.get();
}

HAL_Bool HAL_GetUserActive5V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive5V",params,c);
	return c.get();
}

int32_t HAL_GetUserCurrentFaults5V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults5V",params,c);
	return c.get();
}

double HAL_GetUserVoltage3V3(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserVoltage3V3",params,c);
	return c.get();
}

double HAL_GetUserCurrent3V3(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserCurrent3V3",params,c);
	return c.get();
}

HAL_Bool HAL_GetUserActive3V3(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive3V3",params,c);
	return c.get();
}

int32_t HAL_GetUserCurrentFaults3V3(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults3V3",params,c);
	return c.get();
}

HAL_DigitalHandle HAL_InitializePWMPort(HAL_PortHandle portHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_DigitalHandle> c;
	callFunc("HAL_InitializePWMPort",params,c);
	return c.get();
}

void HAL_FreePWMPort(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreePWMPort",params);
}

HAL_Bool HAL_CheckPWMChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckPWMChannel",params,c);
	return c.get();
}

void HAL_SetPWMConfig(HAL_DigitalHandle pwmPortHandle, double maxPwm, double deadbandMaxPwm, double centerPwm, double deadbandMinPwm, double minPwm, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"double",maxPwm});
	parameters.push_back({"double",deadbandMaxPwm});
	parameters.push_back({"double",centerPwm});
	parameters.push_back({"double",deadbandMinPwm});
	parameters.push_back({"double",minPwm});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMConfig",params);
}

void HAL_SetPWMConfigRaw(HAL_DigitalHandle pwmPortHandle, int32_t maxPwm, int32_t deadbandMaxPwm, int32_t centerPwm, int32_t deadbandMinPwm, int32_t minPwm, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t",maxPwm});
	parameters.push_back({"int32_t",deadbandMaxPwm});
	parameters.push_back({"int32_t",centerPwm});
	parameters.push_back({"int32_t",deadbandMinPwm});
	parameters.push_back({"int32_t",minPwm});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMConfigRaw",params);
}

void HAL_GetPWMConfigRaw(HAL_DigitalHandle pwmPortHandle, int32_t* maxPwm, int32_t* deadbandMaxPwm, int32_t* centerPwm, int32_t* deadbandMinPwm, int32_t* minPwm, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",maxPwm});
	parameters.push_back({"int32_t*",deadbandMaxPwm});
	parameters.push_back({"int32_t*",centerPwm});
	parameters.push_back({"int32_t*",deadbandMinPwm});
	parameters.push_back({"int32_t*",minPwm});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_GetPWMConfigRaw",params);
}

void HAL_SetPWMEliminateDeadband(HAL_DigitalHandle pwmPortHandle, HAL_Bool eliminateDeadband, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"HAL_Bool",eliminateDeadband});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMEliminateDeadband",params);
}

HAL_Bool HAL_GetPWMEliminateDeadband(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetPWMEliminateDeadband",params,c);
	return c.get();
}

void HAL_SetPWMRaw(HAL_DigitalHandle pwmPortHandle, int32_t value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMRaw",params);
}

void HAL_SetPWMSpeed(HAL_DigitalHandle pwmPortHandle, double speed, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"double",speed});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMSpeed",params);
}

void HAL_SetPWMPosition(HAL_DigitalHandle pwmPortHandle, double position, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"double",position});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMPosition",params);
}

void HAL_SetPWMDisabled(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMDisabled",params);
}

int32_t HAL_GetPWMRaw(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetPWMRaw",params,c);
	return c.get();
}

double HAL_GetPWMSpeed(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPWMSpeed",params,c);
	return c.get();
}

double HAL_GetPWMPosition(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPWMPosition",params,c);
	return c.get();
}

void HAL_LatchPWMZero(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_LatchPWMZero",params);
}

void HAL_SetPWMPeriodScale(HAL_DigitalHandle pwmPortHandle, int32_t squelchMask, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t",squelchMask});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMPeriodScale",params);
}

int32_t HAL_GetLoopTiming(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetLoopTiming",params,c);
	return c.get();
}

HAL_RelayHandle HAL_InitializeRelayPort(HAL_PortHandle portHandle, HAL_Bool fwd, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"HAL_Bool",fwd});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_RelayHandle> c;
	callFunc("HAL_InitializeRelayPort",params,c);
	return c.get();
}

void HAL_FreeRelayPort(HAL_RelayHandle relayPortHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle",relayPortHandle});
	callFunc("HAL_FreeRelayPort",params);
}

HAL_Bool HAL_CheckRelayChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckRelayChannel",params,c);
	return c.get();
}

void HAL_SetRelay(HAL_RelayHandle relayPortHandle, HAL_Bool on, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle",relayPortHandle});
	parameters.push_back({"HAL_Bool",on});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetRelay",params);
}

HAL_Bool HAL_GetRelay(HAL_RelayHandle relayPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle",relayPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetRelay",params,c);
	return c.get();
}

void HAL_InitializeSerialPort(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializeSerialPort",params);
}

void HAL_SetSerialBaudRate(HAL_SerialPort port, int32_t baud, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",baud});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialBaudRate",params);
}

void HAL_SetSerialDataBits(HAL_SerialPort port, int32_t bits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",bits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialDataBits",params);
}

void HAL_SetSerialParity(HAL_SerialPort port, int32_t parity, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",parity});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialParity",params);
}

void HAL_SetSerialStopBits(HAL_SerialPort port, int32_t stopBits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",stopBits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialStopBits",params);
}

void HAL_SetSerialWriteMode(HAL_SerialPort port, int32_t mode, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",mode});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialWriteMode",params);
}

void HAL_SetSerialFlowControl(HAL_SerialPort port, int32_t flow, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",flow});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialFlowControl",params);
}

void HAL_SetSerialTimeout(HAL_SerialPort port, double timeout, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"double",timeout});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialTimeout",params);
}

void HAL_EnableSerialTermination(HAL_SerialPort port, char terminator, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"char",terminator});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_EnableSerialTermination",params);
}

void HAL_DisableSerialTermination(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_DisableSerialTermination",params);
}

void HAL_SetSerialReadBufferSize(HAL_SerialPort port, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialReadBufferSize",params);
}

void HAL_SetSerialWriteBufferSize(HAL_SerialPort port, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialWriteBufferSize",params);
}

int32_t HAL_GetSerialBytesReceived(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetSerialBytesReceived",params,c);
	return c.get();
}

int32_t HAL_ReadSerial(HAL_SerialPort port, char* buffer, int32_t count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"char*",buffer});
	parameters.push_back({"int32_t",count});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_ReadSerial",params,c);
	return c.get();
}

int32_t HAL_WriteSerial(HAL_SerialPort port, const char* buffer, int32_t count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"const char*",buffer});
	parameters.push_back({"int32_t",count});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_WriteSerial",params,c);
	return c.get();
}

void HAL_FlushSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FlushSerial",params);
}

void HAL_ClearSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearSerial",params);
}

void HAL_CloseSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CloseSerial",params);
}

HAL_SolenoidHandle HAL_InitializeSolenoidPort(HAL_PortHandle portHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_SolenoidHandle> c;
	callFunc("HAL_InitializeSolenoidPort",params,c);
	return c.get();
}

void HAL_FreeSolenoidPort(HAL_SolenoidHandle solenoidPortHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle",solenoidPortHandle});
	callFunc("HAL_FreeSolenoidPort",params);
}

HAL_Bool HAL_CheckSolenoidModule(int32_t module){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckSolenoidModule",params,c);
	return c.get();
}

HAL_Bool HAL_CheckSolenoidChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckSolenoidChannel",params,c);
	return c.get();
}

HAL_Bool HAL_GetSolenoid(HAL_SolenoidHandle solenoidPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle",solenoidPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetSolenoid",params,c);
	return c.get();
}

int32_t HAL_GetAllSolenoids(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAllSolenoids",params,c);
	return c.get();
}

void HAL_SetSolenoid(HAL_SolenoidHandle solenoidPortHandle, HAL_Bool value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle",solenoidPortHandle});
	parameters.push_back({"HAL_Bool",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSolenoid",params);
}

void HAL_SetAllSolenoids(int32_t module, int32_t state, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t",state});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAllSolenoids",params);
}

int32_t HAL_GetPCMSolenoidBlackList(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetPCMSolenoidBlackList",params,c);
	return c.get();
}

HAL_Bool HAL_GetPCMSolenoidVoltageStickyFault(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetPCMSolenoidVoltageStickyFault",params,c);
	return c.get();
}

HAL_Bool HAL_GetPCMSolenoidVoltageFault(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetPCMSolenoidVoltageFault",params,c);
	return c.get();
}

void HAL_ClearAllPCMStickyFaults(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearAllPCMStickyFaults",params);
}

void HAL_InitializeSPI(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializeSPI",params);
}

int32_t HAL_TransactionSPI(HAL_SPIPort port, uint8_t* dataToSend, uint8_t* dataReceived, int32_t size){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"uint8_t*",dataToSend});
	parameters.push_back({"uint8_t*",dataReceived});
	parameters.push_back({"int32_t",size});
	Channel<int32_t> c;
	callFunc("HAL_TransactionSPI",params,c);
	return c.get();
}

int32_t HAL_WriteSPI(HAL_SPIPort port, uint8_t* dataToSend, int32_t sendSize){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"uint8_t*",dataToSend});
	parameters.push_back({"int32_t",sendSize});
	Channel<int32_t> c;
	callFunc("HAL_WriteSPI",params,c);
	return c.get();
}

int32_t HAL_ReadSPI(HAL_SPIPort port, uint8_t* buffer, int32_t count){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"uint8_t*",buffer});
	parameters.push_back({"int32_t",count});
	Channel<int32_t> c;
	callFunc("HAL_ReadSPI",params,c);
	return c.get();
}

void HAL_CloseSPI(HAL_SPIPort port){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	callFunc("HAL_CloseSPI",params);
}

void HAL_SetSPISpeed(HAL_SPIPort port, int32_t speed){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",speed});
	callFunc("HAL_SetSPISpeed",params);
}

void HAL_SetSPIOpts(HAL_SPIPort port, HAL_Bool msbFirst, HAL_Bool sampleOnTrailing, HAL_Bool clkIdleHigh){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"HAL_Bool",msbFirst});
	parameters.push_back({"HAL_Bool",sampleOnTrailing});
	parameters.push_back({"HAL_Bool",clkIdleHigh});
	callFunc("HAL_SetSPIOpts",params);
}

void HAL_SetSPIChipSelectActiveHigh(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSPIChipSelectActiveHigh",params);
}

void HAL_SetSPIChipSelectActiveLow(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSPIChipSelectActiveLow",params);
}

int32_t HAL_GetSPIHandle(HAL_SPIPort port){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	Channel<int32_t> c;
	callFunc("HAL_GetSPIHandle",params,c);
	return c.get();
}

void HAL_SetSPIHandle(HAL_SPIPort port, int32_t handle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",handle});
	callFunc("HAL_SetSPIHandle",params);
}

void HAL_InitSPIAccumulator(HAL_SPIPort port, int32_t period, int32_t cmd, int32_t xferSize, int32_t validMask, int32_t validValue, int32_t dataShift, int32_t dataSize, HAL_Bool isSigned, HAL_Bool bigEndian, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",period});
	parameters.push_back({"int32_t",cmd});
	parameters.push_back({"int32_t",xferSize});
	parameters.push_back({"int32_t",validMask});
	parameters.push_back({"int32_t",validValue});
	parameters.push_back({"int32_t",dataShift});
	parameters.push_back({"int32_t",dataSize});
	parameters.push_back({"HAL_Bool",isSigned});
	parameters.push_back({"HAL_Bool",bigEndian});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitSPIAccumulator",params);
}

void HAL_FreeSPIAccumulator(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreeSPIAccumulator",params);
}

void HAL_ResetSPIAccumulator(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetSPIAccumulator",params);
}

void HAL_SetSPIAccumulatorCenter(HAL_SPIPort port, int32_t center, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",center});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSPIAccumulatorCenter",params);
}

void HAL_SetSPIAccumulatorDeadband(HAL_SPIPort port, int32_t deadband, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",deadband});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSPIAccumulatorDeadband",params);
}

int32_t HAL_GetSPIAccumulatorLastValue(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetSPIAccumulatorLastValue",params,c);
	return c.get();
}

int64_t HAL_GetSPIAccumulatorValue(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetSPIAccumulatorValue",params,c);
	return c.get();
}

int64_t HAL_GetSPIAccumulatorCount(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetSPIAccumulatorCount",params,c);
	return c.get();
}

double HAL_GetSPIAccumulatorAverage(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetSPIAccumulatorAverage",params,c);
	return c.get();
}

void HAL_GetSPIAccumulatorOutput(HAL_SPIPort port, int64_t* value, int64_t* count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int64_t*",value});
	parameters.push_back({"int64_t*",count});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_GetSPIAccumulatorOutput",params);
}

int32_t HAL_GetThreadPriority(NativeThreadHandle handle, HAL_Bool* isRealTime, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"NativeThreadHandle",handle});
	parameters.push_back({"HAL_Bool*",isRealTime});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetThreadPriority",params,c);
	return c.get();
}

int32_t HAL_GetCurrentThreadPriority(HAL_Bool* isRealTime, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool*",isRealTime});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetCurrentThreadPriority",params,c);
	return c.get();
}

HAL_Bool HAL_SetThreadPriority(NativeThreadHandle handle, HAL_Bool realTime, int32_t priority, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"NativeThreadHandle",handle});
	parameters.push_back({"HAL_Bool",realTime});
	parameters.push_back({"int32_t",priority});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_SetThreadPriority",params,c);
	return c.get();
}

HAL_Bool HAL_SetCurrentThreadPriority(HAL_Bool realTime, int32_t priority, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool",realTime});
	parameters.push_back({"int32_t",priority});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_SetCurrentThreadPriority",params,c);
	return c.get();
}

#ifdef __cplusplus
}
#endif
