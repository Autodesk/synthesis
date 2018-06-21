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

#include "FunctionSignature.h" // ParameterValueInfo
#include "Channel.h"

using namespace hal;

#ifdef __cplusplus
extern "C" {
#endif

void HAL_SetAccelerometerActive(HAL_Bool active){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool",active});
	callFunc("HAL_SetAccelerometerActive",parameters);
}

void HAL_SetAccelerometerRange(HAL_AccelerometerRange range){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AccelerometerRange",range});
	callFunc("HAL_SetAccelerometerRange",parameters);
}

double HAL_GetAccelerometerX(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<double> c;
	callFunc("HAL_GetAccelerometerX",parameters,c);
	return c.get();
}

double HAL_GetAccelerometerY(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<double> c;
	callFunc("HAL_GetAccelerometerY",parameters,c);
	return c.get();
}

double HAL_GetAccelerometerZ(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<double> c;
	callFunc("HAL_GetAccelerometerZ",parameters,c);
	return c.get();
}

HAL_Bool HAL_IsAccumulatorChannel(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_IsAccumulatorChannel",parameters,c);
	return c.get();
}

void HAL_InitAccumulator(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitAccumulator",parameters);
}

void HAL_ResetAccumulator(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetAccumulator",parameters);
}

void HAL_SetAccumulatorCenter(HAL_AnalogInputHandle analogPortHandle, int32_t center, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t",center});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAccumulatorCenter",parameters);
}

void HAL_SetAccumulatorDeadband(HAL_AnalogInputHandle analogPortHandle, int32_t deadband, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t",deadband});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAccumulatorDeadband",parameters);
}

int64_t HAL_GetAccumulatorValue(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetAccumulatorValue",parameters,c);
	return c.get();
}

int64_t HAL_GetAccumulatorCount(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetAccumulatorCount",parameters,c);
	return c.get();
}

void HAL_GetAccumulatorOutput(HAL_AnalogInputHandle analogPortHandle, int64_t* value, int64_t* count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int64_t*",value});
	parameters.push_back({"int64_t*",count});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_GetAccumulatorOutput",parameters);
}

HAL_GyroHandle HAL_InitializeAnalogGyro(HAL_AnalogInputHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_GyroHandle> c;
	callFunc("HAL_InitializeAnalogGyro",parameters,c);
	return c.get();
}

void HAL_SetupAnalogGyro(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetupAnalogGyro",parameters);
}

void HAL_FreeAnalogGyro(HAL_GyroHandle handle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	callFunc("HAL_FreeAnalogGyro",parameters);
}

void HAL_SetAnalogGyroParameters(HAL_GyroHandle handle, double voltsPerDegreePerSecond, double offset, int32_t center, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"double",voltsPerDegreePerSecond});
	parameters.push_back({"double",offset});
	parameters.push_back({"int32_t",center});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogGyroParameters",parameters);
}

void HAL_SetAnalogGyroVoltsPerDegreePerSecond(HAL_GyroHandle handle, double voltsPerDegreePerSecond, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"double",voltsPerDegreePerSecond});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogGyroVoltsPerDegreePerSecond",parameters);
}

void HAL_ResetAnalogGyro(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetAnalogGyro",parameters);
}

void HAL_CalibrateAnalogGyro(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CalibrateAnalogGyro",parameters);
}

void HAL_SetAnalogGyroDeadband(HAL_GyroHandle handle, double volts, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"double",volts});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogGyroDeadband",parameters);
}

double HAL_GetAnalogGyroAngle(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogGyroAngle",parameters,c);
	return c.get();
}

double HAL_GetAnalogGyroRate(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogGyroRate",parameters,c);
	return c.get();
}

double HAL_GetAnalogGyroOffset(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogGyroOffset",parameters,c);
	return c.get();
}

int32_t HAL_GetAnalogGyroCenter(HAL_GyroHandle handle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_GyroHandle",handle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogGyroCenter",parameters,c);
	return c.get();
}

HAL_AnalogInputHandle HAL_InitializeAnalogInputPort(HAL_PortHandle portHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_AnalogInputHandle> c;
	callFunc("HAL_InitializeAnalogInputPort",parameters,c);
	return c.get();
}

void HAL_FreeAnalogInputPort(HAL_AnalogInputHandle analogPortHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	callFunc("HAL_FreeAnalogInputPort",parameters);
}

HAL_Bool HAL_CheckAnalogModule(int32_t module){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogModule",parameters,c);
	return c.get();
}

HAL_Bool HAL_CheckAnalogInputChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogInputChannel",parameters,c);
	return c.get();
}

void HAL_SetAnalogSampleRate(double samplesPerSecond, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double",samplesPerSecond});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogSampleRate",parameters);
}

double HAL_GetAnalogSampleRate(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogSampleRate",parameters,c);
	return c.get();
}

void HAL_SetAnalogAverageBits(HAL_AnalogInputHandle analogPortHandle, int32_t bits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t",bits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogAverageBits",parameters);
}

int32_t HAL_GetAnalogAverageBits(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogAverageBits",parameters,c);
	return c.get();
}

void HAL_SetAnalogOversampleBits(HAL_AnalogInputHandle analogPortHandle, int32_t bits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t",bits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogOversampleBits",parameters);
}

int32_t HAL_GetAnalogOversampleBits(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogOversampleBits",parameters,c);
	return c.get();
}

int32_t HAL_GetAnalogValue(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogValue",parameters,c);
	return c.get();
}

int32_t HAL_GetAnalogAverageValue(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogAverageValue",parameters,c);
	return c.get();
}

int32_t HAL_GetAnalogVoltsToValue(HAL_AnalogInputHandle analogPortHandle, double voltage, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"double",voltage});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogVoltsToValue",parameters,c);
	return c.get();
}

double HAL_GetAnalogVoltage(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogVoltage",parameters,c);
	return c.get();
}

double HAL_GetAnalogAverageVoltage(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogAverageVoltage",parameters,c);
	return c.get();
}

int32_t HAL_GetAnalogLSBWeight(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogLSBWeight",parameters,c);
	return c.get();
}

int32_t HAL_GetAnalogOffset(HAL_AnalogInputHandle analogPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",analogPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAnalogOffset",parameters,c);
	return c.get();
}

HAL_AnalogOutputHandle HAL_InitializeAnalogOutputPort(HAL_PortHandle portHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_AnalogOutputHandle> c;
	callFunc("HAL_InitializeAnalogOutputPort",parameters,c);
	return c.get();
}

void HAL_FreeAnalogOutputPort(HAL_AnalogOutputHandle analogOutputHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle",analogOutputHandle});
	callFunc("HAL_FreeAnalogOutputPort",parameters);
}

void HAL_SetAnalogOutput(HAL_AnalogOutputHandle analogOutputHandle, double voltage, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle",analogOutputHandle});
	parameters.push_back({"double",voltage});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogOutput",parameters);
}

double HAL_GetAnalogOutput(HAL_AnalogOutputHandle analogOutputHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogOutputHandle",analogOutputHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetAnalogOutput",parameters,c);
	return c.get();
}

HAL_Bool HAL_CheckAnalogOutputChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckAnalogOutputChannel",parameters,c);
	return c.get();
}

HAL_AnalogTriggerHandle HAL_InitializeAnalogTrigger(HAL_AnalogInputHandle portHandle, int32_t* index, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogInputHandle",portHandle});
	parameters.push_back({"int32_t*",index});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_AnalogTriggerHandle> c;
	callFunc("HAL_InitializeAnalogTrigger",parameters,c);
	return c.get();
}

void HAL_CleanAnalogTrigger(HAL_AnalogTriggerHandle analogTriggerHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CleanAnalogTrigger",parameters);
}

void HAL_SetAnalogTriggerLimitsRaw(HAL_AnalogTriggerHandle analogTriggerHandle, int32_t lower, int32_t upper, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"int32_t",lower});
	parameters.push_back({"int32_t",upper});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogTriggerLimitsRaw",parameters);
}

void HAL_SetAnalogTriggerLimitsVoltage(HAL_AnalogTriggerHandle analogTriggerHandle, double lower, double upper, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"double",lower});
	parameters.push_back({"double",upper});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogTriggerLimitsVoltage",parameters);
}

void HAL_SetAnalogTriggerAveraged(HAL_AnalogTriggerHandle analogTriggerHandle, HAL_Bool useAveragedValue, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"HAL_Bool",useAveragedValue});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogTriggerAveraged",parameters);
}

void HAL_SetAnalogTriggerFiltered(HAL_AnalogTriggerHandle analogTriggerHandle, HAL_Bool useFilteredValue, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"HAL_Bool",useFilteredValue});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAnalogTriggerFiltered",parameters);
}

HAL_Bool HAL_GetAnalogTriggerInWindow(HAL_AnalogTriggerHandle analogTriggerHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerInWindow",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetAnalogTriggerTriggerState(HAL_AnalogTriggerHandle analogTriggerHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerTriggerState",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetAnalogTriggerOutput(HAL_AnalogTriggerHandle analogTriggerHandle, HAL_AnalogTriggerType type, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_AnalogTriggerHandle",analogTriggerHandle});
	parameters.push_back({"HAL_AnalogTriggerType",type});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetAnalogTriggerOutput",parameters,c);
	return c.get();
}

HAL_CompressorHandle HAL_InitializeCompressor(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_CompressorHandle> c;
	callFunc("HAL_InitializeCompressor",parameters,c);
	return c.get();
}

HAL_Bool HAL_CheckCompressorModule(int32_t module){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckCompressorModule",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCompressor(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressor",parameters,c);
	return c.get();
}

void HAL_SetCompressorClosedLoopControl(HAL_CompressorHandle compressorHandle, HAL_Bool value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"HAL_Bool",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCompressorClosedLoopControl",parameters);
}

HAL_Bool HAL_GetCompressorClosedLoopControl(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorClosedLoopControl",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorPressureSwitch(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorPressureSwitch",parameters,c);
	return c.get();
}

double HAL_GetCompressorCurrent(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetCompressorCurrent",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorCurrentTooHighFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorCurrentTooHighFault",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorCurrentTooHighStickyFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorCurrentTooHighStickyFault",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorShortedStickyFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorShortedStickyFault",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorShortedFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorShortedFault",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorNotConnectedStickyFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorNotConnectedStickyFault",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCompressorNotConnectedFault(HAL_CompressorHandle compressorHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CompressorHandle",compressorHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCompressorNotConnectedFault",parameters,c);
	return c.get();
}

int32_t HAL_GetSystemClockTicksPerMicrosecond(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetSystemClockTicksPerMicrosecond",parameters,c);
	return c.get();
}

HAL_CounterHandle HAL_InitializeCounter(HAL_Counter_Mode mode, int32_t* index, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Counter_Mode",mode});
	parameters.push_back({"int32_t*",index});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_CounterHandle> c;
	callFunc("HAL_InitializeCounter",parameters,c);
	return c.get();
}

void HAL_FreeCounter(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreeCounter",parameters);
}

void HAL_SetCounterAverageSize(HAL_CounterHandle counterHandle, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterAverageSize",parameters);
}

void HAL_SetCounterUpSource(HAL_CounterHandle counterHandle, HAL_Handle digitalSourceHandle, HAL_AnalogTriggerType analogTriggerType, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Handle",digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerType});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterUpSource",parameters);
}

void HAL_SetCounterUpSourceEdge(HAL_CounterHandle counterHandle, HAL_Bool risingEdge, HAL_Bool fallingEdge, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",risingEdge});
	parameters.push_back({"HAL_Bool",fallingEdge});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterUpSourceEdge",parameters);
}

void HAL_ClearCounterUpSource(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearCounterUpSource",parameters);
}

void HAL_SetCounterDownSource(HAL_CounterHandle counterHandle, HAL_Handle digitalSourceHandle, HAL_AnalogTriggerType analogTriggerType, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Handle",digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerType});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterDownSource",parameters);
}

void HAL_SetCounterDownSourceEdge(HAL_CounterHandle counterHandle, HAL_Bool risingEdge, HAL_Bool fallingEdge, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",risingEdge});
	parameters.push_back({"HAL_Bool",fallingEdge});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterDownSourceEdge",parameters);
}

void HAL_ClearCounterDownSource(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearCounterDownSource",parameters);
}

void HAL_SetCounterUpDownMode(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterUpDownMode",parameters);
}

void HAL_SetCounterExternalDirectionMode(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterExternalDirectionMode",parameters);
}

void HAL_SetCounterSemiPeriodMode(HAL_CounterHandle counterHandle, HAL_Bool highSemiPeriod, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",highSemiPeriod});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterSemiPeriodMode",parameters);
}

void HAL_SetCounterPulseLengthMode(HAL_CounterHandle counterHandle, double threshold, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"double",threshold});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterPulseLengthMode",parameters);
}

int32_t HAL_GetCounterSamplesToAverage(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetCounterSamplesToAverage",parameters,c);
	return c.get();
}

void HAL_SetCounterSamplesToAverage(HAL_CounterHandle counterHandle, int32_t samplesToAverage, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t",samplesToAverage});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterSamplesToAverage",parameters);
}

void HAL_ResetCounter(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetCounter",parameters);
}

int32_t HAL_GetCounter(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetCounter",parameters,c);
	return c.get();
}

double HAL_GetCounterPeriod(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetCounterPeriod",parameters,c);
	return c.get();
}

void HAL_SetCounterMaxPeriod(HAL_CounterHandle counterHandle, double maxPeriod, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"double",maxPeriod});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterMaxPeriod",parameters);
}

void HAL_SetCounterUpdateWhenEmpty(HAL_CounterHandle counterHandle, HAL_Bool enabled, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",enabled});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterUpdateWhenEmpty",parameters);
}

HAL_Bool HAL_GetCounterStopped(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCounterStopped",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetCounterDirection(HAL_CounterHandle counterHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetCounterDirection",parameters,c);
	return c.get();
}

void HAL_SetCounterReverseDirection(HAL_CounterHandle counterHandle, HAL_Bool reverseDirection, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_CounterHandle",counterHandle});
	parameters.push_back({"HAL_Bool",reverseDirection});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetCounterReverseDirection",parameters);
}

HAL_NotifierHandle HAL_InitializeNotifierNonThreadedUnsafe(HAL_NotifierProcessFunction process, void* param, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierProcessFunction",process});
	parameters.push_back({"void*",param});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_NotifierHandle> c;
	callFunc("HAL_InitializeNotifierNonThreadedUnsafe",parameters,c);
	return c.get();
}

HAL_DigitalHandle HAL_InitializeDIOPort(HAL_PortHandle portHandle, HAL_Bool input, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"HAL_Bool",input});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_DigitalHandle> c;
	callFunc("HAL_InitializeDIOPort",parameters,c);
	return c.get();
}

HAL_Bool HAL_CheckDIOChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckDIOChannel",parameters,c);
	return c.get();
}

void HAL_FreeDIOPort(HAL_DigitalHandle dioPortHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	callFunc("HAL_FreeDIOPort",parameters);
}

HAL_DigitalPWMHandle HAL_AllocateDigitalPWM(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_DigitalPWMHandle> c;
	callFunc("HAL_AllocateDigitalPWM",parameters,c);
	return c.get();
}

void HAL_FreeDigitalPWM(HAL_DigitalPWMHandle pwmGenerator, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle",pwmGenerator});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreeDigitalPWM",parameters);
}

void HAL_SetDigitalPWMRate(double rate, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double",rate});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetDigitalPWMRate",parameters);
}

void HAL_SetDigitalPWMDutyCycle(HAL_DigitalPWMHandle pwmGenerator, double dutyCycle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle",pwmGenerator});
	parameters.push_back({"double",dutyCycle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetDigitalPWMDutyCycle",parameters);
}

void HAL_SetDigitalPWMOutputChannel(HAL_DigitalPWMHandle pwmGenerator, int32_t channel, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalPWMHandle",pwmGenerator});
	parameters.push_back({"int32_t",channel});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetDigitalPWMOutputChannel",parameters);
}

void HAL_SetDIO(HAL_DigitalHandle dioPortHandle, HAL_Bool value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"HAL_Bool",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetDIO",parameters);
}

HAL_Bool HAL_GetDIO(HAL_DigitalHandle dioPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetDIO",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetDIODirection(HAL_DigitalHandle dioPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetDIODirection",parameters,c);
	return c.get();
}

void HAL_Pulse(HAL_DigitalHandle dioPortHandle, double pulseLength, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"double",pulseLength});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_Pulse",parameters);
}

HAL_Bool HAL_IsPulsing(HAL_DigitalHandle dioPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_IsPulsing",parameters,c);
	return c.get();
}

HAL_Bool HAL_IsAnyPulsing(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_IsAnyPulsing",parameters,c);
	return c.get();
}

void HAL_SetFilterSelect(HAL_DigitalHandle dioPortHandle, int32_t filterIndex, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t",filterIndex});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetFilterSelect",parameters);
}

int32_t HAL_GetFilterSelect(HAL_DigitalHandle dioPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",dioPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetFilterSelect",parameters,c);
	return c.get();
}

void HAL_SetFilterPeriod(int32_t filterIndex, int64_t value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",filterIndex});
	parameters.push_back({"int64_t",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetFilterPeriod",parameters);
}

int64_t HAL_GetFilterPeriod(int32_t filterIndex, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",filterIndex});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetFilterPeriod",parameters,c);
	return c.get();
}

int32_t HAL_SetErrorData(const char* errors, int32_t errorsLength, int32_t waitMs){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"const char*",errors});
	parameters.push_back({"int32_t",errorsLength});
	parameters.push_back({"int32_t",waitMs});
	Channel<int32_t> c;
	callFunc("HAL_SetErrorData",parameters,c);
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
	callFunc("HAL_SendError",parameters,c);
	return c.get();
}

int32_t HAL_GetControlWord(HAL_ControlWord* controlWord){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_ControlWord*",controlWord});
	Channel<int32_t> c;
	callFunc("HAL_GetControlWord",parameters,c);
	return c.get();
}

HAL_AllianceStationID HAL_GetAllianceStation(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_AllianceStationID> c;
	callFunc("HAL_GetAllianceStation",parameters,c);
	return c.get();
}

int32_t HAL_GetJoystickAxes(int32_t joystickNum, HAL_JoystickAxes* axes){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"HAL_JoystickAxes*",axes});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickAxes",parameters,c);
	return c.get();
}

int32_t HAL_GetJoystickPOVs(int32_t joystickNum, HAL_JoystickPOVs* povs){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"HAL_JoystickPOVs*",povs});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickPOVs",parameters,c);
	return c.get();
}

int32_t HAL_GetJoystickButtons(int32_t joystickNum, HAL_JoystickButtons* buttons){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"HAL_JoystickButtons*",buttons});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickButtons",parameters,c);
	return c.get();
}

int32_t HAL_GetJoystickDescriptor(int32_t joystickNum, HAL_JoystickDescriptor* desc){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"HAL_JoystickDescriptor*",desc});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickDescriptor",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetJoystickIsXbox(int32_t joystickNum){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetJoystickIsXbox",parameters,c);
	return c.get();
}

int32_t HAL_GetJoystickType(int32_t joystickNum){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickType",parameters,c);
	return c.get();
}

char* HAL_GetJoystickName(int32_t joystickNum){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	Channel<char*> c;
	callFunc("HAL_GetJoystickName",parameters,c);
	return c.get();
}

void HAL_FreeJoystickName(char* name){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"char*",name});
	callFunc("HAL_FreeJoystickName",parameters);
}

int32_t HAL_GetJoystickAxisType(int32_t joystickNum, int32_t axis){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"int32_t",axis});
	Channel<int32_t> c;
	callFunc("HAL_GetJoystickAxisType",parameters,c);
	return c.get();
}

int32_t HAL_SetJoystickOutputs(int32_t joystickNum, int64_t outputs, int32_t leftRumble, int32_t rightRumble){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",joystickNum});
	parameters.push_back({"int64_t",outputs});
	parameters.push_back({"int32_t",leftRumble});
	parameters.push_back({"int32_t",rightRumble});
	Channel<int32_t> c;
	callFunc("HAL_SetJoystickOutputs",parameters,c);
	return c.get();
}

double HAL_GetMatchTime(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetMatchTime",parameters,c);
	return c.get();
}

void HAL_ReleaseDSMutex(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ReleaseDSMutex",parameters);
}

bool HAL_IsNewControlData(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<bool> c;
	callFunc("HAL_IsNewControlData",parameters,c);
	return c.get();
}

void HAL_WaitForDSData(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_WaitForDSData",parameters);
}

HAL_Bool HAL_WaitForDSDataTimeout(double timeout){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"double",timeout});
	Channel<HAL_Bool> c;
	callFunc("HAL_WaitForDSDataTimeout",parameters,c);
	return c.get();
}

void HAL_InitializeDriverStation(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_InitializeDriverStation",parameters);
}

void HAL_ObserveUserProgramStarting(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramStarting",parameters);
}

void HAL_ObserveUserProgramDisabled(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramDisabled",parameters);
}

void HAL_ObserveUserProgramAutonomous(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramAutonomous",parameters);
}

void HAL_ObserveUserProgramTeleop(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramTeleop",parameters);
}

void HAL_ObserveUserProgramTest(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	callFunc("HAL_ObserveUserProgramTest",parameters);
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
	callFunc("HAL_InitializeEncoder",parameters,c);
	return c.get();
}

void HAL_FreeEncoder(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreeEncoder",parameters);
}

int32_t HAL_GetEncoder(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoder",parameters,c);
	return c.get();
}

int32_t HAL_GetEncoderRaw(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoderRaw",parameters,c);
	return c.get();
}

int32_t HAL_GetEncoderEncodingScale(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoderEncodingScale",parameters,c);
	return c.get();
}

void HAL_ResetEncoder(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetEncoder",parameters);
}

double HAL_GetEncoderPeriod(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderPeriod",parameters,c);
	return c.get();
}

void HAL_SetEncoderMaxPeriod(HAL_EncoderHandle encoderHandle, double maxPeriod, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"double",maxPeriod});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderMaxPeriod",parameters);
}

HAL_Bool HAL_GetEncoderStopped(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetEncoderStopped",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetEncoderDirection(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetEncoderDirection",parameters,c);
	return c.get();
}

double HAL_GetEncoderDistance(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderDistance",parameters,c);
	return c.get();
}

double HAL_GetEncoderRate(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderRate",parameters,c);
	return c.get();
}

void HAL_SetEncoderMinRate(HAL_EncoderHandle encoderHandle, double minRate, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"double",minRate});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderMinRate",parameters);
}

void HAL_SetEncoderDistancePerPulse(HAL_EncoderHandle encoderHandle, double distancePerPulse, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"double",distancePerPulse});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderDistancePerPulse",parameters);
}

void HAL_SetEncoderReverseDirection(HAL_EncoderHandle encoderHandle, HAL_Bool reverseDirection, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"HAL_Bool",reverseDirection});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderReverseDirection",parameters);
}

void HAL_SetEncoderSamplesToAverage(HAL_EncoderHandle encoderHandle, int32_t samplesToAverage, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t",samplesToAverage});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderSamplesToAverage",parameters);
}

int32_t HAL_GetEncoderSamplesToAverage(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoderSamplesToAverage",parameters,c);
	return c.get();
}

void HAL_SetEncoderIndexSource(HAL_EncoderHandle encoderHandle, HAL_Handle digitalSourceHandle, HAL_AnalogTriggerType analogTriggerType, HAL_EncoderIndexingType type, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"HAL_Handle",digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerType});
	parameters.push_back({"HAL_EncoderIndexingType",type});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetEncoderIndexSource",parameters);
}

int32_t HAL_GetEncoderFPGAIndex(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetEncoderFPGAIndex",parameters,c);
	return c.get();
}

double HAL_GetEncoderDecodingScaleFactor(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderDecodingScaleFactor",parameters,c);
	return c.get();
}

double HAL_GetEncoderDistancePerPulse(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetEncoderDistancePerPulse",parameters,c);
	return c.get();
}

HAL_EncoderEncodingType HAL_GetEncoderEncodingType(HAL_EncoderHandle encoderHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_EncoderHandle",encoderHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_EncoderEncodingType> c;
	callFunc("HAL_GetEncoderEncodingType",parameters,c);
	return c.get();
}

const char* HAL_GetErrorMessage(int32_t code){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",code});
	Channel<const char*> c;
	callFunc("HAL_GetErrorMessage",parameters,c);
	return c.get();
}

int32_t HAL_GetFPGAVersion(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetFPGAVersion",parameters,c);
	return c.get();
}

int64_t HAL_GetFPGARevision(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetFPGARevision",parameters,c);
	return c.get();
}

HAL_RuntimeType HAL_GetRuntimeType(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<HAL_RuntimeType> c;
	callFunc("HAL_GetRuntimeType",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetFPGAButton(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetFPGAButton",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetSystemActive(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetSystemActive",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetBrownedOut(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetBrownedOut",parameters,c);
	return c.get();
}

void HAL_BaseInitialize(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_BaseInitialize",parameters);
}

HAL_PortHandle HAL_GetPort(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_PortHandle> c;
	callFunc("HAL_GetPort",parameters,c);
	return c.get();
}

HAL_PortHandle HAL_GetPortWithModule(int32_t module, int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t",channel});
	Channel<HAL_PortHandle> c;
	callFunc("HAL_GetPortWithModule",parameters,c);
	return c.get();
}

uint64_t HAL_GetFPGATime(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<uint64_t> c;
	callFunc("HAL_GetFPGATime",parameters,c);
	return c.get();
}

HAL_Bool HAL_Initialize(int32_t timeout, int32_t mode){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",timeout});
	parameters.push_back({"int32_t",mode});
	Channel<HAL_Bool> c;
	callFunc("HAL_Initialize",parameters,c);
	return c.get();
}

int64_t HAL_Report(int32_t resource, int32_t instanceNumber, int32_t context = 0, const char* feature = nullptr){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",resource});
	parameters.push_back({"int32_t",instanceNumber});
	parameters.push_back({"int32_t context =",0});
	parameters.push_back({"const char* feature =",nullptr});
	Channel<int64_t> c;
	callFunc("HAL_Report",parameters,c);
	return c.get();
}

int64_t HAL_Report(int32_t resource, int32_t instanceNumber, int32_t context, const char* feature){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",resource});
	parameters.push_back({"int32_t",instanceNumber});
	parameters.push_back({"int32_t",context});
	parameters.push_back({"const char*",feature});
	Channel<int64_t> c;
	callFunc("HAL_Report",parameters,c);
	return c.get();
}

static inline HAL_HandleEnum getHandleType(HAL_Handle handle) { // mask first 8 bits and cast to enum return static_cast<HAL_HandleEnum>((handle >> 24) & 0xff){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Handle handle) { // mask first 8 bits and cast to enum return static_cast<HAL_HandleEnum>((handle >> 24) &",0xff});
	Channel<static inline> c;
	callFunc("HAL_HandleEnum getHandleType",parameters,c);
	return c.get();
}

void HAL_InitializeI2C(HAL_I2CPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializeI2C",parameters);
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
	callFunc("HAL_TransactionI2C",parameters,c);
	return c.get();
}

int32_t HAL_WriteI2C(HAL_I2CPort port, int32_t deviceAddress, uint8_t* dataToSend, int32_t sendSize){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	parameters.push_back({"int32_t",deviceAddress});
	parameters.push_back({"uint8_t*",dataToSend});
	parameters.push_back({"int32_t",sendSize});
	Channel<int32_t> c;
	callFunc("HAL_WriteI2C",parameters,c);
	return c.get();
}

int32_t HAL_ReadI2C(HAL_I2CPort port, int32_t deviceAddress, uint8_t* buffer, int32_t count){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	parameters.push_back({"int32_t",deviceAddress});
	parameters.push_back({"uint8_t*",buffer});
	parameters.push_back({"int32_t",count});
	Channel<int32_t> c;
	callFunc("HAL_ReadI2C",parameters,c);
	return c.get();
}

void HAL_CloseI2C(HAL_I2CPort port){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_I2CPort",port});
	callFunc("HAL_CloseI2C",parameters);
}

HAL_InterruptHandle HAL_InitializeInterrupts(HAL_Bool watcher, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool",watcher});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_InterruptHandle> c;
	callFunc("HAL_InitializeInterrupts",parameters,c);
	return c.get();
}

void HAL_CleanInterrupts(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CleanInterrupts",parameters);
}

int64_t HAL_WaitForInterrupt(HAL_InterruptHandle interruptHandle, double timeout, HAL_Bool ignorePrevious, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"double",timeout});
	parameters.push_back({"HAL_Bool",ignorePrevious});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_WaitForInterrupt",parameters,c);
	return c.get();
}

void HAL_EnableInterrupts(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_EnableInterrupts",parameters);
}

void HAL_DisableInterrupts(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_DisableInterrupts",parameters);
}

double HAL_ReadInterruptRisingTimestamp(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_ReadInterruptRisingTimestamp",parameters,c);
	return c.get();
}

double HAL_ReadInterruptFallingTimestamp(HAL_InterruptHandle interruptHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_ReadInterruptFallingTimestamp",parameters,c);
	return c.get();
}

void HAL_RequestInterrupts(HAL_InterruptHandle interruptHandle, HAL_Handle digitalSourceHandle, HAL_AnalogTriggerType analogTriggerType, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"HAL_Handle",digitalSourceHandle});
	parameters.push_back({"HAL_AnalogTriggerType",analogTriggerType});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_RequestInterrupts",parameters);
}

void HAL_AttachInterruptHandler(HAL_InterruptHandle interruptHandle, HAL_InterruptHandlerFunction handler, void* param, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"HAL_InterruptHandlerFunction",handler});
	parameters.push_back({"void*",param});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_AttachInterruptHandler",parameters);
}

void HAL_AttachInterruptHandlerThreaded(HAL_InterruptHandle interruptHandle, HAL_InterruptHandlerFunction handler, void* param, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"HAL_InterruptHandlerFunction",handler});
	parameters.push_back({"void*",param});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_AttachInterruptHandlerThreaded",parameters);
}

void HAL_SetInterruptUpSourceEdge(HAL_InterruptHandle interruptHandle, HAL_Bool risingEdge, HAL_Bool fallingEdge, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_InterruptHandle",interruptHandle});
	parameters.push_back({"HAL_Bool",risingEdge});
	parameters.push_back({"HAL_Bool",fallingEdge});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetInterruptUpSourceEdge",parameters);
}

HAL_NotifierHandle HAL_InitializeNotifier(HAL_NotifierProcessFunction process, void* param, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierProcessFunction",process});
	parameters.push_back({"void*",param});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_NotifierHandle> c;
	callFunc("HAL_InitializeNotifier",parameters,c);
	return c.get();
}

void HAL_CleanNotifier(HAL_NotifierHandle notifierHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle",notifierHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CleanNotifier",parameters);
}

void* HAL_GetNotifierParam(HAL_NotifierHandle notifierHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle",notifierHandle});
	parameters.push_back({"int32_t*",status});
	Channel<void*> c;
	callFunc("HAL_GetNotifierParam",parameters,c);
	return c.get();
}

void HAL_UpdateNotifierAlarm(HAL_NotifierHandle notifierHandle, uint64_t triggerTime, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle",notifierHandle});
	parameters.push_back({"uint64_t",triggerTime});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_UpdateNotifierAlarm",parameters);
}

void HAL_StopNotifierAlarm(HAL_NotifierHandle notifierHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_NotifierHandle",notifierHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_StopNotifierAlarm",parameters);
}

void HAL_InitializeOSSerialPort(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializeOSSerialPort",parameters);
}

void HAL_SetOSSerialBaudRate(HAL_SerialPort port, int32_t baud, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",baud});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialBaudRate",parameters);
}

void HAL_SetOSSerialDataBits(HAL_SerialPort port, int32_t bits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",bits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialDataBits",parameters);
}

void HAL_SetOSSerialParity(HAL_SerialPort port, int32_t parity, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",parity});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialParity",parameters);
}

void HAL_SetOSSerialStopBits(HAL_SerialPort port, int32_t stopBits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",stopBits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialStopBits",parameters);
}

void HAL_SetOSSerialWriteMode(HAL_SerialPort port, int32_t mode, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",mode});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialWriteMode",parameters);
}

void HAL_SetOSSerialFlowControl(HAL_SerialPort port, int32_t flow, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",flow});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialFlowControl",parameters);
}

void HAL_SetOSSerialTimeout(HAL_SerialPort port, double timeout, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"double",timeout});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialTimeout",parameters);
}

void HAL_EnableOSSerialTermination(HAL_SerialPort port, char terminator, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"char",terminator});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_EnableOSSerialTermination",parameters);
}

void HAL_DisableOSSerialTermination(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_DisableOSSerialTermination",parameters);
}

void HAL_SetOSSerialReadBufferSize(HAL_SerialPort port, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialReadBufferSize",parameters);
}

void HAL_SetOSSerialWriteBufferSize(HAL_SerialPort port, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetOSSerialWriteBufferSize",parameters);
}

int32_t HAL_GetOSSerialBytesReceived(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetOSSerialBytesReceived",parameters,c);
	return c.get();
}

int32_t HAL_ReadOSSerial(HAL_SerialPort port, char* buffer, int32_t count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"char*",buffer});
	parameters.push_back({"int32_t",count});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_ReadOSSerial",parameters,c);
	return c.get();
}

int32_t HAL_WriteOSSerial(HAL_SerialPort port, const char* buffer, int32_t count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"const char*",buffer});
	parameters.push_back({"int32_t",count});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_WriteOSSerial",parameters,c);
	return c.get();
}

void HAL_FlushOSSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FlushOSSerial",parameters);
}

void HAL_ClearOSSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearOSSerial",parameters);
}

void HAL_CloseOSSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CloseOSSerial",parameters);
}

void HAL_InitializePDP(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializePDP",parameters);
}

HAL_Bool HAL_CheckPDPChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckPDPChannel",parameters,c);
	return c.get();
}

HAL_Bool HAL_CheckPDPModule(int32_t module){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckPDPModule",parameters,c);
	return c.get();
}

double HAL_GetPDPTemperature(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPTemperature",parameters,c);
	return c.get();
}

double HAL_GetPDPVoltage(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPVoltage",parameters,c);
	return c.get();
}

double HAL_GetPDPChannelCurrent(int32_t module, int32_t channel, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t",channel});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPChannelCurrent",parameters,c);
	return c.get();
}

double HAL_GetPDPTotalCurrent(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPTotalCurrent",parameters,c);
	return c.get();
}

double HAL_GetPDPTotalPower(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPTotalPower",parameters,c);
	return c.get();
}

double HAL_GetPDPTotalEnergy(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPDPTotalEnergy",parameters,c);
	return c.get();
}

void HAL_ResetPDPTotalEnergy(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetPDPTotalEnergy",parameters);
}

void HAL_ClearPDPStickyFaults(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearPDPStickyFaults",parameters);
}

int32_t HAL_GetNumAccumulators(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumAccumulators",parameters,c);
	return c.get();
}

int32_t HAL_GetNumAnalogTriggers(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogTriggers",parameters,c);
	return c.get();
}

int32_t HAL_GetNumAnalogInputs(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogInputs",parameters,c);
	return c.get();
}

int32_t HAL_GetNumAnalogOutputs(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumAnalogOutputs",parameters,c);
	return c.get();
}

int32_t HAL_GetNumCounters(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumCounters",parameters,c);
	return c.get();
}

int32_t HAL_GetNumDigitalHeaders(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalHeaders",parameters,c);
	return c.get();
}

int32_t HAL_GetNumPWMHeaders(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPWMHeaders",parameters,c);
	return c.get();
}

int32_t HAL_GetNumDigitalChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalChannels",parameters,c);
	return c.get();
}

int32_t HAL_GetNumPWMChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPWMChannels",parameters,c);
	return c.get();
}

int32_t HAL_GetNumDigitalPWMOutputs(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumDigitalPWMOutputs",parameters,c);
	return c.get();
}

int32_t HAL_GetNumEncoders(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumEncoders",parameters,c);
	return c.get();
}

int32_t HAL_GetNumInterrupts(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumInterrupts",parameters,c);
	return c.get();
}

int32_t HAL_GetNumRelayChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumRelayChannels",parameters,c);
	return c.get();
}

int32_t HAL_GetNumRelayHeaders(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumRelayHeaders",parameters,c);
	return c.get();
}

int32_t HAL_GetNumPCMModules(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPCMModules",parameters,c);
	return c.get();
}

int32_t HAL_GetNumSolenoidChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumSolenoidChannels",parameters,c);
	return c.get();
}

int32_t HAL_GetNumPDPModules(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPDPModules",parameters,c);
	return c.get();
}

int32_t HAL_GetNumPDPChannels(){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	Channel<int32_t> c;
	callFunc("HAL_GetNumPDPChannels",parameters,c);
	return c.get();
}

double HAL_GetVinVoltage(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetVinVoltage",parameters,c);
	return c.get();
}

double HAL_GetVinCurrent(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetVinCurrent",parameters,c);
	return c.get();
}

double HAL_GetUserVoltage6V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserVoltage6V",parameters,c);
	return c.get();
}

double HAL_GetUserCurrent6V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserCurrent6V",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetUserActive6V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive6V",parameters,c);
	return c.get();
}

int32_t HAL_GetUserCurrentFaults6V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults6V",parameters,c);
	return c.get();
}

double HAL_GetUserVoltage5V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserVoltage5V",parameters,c);
	return c.get();
}

double HAL_GetUserCurrent5V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserCurrent5V",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetUserActive5V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive5V",parameters,c);
	return c.get();
}

int32_t HAL_GetUserCurrentFaults5V(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults5V",parameters,c);
	return c.get();
}

double HAL_GetUserVoltage3V3(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserVoltage3V3",parameters,c);
	return c.get();
}

double HAL_GetUserCurrent3V3(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetUserCurrent3V3",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetUserActive3V3(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetUserActive3V3",parameters,c);
	return c.get();
}

int32_t HAL_GetUserCurrentFaults3V3(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetUserCurrentFaults3V3",parameters,c);
	return c.get();
}

HAL_DigitalHandle HAL_InitializePWMPort(HAL_PortHandle portHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_DigitalHandle> c;
	callFunc("HAL_InitializePWMPort",parameters,c);
	return c.get();
}

void HAL_FreePWMPort(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreePWMPort",parameters);
}

HAL_Bool HAL_CheckPWMChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckPWMChannel",parameters,c);
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
	callFunc("HAL_SetPWMConfig",parameters);
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
	callFunc("HAL_SetPWMConfigRaw",parameters);
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
	callFunc("HAL_GetPWMConfigRaw",parameters);
}

void HAL_SetPWMEliminateDeadband(HAL_DigitalHandle pwmPortHandle, HAL_Bool eliminateDeadband, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"HAL_Bool",eliminateDeadband});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMEliminateDeadband",parameters);
}

HAL_Bool HAL_GetPWMEliminateDeadband(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetPWMEliminateDeadband",parameters,c);
	return c.get();
}

void HAL_SetPWMRaw(HAL_DigitalHandle pwmPortHandle, int32_t value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMRaw",parameters);
}

void HAL_SetPWMSpeed(HAL_DigitalHandle pwmPortHandle, double speed, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"double",speed});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMSpeed",parameters);
}

void HAL_SetPWMPosition(HAL_DigitalHandle pwmPortHandle, double position, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"double",position});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMPosition",parameters);
}

void HAL_SetPWMDisabled(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMDisabled",parameters);
}

int32_t HAL_GetPWMRaw(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetPWMRaw",parameters,c);
	return c.get();
}

double HAL_GetPWMSpeed(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPWMSpeed",parameters,c);
	return c.get();
}

double HAL_GetPWMPosition(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetPWMPosition",parameters,c);
	return c.get();
}

void HAL_LatchPWMZero(HAL_DigitalHandle pwmPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_LatchPWMZero",parameters);
}

void HAL_SetPWMPeriodScale(HAL_DigitalHandle pwmPortHandle, int32_t squelchMask, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_DigitalHandle",pwmPortHandle});
	parameters.push_back({"int32_t",squelchMask});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetPWMPeriodScale",parameters);
}

int32_t HAL_GetLoopTiming(int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetLoopTiming",parameters,c);
	return c.get();
}

HAL_RelayHandle HAL_InitializeRelayPort(HAL_PortHandle portHandle, HAL_Bool fwd, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"HAL_Bool",fwd});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_RelayHandle> c;
	callFunc("HAL_InitializeRelayPort",parameters,c);
	return c.get();
}

void HAL_FreeRelayPort(HAL_RelayHandle relayPortHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle",relayPortHandle});
	callFunc("HAL_FreeRelayPort",parameters);
}

HAL_Bool HAL_CheckRelayChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckRelayChannel",parameters,c);
	return c.get();
}

void HAL_SetRelay(HAL_RelayHandle relayPortHandle, HAL_Bool on, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle",relayPortHandle});
	parameters.push_back({"HAL_Bool",on});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetRelay",parameters);
}

HAL_Bool HAL_GetRelay(HAL_RelayHandle relayPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_RelayHandle",relayPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetRelay",parameters,c);
	return c.get();
}

void HAL_InitializeSerialPort(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializeSerialPort",parameters);
}

void HAL_SetSerialBaudRate(HAL_SerialPort port, int32_t baud, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",baud});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialBaudRate",parameters);
}

void HAL_SetSerialDataBits(HAL_SerialPort port, int32_t bits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",bits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialDataBits",parameters);
}

void HAL_SetSerialParity(HAL_SerialPort port, int32_t parity, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",parity});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialParity",parameters);
}

void HAL_SetSerialStopBits(HAL_SerialPort port, int32_t stopBits, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",stopBits});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialStopBits",parameters);
}

void HAL_SetSerialWriteMode(HAL_SerialPort port, int32_t mode, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",mode});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialWriteMode",parameters);
}

void HAL_SetSerialFlowControl(HAL_SerialPort port, int32_t flow, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",flow});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialFlowControl",parameters);
}

void HAL_SetSerialTimeout(HAL_SerialPort port, double timeout, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"double",timeout});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialTimeout",parameters);
}

void HAL_EnableSerialTermination(HAL_SerialPort port, char terminator, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"char",terminator});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_EnableSerialTermination",parameters);
}

void HAL_DisableSerialTermination(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_DisableSerialTermination",parameters);
}

void HAL_SetSerialReadBufferSize(HAL_SerialPort port, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialReadBufferSize",parameters);
}

void HAL_SetSerialWriteBufferSize(HAL_SerialPort port, int32_t size, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t",size});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSerialWriteBufferSize",parameters);
}

int32_t HAL_GetSerialBytesReceived(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetSerialBytesReceived",parameters,c);
	return c.get();
}

int32_t HAL_ReadSerial(HAL_SerialPort port, char* buffer, int32_t count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"char*",buffer});
	parameters.push_back({"int32_t",count});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_ReadSerial",parameters,c);
	return c.get();
}

int32_t HAL_WriteSerial(HAL_SerialPort port, const char* buffer, int32_t count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"const char*",buffer});
	parameters.push_back({"int32_t",count});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_WriteSerial",parameters,c);
	return c.get();
}

void HAL_FlushSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FlushSerial",parameters);
}

void HAL_ClearSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearSerial",parameters);
}

void HAL_CloseSerial(HAL_SerialPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SerialPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_CloseSerial",parameters);
}

HAL_SolenoidHandle HAL_InitializeSolenoidPort(HAL_PortHandle portHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_PortHandle",portHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_SolenoidHandle> c;
	callFunc("HAL_InitializeSolenoidPort",parameters,c);
	return c.get();
}

void HAL_FreeSolenoidPort(HAL_SolenoidHandle solenoidPortHandle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle",solenoidPortHandle});
	callFunc("HAL_FreeSolenoidPort",parameters);
}

HAL_Bool HAL_CheckSolenoidModule(int32_t module){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckSolenoidModule",parameters,c);
	return c.get();
}

HAL_Bool HAL_CheckSolenoidChannel(int32_t channel){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",channel});
	Channel<HAL_Bool> c;
	callFunc("HAL_CheckSolenoidChannel",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetSolenoid(HAL_SolenoidHandle solenoidPortHandle, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle",solenoidPortHandle});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetSolenoid",parameters,c);
	return c.get();
}

int32_t HAL_GetAllSolenoids(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetAllSolenoids",parameters,c);
	return c.get();
}

void HAL_SetSolenoid(HAL_SolenoidHandle solenoidPortHandle, HAL_Bool value, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SolenoidHandle",solenoidPortHandle});
	parameters.push_back({"HAL_Bool",value});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSolenoid",parameters);
}

void HAL_SetAllSolenoids(int32_t module, int32_t state, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t",state});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetAllSolenoids",parameters);
}

int32_t HAL_GetPCMSolenoidBlackList(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetPCMSolenoidBlackList",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetPCMSolenoidVoltageStickyFault(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetPCMSolenoidVoltageStickyFault",parameters,c);
	return c.get();
}

HAL_Bool HAL_GetPCMSolenoidVoltageFault(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_GetPCMSolenoidVoltageFault",parameters,c);
	return c.get();
}

void HAL_ClearAllPCMStickyFaults(int32_t module, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"int32_t",module});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ClearAllPCMStickyFaults",parameters);
}

void HAL_InitializeSPI(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_InitializeSPI",parameters);
}

int32_t HAL_TransactionSPI(HAL_SPIPort port, uint8_t* dataToSend, uint8_t* dataReceived, int32_t size){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"uint8_t*",dataToSend});
	parameters.push_back({"uint8_t*",dataReceived});
	parameters.push_back({"int32_t",size});
	Channel<int32_t> c;
	callFunc("HAL_TransactionSPI",parameters,c);
	return c.get();
}

int32_t HAL_WriteSPI(HAL_SPIPort port, uint8_t* dataToSend, int32_t sendSize){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"uint8_t*",dataToSend});
	parameters.push_back({"int32_t",sendSize});
	Channel<int32_t> c;
	callFunc("HAL_WriteSPI",parameters,c);
	return c.get();
}

int32_t HAL_ReadSPI(HAL_SPIPort port, uint8_t* buffer, int32_t count){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"uint8_t*",buffer});
	parameters.push_back({"int32_t",count});
	Channel<int32_t> c;
	callFunc("HAL_ReadSPI",parameters,c);
	return c.get();
}

void HAL_CloseSPI(HAL_SPIPort port){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	callFunc("HAL_CloseSPI",parameters);
}

void HAL_SetSPISpeed(HAL_SPIPort port, int32_t speed){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",speed});
	callFunc("HAL_SetSPISpeed",parameters);
}

void HAL_SetSPIOpts(HAL_SPIPort port, HAL_Bool msbFirst, HAL_Bool sampleOnTrailing, HAL_Bool clkIdleHigh){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"HAL_Bool",msbFirst});
	parameters.push_back({"HAL_Bool",sampleOnTrailing});
	parameters.push_back({"HAL_Bool",clkIdleHigh});
	callFunc("HAL_SetSPIOpts",parameters);
}

void HAL_SetSPIChipSelectActiveHigh(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSPIChipSelectActiveHigh",parameters);
}

void HAL_SetSPIChipSelectActiveLow(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSPIChipSelectActiveLow",parameters);
}

int32_t HAL_GetSPIHandle(HAL_SPIPort port){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	Channel<int32_t> c;
	callFunc("HAL_GetSPIHandle",parameters,c);
	return c.get();
}

void HAL_SetSPIHandle(HAL_SPIPort port, int32_t handle){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",handle});
	callFunc("HAL_SetSPIHandle",parameters);
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
	callFunc("HAL_InitSPIAccumulator",parameters);
}

void HAL_FreeSPIAccumulator(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_FreeSPIAccumulator",parameters);
}

void HAL_ResetSPIAccumulator(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_ResetSPIAccumulator",parameters);
}

void HAL_SetSPIAccumulatorCenter(HAL_SPIPort port, int32_t center, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",center});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSPIAccumulatorCenter",parameters);
}

void HAL_SetSPIAccumulatorDeadband(HAL_SPIPort port, int32_t deadband, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t",deadband});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_SetSPIAccumulatorDeadband",parameters);
}

int32_t HAL_GetSPIAccumulatorLastValue(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetSPIAccumulatorLastValue",parameters,c);
	return c.get();
}

int64_t HAL_GetSPIAccumulatorValue(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetSPIAccumulatorValue",parameters,c);
	return c.get();
}

int64_t HAL_GetSPIAccumulatorCount(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<int64_t> c;
	callFunc("HAL_GetSPIAccumulatorCount",parameters,c);
	return c.get();
}

double HAL_GetSPIAccumulatorAverage(HAL_SPIPort port, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int32_t*",status});
	Channel<double> c;
	callFunc("HAL_GetSPIAccumulatorAverage",parameters,c);
	return c.get();
}

void HAL_GetSPIAccumulatorOutput(HAL_SPIPort port, int64_t* value, int64_t* count, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_SPIPort",port});
	parameters.push_back({"int64_t*",value});
	parameters.push_back({"int64_t*",count});
	parameters.push_back({"int32_t*",status});
	callFunc("HAL_GetSPIAccumulatorOutput",parameters);
}

int32_t HAL_GetThreadPriority(NativeThreadHandle handle, HAL_Bool* isRealTime, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"NativeThreadHandle",handle});
	parameters.push_back({"HAL_Bool*",isRealTime});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetThreadPriority",parameters,c);
	return c.get();
}

int32_t HAL_GetCurrentThreadPriority(HAL_Bool* isRealTime, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool*",isRealTime});
	parameters.push_back({"int32_t*",status});
	Channel<int32_t> c;
	callFunc("HAL_GetCurrentThreadPriority",parameters,c);
	return c.get();
}

HAL_Bool HAL_SetThreadPriority(NativeThreadHandle handle, HAL_Bool realTime, int32_t priority, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"NativeThreadHandle",handle});
	parameters.push_back({"HAL_Bool",realTime});
	parameters.push_back({"int32_t",priority});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_SetThreadPriority",parameters,c);
	return c.get();
}

HAL_Bool HAL_SetCurrentThreadPriority(HAL_Bool realTime, int32_t priority, int32_t* status){
	std::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;
	parameters.push_back({"HAL_Bool",realTime});
	parameters.push_back({"int32_t",priority});
	parameters.push_back({"int32_t*",status});
	Channel<HAL_Bool> c;
	callFunc("HAL_SetCurrentThreadPriority",parameters,c);
	return c.get();
}

#ifdef __cplusplus
}
#endif
