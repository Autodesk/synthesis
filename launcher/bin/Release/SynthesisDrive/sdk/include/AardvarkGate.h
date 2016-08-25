#pragma once

#include <cstdint>
#include <cstring> // memcpy
#include <HAL\Accelerometer.hpp>

// annotation to specify if the
// information should be exported
// or imported
#define __IN__
#define __OUT__

typedef struct gateport_t {
	uint8_t pin;
	uint8_t module;
} GatePort;

class AardvarkGate {
public:
	static AardvarkGate* m_instance;
	AardvarkGate() {}
	AardvarkGate(AardvarkGate& other) { copy(&other, this); }
	explicit AardvarkGate(AardvarkGate* other) { copy(other, this); }
	~AardvarkGate() {}

	static const size_t kNumAnaOutputs = 16;
	static const size_t kNumAnaInputs = 4;
	static const size_t kNumDigPwms = 20;
	static const size_t kNumDigRelays = 8;
	static const size_t kNumDigDIOs = 26;
	static const size_t kNumDigCounters = kNumDigDIOs;
	static const size_t kNumDigEncoders = kNumDigDIOs;
	static const size_t kNumSolenoids = 10;

	inline static void copy(AardvarkGate* src, AardvarkGate* dst) {
		memcpy(dst, src, sizeof(AardvarkGate));
	}

	// accelerometer
	__OUT__ bool m_accActive;
	__OUT__ AccelerometerRange m_accRange;
	__IN__ double m_accX;
	__IN__ double m_accY;
	__IN__ double m_accZ;

	// analog
	__OUT__ int16_t m_anaOutputValue[kNumAnaOutputs];
	__IN__ int64_t m_anaOutputAccumulator[kNumAnaOutputs];
	__OUT__ int64_t m_anaOutputAccumulatorCenter[kNumAnaOutputs];
	__IN__ uint32_t m_anaOutputAccumulatorCount[kNumAnaOutputs];
	__IN__ int32_t m_anaOutputAverageValue[kNumAnaOutputs];
	__OUT__ uint32_t m_anaOutputAverageBits[kNumAnaOutputs];
	__OUT__ uint32_t m_anaOutputOversampleBits[kNumAnaOutputs];

	// digital
	__OUT__ uint16_t m_digPwm[kNumDigPwms];
	__OUT__ bool m_digRelayForward[kNumDigRelays];
	__OUT__ bool m_digRelayReverse[kNumDigRelays];
	__OUT__ bool m_digIO[kNumDigDIOs];

	// counter
	__IN__ int32_t m_digCounter[kNumDigCounters];
	__IN__ bool m_digCounterStopped[kNumDigCounters];
	__IN__ bool m_digCounterDirection[kNumDigCounters];
	__OUT__ bool m_digCounterReverse[kNumDigCounters];

	// encoder
	__IN__ int32_t m_digEncoder[kNumDigEncoders];
	__IN__ bool m_digEncoderStopped[kNumDigEncoders];
	__IN__ bool m_digEncoderDirection[kNumDigEncoders];
	__OUT__ bool m_digEncoderReverse[kNumDigEncoders];

	// solenoid
	__OUT__ bool m_pcmSolenoid[kNumSolenoids];
};

inline AardvarkGate* GetGate() {
	if (AardvarkGate::m_instance == nullptr) AardvarkGate::m_instance = new AardvarkGate;
	return AardvarkGate::m_instance;
}

class AardvarkStation {
public:
	static AardvarkStation* m_instance;
	AardvarkStation() {}
	AardvarkStation(const AardvarkStation& other) { copy(&other, this); }
	explicit AardvarkStation(AardvarkStation* other) { copy(other, this); }
	~AardvarkStation() {}

	inline static void copy(AardvarkStation* src, AardvarkStation* dst) {
		memcpy(dst, src, sizeof(AardvarkStation));
	}

	uint8_t m_axis[4][6];
	uint16_t m_buttons[4];
	uint8_t m_pov[4][2];

	bool m_enabled;
	uint8_t m_state; // teleop, auto, test
};

inline AardvarkStation* GetStation() {
	if (AardvarkStation::m_instance == nullptr) AardvarkStation::m_instance = new AardvarkStation;
	return AardvarkStation::m_instance;
}
