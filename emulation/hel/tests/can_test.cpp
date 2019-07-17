#include "testing.hpp"
//#include "ctre/Phoenix.h"
#include <cmath>
#include "FRC_NetworkCommunication/CANSessionMux.h"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

std::vector<uint8_t> generateCTREPercentOutputData(double percent_output)noexcept{
    std::vector<uint8_t> data(hel::ctre::CANMotorController::MessageData::SIZE);
    uint32_t percent_output_int = std::fabs(percent_output) * 0x0400;

    // Divide percent_output_int among the bytes as expected by CTRE's CAN protocol
    data[1] = (percent_output_int >> 8) & 0xFF;
    data[2] = percent_output_int & 0xFF;

    if(percent_output < 0.0){
        data[0] = 0xFF;
        data[1] = 0xFF - data[1];
        data[2] = 0xFF - data[2];
    }
    return data;
}

std::vector<uint8_t> generateREVPercentOutputData(double percent_output)noexcept{
    std::vector<uint8_t> data(8);
    float power = percent_output;
    std::memcpy(data.data(), &power, sizeof(float));
    return data;
}

void printControllers(std::map<unsigned, std::shared_ptr<hel::CANMotorControllerBase>> can_motor_controllers){
    std::cout << "can_motor_controllers:" << asString(can_motor_controllers, [](auto a){ return a.second->toString(); }) << "\n";
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

TEST(CANTest, setCTREPercentOutput){
    const unsigned DEVICE_ID = 3;

    int32_t id = hel::CANMessageID::generate(
        hel::CANMessageID::Type::TALON_SRX,
        hel::CANMessageID::Manufacturer::CTRE,
        0,
        DEVICE_ID
    );
    double percent_output = 0.6;
    std::vector<uint8_t> data = generateCTREPercentOutputData(percent_output);

    FRC_NetworkCommunication_CANSessionMux_sendMessage(id, data.data(), data.size(), 0, nullptr);

    auto instance = hel::RoboRIOManager::getInstance();
    printControllers(instance.first->can_motor_controllers);
    EXPECT_NEAR(instance.first->can_motor_controllers[DEVICE_ID]->getPercentOutput(), percent_output, EPSILON);

    instance.second.unlock();
}

TEST(CANTest, setREVPercentOutput){
    const unsigned DEVICE_ID = 5;

    int32_t id = hel::CANMessageID::generate(
        hel::CANMessageID::Type::SPARK_MAX,
        hel::CANMessageID::Manufacturer::REV,
        hel::rev::CANMotorController::CommandAPIID::FIRMWARE,
        DEVICE_ID
    );
    double percent_output = -0.3;
    std::vector<uint8_t> data;

    FRC_NetworkCommunication_CANSessionMux_sendMessage(id, data.data(), data.size(), 0, nullptr);

    id = hel::CANMessageID::generate(
        hel::CANMessageID::Type::SPARK_MAX,
        hel::CANMessageID::Manufacturer::REV,
        hel::rev::CANMotorController::CommandAPIID::DC_SET,
        DEVICE_ID
    );
    data = generateREVPercentOutputData(percent_output);

    FRC_NetworkCommunication_CANSessionMux_sendMessage(id, data.data(), data.size(), 0, nullptr);

    auto instance = hel::RoboRIOManager::getInstance();
    printControllers(instance.first->can_motor_controllers);
    EXPECT_NEAR(instance.first->can_motor_controllers[DEVICE_ID]->getPercentOutput(), percent_output, EPSILON);

    instance.second.unlock();
}
