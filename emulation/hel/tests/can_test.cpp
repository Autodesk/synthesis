#include "testing.hpp"
//#include "ctre/Phoenix.h"
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

TEST(CANTest, IDs){
    auto instance = hel::RoboRIOManager::getInstance();
    //ctre::phoenix::motorcontrol::can::WPI_TalonSRX talon = {1};
    printControllers(instance.first->can_motor_controllers);
    EXPECT_EQ(1, 1); //TODO
    instance.second.unlock();
}

TEST(CANTest, convertPercentOutputData){
    hel::ctre::CANMotorController a = {hel::CANMessageID::parse(33816705)};

    EXPECT_EQ(a.getID(), 1);

    double percent_output = 1.0;
    a.parseCANPacket(0, generateCTREPercentOutputData(percent_output));
    std::cout << a.toString() << "\n";
    EXPECT_NEAR(a.getPercentOutput(), percent_output, EPSILON);

    percent_output = -0.5;
    a.parseCANPacket(0, generateCTREPercentOutputData(percent_output));
    std::cout << a.toString() << "\n";
    EXPECT_NEAR(a.getPercentOutput(), percent_output, EPSILON);

    percent_output = 0.726132;
    a.parseCANPacket(0, generateCTREPercentOutputData(percent_output));
    std::cout << a.toString() << "\n";
    EXPECT_NEAR(a.getPercentOutput(), percent_output, EPSILON);
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
    EXPECT_NEAR(instance.first->can_motor_controllers[3]->getPercentOutput(), percent_output, EPSILON);

    instance.second.unlock();
}
