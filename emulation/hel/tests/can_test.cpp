#include "gtest/gtest.h"
#include "frc/WPILib.h"
//#include "ctre/Phoenix.h"
#include "roborio_manager.hpp"
#include <iostream>
#include <cmath>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(CANTest, IDs){
    auto instance = hel::RoboRIOManager::getInstance();
    //ctre::phoenix::motorcontrol::can::WPI_TalonSRX talon = {1};
    auto can_motor_controllers = instance.first->can_motor_controllers;
    std::cout<<"can_motor_controllers:" + hel::asString(can_motor_controllers, std::function<std::string(std::pair<uint32_t,hel::CANMotorController>)>([&](std::pair<uint32_t, hel::CANMotorController> a){ return "[" + std::to_string(a.first) + ", " + a.second.toString() + "]";}))<<"\n";
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

TEST(CANTest, convertPercentOutputData){
    hel::CANMotorController a = {33816705};
    double percent_output = 1.0;
    {
        a.setPercentOutput(percent_output);
        a.setPercentOutputData(a.getPercentOutputData());

        EXPECT_DOUBLE_EQ(std::round(a.getPercentOutput() * 1000) / 1000, std::round(percent_output * 1000) / 1000);
    }
    {
        percent_output = -0.5;
        a.setPercentOutput(percent_output);
        a.setPercentOutputData(a.getPercentOutputData());

        EXPECT_DOUBLE_EQ(std::round(a.getPercentOutput() * 1000) / 1000, std::round(percent_output * 1000) / 1000);
    }
    {
        percent_output = 0.726132;
        a.setPercentOutput(percent_output);
        a.setPercentOutputData(a.getPercentOutputData());

        EXPECT_DOUBLE_EQ(std::round(a.getPercentOutput() * 1000) / 1000, std::round(percent_output * 1000) / 1000);
    }
}
