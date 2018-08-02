#include "gtest/gtest.h"
#include "WPILib.h"
#include "roborio_manager.hpp"
#include <iostream>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(EncoderTest, Increment){
    auto instance = hel::RoboRIOManager::getInstance();
    {
        instance.first->encoder_managers[0] = {0,hel::EncoderManager::PortType::DI,11,hel::EncoderManager::PortType::DI};
        frc::Encoder encoder = {0,11,false,frc::Encoder::EncodingType::k4X};
        for(unsigned i = 0; i < 100; i++){
            instance.first->encoder_managers[0].setRawTicks(i);
            instance.first->encoder_managers[0].update();

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[0].getCurrentTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[0].getCurrentTicks(), encoder.GetRaw());
    }
    {
        instance.first->encoder_managers[1] = {12,hel::EncoderManager::PortType::DI,2,hel::EncoderManager::PortType::DI};
        frc::Encoder encoder = {12,2,false,frc::Encoder::EncodingType::k1X};
        for(int i = 0; i > -100; i--){
            instance.first->encoder_managers[1].setRawTicks(i);
            instance.first->encoder_managers[1].update();

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[1].getCurrentTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[1].getCurrentTicks(), encoder.GetRaw());
    }
    {
        instance.first->encoder_managers[1] = {6,hel::EncoderManager::PortType::DI,5,hel::EncoderManager::PortType::DI};
        frc::Encoder encoder = {6,5,false,frc::Encoder::EncodingType::k4X};
        for(int i = 0; i < 100; i++){
            if(i == 50){
                instance.first->encoder_managers[1].reset();
            }

            instance.first->encoder_managers[1].setRawTicks(i);
            instance.first->encoder_managers[1].update();

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[1].getCurrentTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[1].getCurrentTicks(), encoder.GetRaw());
    }
    instance.second.unlock();
}
