#include "gtest/gtest.h"
#include "WPILib.h"
#include "roborio_manager.hpp"
#include <iostream>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(EncoderTest, Increment){
    auto instance = hel::RoboRIOManager::getInstance();
    {
        frc::Encoder encoder = {0,11,false,frc::Encoder::EncodingType::k4X};
        instance.first->encoder_managers[0] = {0,hel::Encoder::PortType::DI,11,hel::Encoder::PortType::DI};
        for(unsigned i = 0; i < 100; i++){
            instance.first->encoder_managers[0].setTicks(i);
            instance.first->encoder_managers[0].update();

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[0].getTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[0].getTicks(), encoder.GetRaw());
    }
    {
        frc::Encoder encoder = {12,2,false,frc::Encoder::EncodingType::k1X};
        instance.first->encoder_managers[1] = {12,hel::Encoder::PortType::DI,2,hel::Encoder::PortType::DI};
        for(int i = 0; i > -100; i--){
            instance.first->encoder_managers[1].setTicks(i);
            instance.first->encoder_managers[1].update();

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[1].getTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[1].getTicks(), encoder.GetRaw());
    }
    instance.second.unlock();
}
