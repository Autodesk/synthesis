#include "testing.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(EncoderTest, Increment){
    constexpr unsigned PORT_A = 2, PORT_B = 3;

    auto instance = hel::RoboRIOManager::getInstance();

    instance.first->encoder_managers[0] = hel::EncoderManager{
        PORT_A, hel::EncoderManager::PortType::DI,
        PORT_B, hel::EncoderManager::PortType::DI
    };

    frc::Encoder encoder = {PORT_A, PORT_B};

    instance.first->encoder_managers[0].get().setTicks(100);
    instance.first->encoder_managers[0].get().update();

    EXPECT_EQ(instance.first->encoder_managers[0].get().getTicks(), encoder.GetRaw());
    instance.second.unlock();
}

TEST(EncoderTest, Incrementk1X){
    constexpr unsigned PORT_A = 12, PORT_B = 2;

    auto instance = hel::RoboRIOManager::getInstance();

    instance.first->encoder_managers[1] = hel::EncoderManager{
        PORT_A, hel::EncoderManager::PortType::DI,
        PORT_B, hel::EncoderManager::PortType::DI
    };

    frc::Encoder encoder = {PORT_A, PORT_B, false, frc::Encoder::EncodingType::k1X};

    instance.first->encoder_managers[1].get().setTicks(-100);
    instance.first->encoder_managers[1].get().update();

    EXPECT_EQ(instance.first->encoder_managers[1].get().getTicks(), encoder.GetRaw());
    instance.second.unlock();
}
