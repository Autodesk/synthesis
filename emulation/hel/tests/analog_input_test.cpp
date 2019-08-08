#include "testing.hpp"

TEST(AnalogInputTest, GetValue){
	constexpr int PORT = 0;

	std::vector<int32_t> values = {5, 6, 7};

	auto instance = hel::RoboRIOManager::getInstance();
	instance.first->analog_inputs.setValues(PORT, values);
	instance.second.unlock();

	frc::AnalogInput ai = frc::AnalogInput{PORT};

	EXPECT_EQ(values.back(), ai.GetValue());
}

TEST(AnalogInputTest, GetVoltage){
	constexpr int PORT = 0;

	std::vector<int32_t> values = {5, 6, 7};

	auto instance = hel::RoboRIOManager::getInstance();
	instance.first->analog_inputs.setValues(PORT, values);
	instance.second.unlock();

	frc::AnalogInput ai = frc::AnalogInput{PORT};

	EXPECT_EQ(values.back(), ai.GetVoltage());

}
