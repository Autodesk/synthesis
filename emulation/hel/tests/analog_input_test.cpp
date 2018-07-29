#include "gtest/gtest.h"
#include "roborio.hpp"

TEST(AnalogInputTest, ReadWriteConfig) {
    auto value = nFPGA::nRoboRIO_FPGANamespace::tAI::tConfig{};
    value.ScanSize = 3;
    value.ConvertRate = 65536;
    auto instance = hel::RoboRIOManager::getInstance();
    instance.first->analog_inputs.setConfig(value);

    EXPECT_EQ(65536u, instance.first->analog_inputs.getConfig().ConvertRate);
    instance.second.unlock();
}
