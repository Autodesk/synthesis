#include "gtest/gtest.h"
#include "WPILib.h"
#include "ctre/Phoenix.h"
#include "roborio.hpp"
#include <iostream>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(UtilTest, findMostSignificantBit){
    uint32_t a = 33816705;

    EXPECT_EQ(26, hel::findMostSignificantBit(a));
}

TEST(UtilTest, checkBits){
    uint32_t a = 0b11000;
    uint32_t b = 0b11000;
    uint32_t c = 0b01001;
    uint32_t comparison_mask_1 = 0b11111;
    uint32_t base_talon = 0x02040000;
    uint32_t send_talon = 33816705;
    uint32_t comparison_mask_2 = 0b11000001000000000000000000;

    EXPECT_EQ(true, hel::compareBits(a,b,comparison_mask_1));
    EXPECT_EQ(false, hel::compareBits(a,c,comparison_mask_1));
    EXPECT_EQ(true, hel::compareBits(send_talon,base_talon,comparison_mask_2));
}
