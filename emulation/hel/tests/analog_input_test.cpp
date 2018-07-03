#include "gtest/gtest.h"

#include "roborio.h"

TEST(AnalogInputTest, ReadWriteConfig) {
    auto value = hal::tAI::tConfig{};
    value.ScanSize = 3;
    value.ConvertRate = 65536;
    cerebrum::roborio_state.analog_inputs.setConfig(value);

    EXPECT_EQ(65536u, cerebrum::roborio_state.analog_inputs.getConfig().ConvertRate);
}

int main(int argc, char **argv) {
    testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}
