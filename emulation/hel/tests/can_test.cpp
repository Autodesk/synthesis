#include "gtest/gtest.h"
#include "WPILib.h"
#include "ctre/Phoenix.h"
#include "roborio_manager.hpp"
#include <iostream>
#include <cmath>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(CANTest, IDs){
    auto instance = hel::RoboRIOManager::getInstance();
    ctre::phoenix::motorcontrol::can::WPI_TalonSRX talon = {1};
    EXPECT_EQ(1, 1); //TODO
    instance.second.unlock();
}

TEST(CANTest, checkBits){
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

TEST(CANTest, speedData){
    hel::CANMotorController a = {33816705};
    double speed = 1.0;
    {
        a.setSpeed(speed);
        a.setSpeedData(a.getSpeedData());

        EXPECT_DOUBLE_EQ(std::round(a.getSpeed() * 1000) / 1000, std::round(speed * 1000) / 1000);
    }
    {
        speed = -0.5;
        a.setSpeed(speed);
        a.setSpeedData(a.getSpeedData());

        EXPECT_DOUBLE_EQ(std::round(a.getSpeed() * 1000) / 1000, std::round(speed * 1000) / 1000);
    }
    {
        speed = 0.726132;
        a.setSpeed(speed);
        a.setSpeedData(a.getSpeedData());

        EXPECT_DOUBLE_EQ(std::round(a.getSpeed() * 1000) / 1000, std::round(speed * 1000) / 1000);
    }
}
