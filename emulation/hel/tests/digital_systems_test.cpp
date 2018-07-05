#include "gtest/gtest.h"
#include "roborio.h"
#include <iostream>
using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(DigitalSystemsTest, SetHeaderDO) {
	tDIO* a = tDIO::create(nullptr);
	a->writeOutputEnable_Headers(1u << 3, nullptr);
	a->writeDO_Headers(1u << 3, nullptr);

    EXPECT_EQ(1 << 3, hel::RoboRIOManager::getInstance()->digital_system.getOutputs().Headers);
}
