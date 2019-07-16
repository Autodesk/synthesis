#include "testing.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(DigitalSystemsTest, SetHeaderDO) {
    tDIO* a = tDIO::create(nullptr);
    a->writeOutputEnable_Headers(1u << 3, nullptr);
    a->writeDO_Headers(1u << 3, nullptr);

    auto instance = hel::RoboRIOManager::getInstance();
    EXPECT_EQ(1u << 3, instance.first->digital_system.getOutputs().Headers);
    instance.second.unlock();
}
