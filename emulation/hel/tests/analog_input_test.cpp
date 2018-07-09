#include "gtest/gtest.h"
#include "roborio.h"

TEST(AnalogInputTest, ReadWriteConfig) {
    auto value = hal::tAI::tConfig{};
    value.ScanSize = 3;
    value.ConvertRate = 65536;
    hel::RoboRIOManager::getInstance()->analog_inputs.setConfig(value);

    EXPECT_EQ(65536u, hel::RoboRIOManager::getInstance()->analog_inputs.getConfig().ConvertRate);
}
