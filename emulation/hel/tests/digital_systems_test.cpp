#include "testing.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

// NOTE: when HAL frees MXP DIO ports, it enables their special functions

TEST(DigitalSystemsTest, DigitalInputTest){
	constexpr bool IN_VAL = true;
	for(unsigned i = 0; i < hel::DigitalSystem::NUM_DIGITAL_HEADERS; i++){
		frc::DigitalInput in = frc::DigitalInput(i);

		tDIO::tDI di;
		di.Headers = hel::setBit(di.Headers, IN_VAL, i);

		auto instance = hel::RoboRIOManager::getInstance();
		instance.first->digital_system.setInputs(di);
		instance.second.unlock();

		EXPECT_EQ(IN_VAL, in.Get());
	}
	for(unsigned i = 0; i < hel::DigitalSystem::NUM_DIGITAL_MXP_CHANNELS; i++){
		frc::DigitalInput in = frc::DigitalInput(hel::DigitalSystem::NUM_DIGITAL_HEADERS + i);

		tDIO::tDI di;
		di.MXP = hel::setBit(di.MXP, IN_VAL, i);

		auto instance = hel::RoboRIOManager::getInstance();
		instance.first->digital_system.setInputs(di);
		instance.second.unlock();

		EXPECT_EQ(IN_VAL, in.Get());
	}
}

TEST(DigitalSystemsTest, DigitalOutputTest){
	constexpr bool OUT_VAL = true;
	for(unsigned i = 0; i < hel::DigitalSystem::NUM_DIGITAL_HEADERS; i++){
		frc::DigitalOutput out = frc::DigitalOutput(i);
		out.Set(OUT_VAL);

		auto instance = hel::RoboRIOManager::getInstance();

		EXPECT_EQ(OUT_VAL, hel::checkBitHigh(instance.first->digital_system.getOutputs().Headers, i)); // NOTE: WPILib DigitalOutput Get() returns the input value, not output value

		instance.second.unlock();
	}
	for(unsigned i = 0; i < hel::DigitalSystem::NUM_DIGITAL_MXP_CHANNELS; i++){
		frc::DigitalOutput out = frc::DigitalOutput(hel::DigitalSystem::NUM_DIGITAL_HEADERS + i);
		out.Set(OUT_VAL);

		auto instance = hel::RoboRIOManager::getInstance();

		EXPECT_EQ(OUT_VAL, hel::checkBitHigh(instance.first->digital_system.getOutputs().MXP, i));

		instance.second.unlock();
	}
}

TEST(DigitalSystemsTest, DigitalPulseTest){
	double length = 0.00025; // sec
	uint64_t timeout = 5 * 1E6; // microseconds
	constexpr unsigned I = 0;

	frc::DigitalOutput out = frc::DigitalOutput(I);

	uint64_t start = hel::Global::getCurrentTime();
	uint64_t timeout_start = start;

	out.Pulse(length);

	while(!out.IsPulsing()){ // Wait for pulse to start
		start = hel::Global::getCurrentTime();
		ASSERT_LT(start - timeout_start, timeout);
	}

	uint64_t end = 0;
	while(out.IsPulsing()){ // Wait for pulse to end
		end = hel::Global::getCurrentTime();
		ASSERT_LT(end - start, timeout);
	}

	double duration = (double)(end - start) / 1E6;
	EXPECT_NEAR(length, duration, 0.05);
}
