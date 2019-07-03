#include "testing.hpp"
//#include "ctre/Phoenix.h"
#include "roborio_manager.hpp"
#include <cmath>
#include "FRC_NetworkCommunication/CANSessionMux.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

std::vector<uint8_t> generateCTREPercentOutputData(double percent_output)noexcept{
    std::vector<uint8_t> data(hel::ctre::CANMotorController::MessageData::SIZE);
    uint32_t percent_output_int = std::fabs(percent_output * 256 * 256 * 4);

    //divide percent_output_int among the bytes as expected by CTRE's CAN protocol
    data[1] = percent_output_int / (256*256);
    percent_output_int %= 256 * 256;
    data[2] = percent_output_int / 256;
    percent_output_int %= 256;
    data[3] = percent_output_int;

    if(percent_output < 0.0){//format as 2's compliment
        data[0] = 255;
        data[1] = 255 - data[1];
        data[2] = 255 - data[2];
        data[3] = 255 - data[3];
    }
    return data;
}

void printControllers(std::map<unsigned, std::shared_ptr<hel::CANMotorControllerBase>> can_motor_controllers){
    std::cout << "can_motor_controllers:[";
    for(const std::pair<unsigned, std::shared_ptr<hel::CANMotorControllerBase>>& a: can_motor_controllers){
        std::cout << a.second->toString() << ", ";
    }
    std::cout << "]\n";
}

double round(double a, int digits){
    return std::round(a * std::pow(10, digits)) / std::pow(10, digits);
}

TEST(CANTest, IDs){
    auto instance = hel::RoboRIOManager::getInstance();
    //ctre::phoenix::motorcontrol::can::WPI_TalonSRX talon = {1};
    printControllers(instance.first->can_motor_controllers);
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
    hel::ctre::CANMotorController a = {hel::CANMessageID::parse(33816705)};

    EXPECT_EQ(a.getID(), 1);
    double percent_output = 1.0;
    {
        a.setPercentOutput(percent_output);
        a.parseCANPacket(0, generateCTREPercentOutputData(a.getPercentOutput()));

        std::cout << a.toString() << "\n";
        EXPECT_DOUBLE_EQ(round(a.getPercentOutput(), 3), round(percent_output, 3));
    }
    {
        percent_output = -0.5;
        a.setPercentOutput(percent_output);
        a.parseCANPacket(0, generateCTREPercentOutputData(a.getPercentOutput()));

        std::cout << a.toString() << "\n";
        EXPECT_DOUBLE_EQ(round(a.getPercentOutput(), 3), round(percent_output, 3));
    }
    {
        percent_output = 0.726132;
        a.setPercentOutput(percent_output);
        a.parseCANPacket(0, generateCTREPercentOutputData(a.getPercentOutput()));

        std::cout << a.toString() << "\n";
        EXPECT_DOUBLE_EQ(round(a.getPercentOutput(), 3), round(percent_output, 3));
    }
}

TEST(CANTest, sendMessage){
    int32_t id = hel::CANMessageID::generate(hel::CANMessageID::Type::TALON_SRX, hel::CANMessageID::Manufacturer::CTRE, 0, 3);
    double percent_output = 0.6;
    std::vector<uint8_t> data(hel::ctre::CANMotorController::MessageData::SIZE);

    FRC_NetworkCommunication_CANSessionMux_sendMessage(id, data.data(), data.size(), 0, nullptr);

    data = generateCTREPercentOutputData(percent_output);
    data[hel::ctre::CANMotorController::MessageData::COMMAND_BYTE] = 0b100000;

    FRC_NetworkCommunication_CANSessionMux_sendMessage(id, data.data(), data.size(), 0, nullptr);

    auto instance = hel::RoboRIOManager::getInstance();
    printControllers(instance.first->can_motor_controllers);
    EXPECT_DOUBLE_EQ(round(instance.first->can_motor_controllers[3]->getPercentOutput(), 3), round(percent_output, 3));

    instance.second.unlock();
}
