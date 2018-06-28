#ifndef CONSTANTS_H
#define CONSTANTS_H

//These are defined by HAL and NI FPGA
namespace minerva::constants{
	namespace NI_FPGA{//TODO scoop all NI FPGA constants
		namespace PWM{
      		constexpr int kNumSystems = 1;
			constexpr int kNumPeriodScaleMXPElements = 10;
			constexpr int kNumPeriodScaleHdrElements = 10;
			constexpr int NumZeroLatchElements = 20;
			constexpr int kNumHdrRegisters = 10;
			constexpr int kNumMXPRegisters = 10;
		}
	}

	namespace HAL{//TODO rename to lowercase for consistency?
		//From athena PortsInternal.h
		constexpr int32_t kNumAccumulators = 2;
		constexpr int32_t kNumAnalogTriggers = 8;
		constexpr int32_t kNumAnalogInputs = 8;
		constexpr int32_t kNumAnalogOutputs = 2;
		constexpr int32_t kNumCounters = 8;
		constexpr int32_t kNumDigitalHeaders = 10;
		constexpr int32_t kNumDigitalMXPChannels = 16;
		constexpr int32_t kNumDigitalSPIPortChannels = 5;
		constexpr int32_t kNumPWMHeaders = 10;
		constexpr int32_t kNumDigitalChannels = kNumDigitalHeaders + kNumDigitalMXPChannels + kNumDigitalSPIPortChannels;
		constexpr int32_t kNumPWMChannels = 10 + kNumPWMHeaders;
		constexpr int32_t kNumDigitalPWMOutputs = 4 + 2;
		constexpr int32_t kNumEncoders = 8;
		constexpr int32_t kNumInterrupts = 8;
		constexpr int32_t kNumRelayChannels = 8;
		constexpr int32_t kNumRelayHeaders = kNumRelayChannels / 2;
		constexpr int32_t kNumPCMModules = 63;
		constexpr int32_t kNumSolenoidChannels = 8;
		constexpr int32_t kNumPDPModules = 63;
		constexpr int32_t kNumPDPChannels = 16;
	
		//From athena DigitalInternal.h
		constexpr int32_t kMXPDigitalPWMOffset = 6;
		constexpr int32_t kExpectedLoopTiming = 40;
		constexpr double kDefaultPwmPeriod = 5.05;
		constexpr double kDefaultPwmCenter = 1.5;
		constexpr int32_t kDefaultPwmStepsDown = 1000;
		constexpr int32_t kPwmDisabled = 0;

		//From athena HandlesInternal.h
		constexpr int16_t InvalidHandleIndex = -1;

		//From athena ConstantsInternal.h
		constexpr int32_t kSystemClockTicksPerMicrosecond = 40;
	}
}

#endif

