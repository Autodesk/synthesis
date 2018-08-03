#include "gtest/gtest.h"
#include "WPILib.h"
#include "roborio_manager.hpp"
#include <iostream>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(EncoderTest, Increment){
    auto instance = hel::RoboRIOManager::getInstance();
    {
        instance.first->encoder_managers[0] = hel::Maybe<hel::EncoderManager>({0,hel::EncoderManager::PortType::DI,11,hel::EncoderManager::PortType::DI});
        frc::Encoder encoder = {0,11,false,frc::Encoder::EncodingType::k4X};
        for(unsigned i = 0; i < 100; i++){
            instance.first->encoder_managers[0].get().setTicks(i);
            instance.first->encoder_managers[0].get().update();

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[0].get().getTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[0].get().getTicks(), encoder.GetRaw());
    }
    {
        instance.first->encoder_managers[1] = hel::Maybe<hel::EncoderManager>({12,hel::EncoderManager::PortType::DI,2,hel::EncoderManager::PortType::DI});
        frc::Encoder encoder = {12,2,false,frc::Encoder::EncodingType::k1X};
        for(int i = 0; i > -100; i--){
            instance.first->encoder_managers[1].get().setTicks(i);
            instance.first->encoder_managers[1].get().update();

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[1].get().getTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[1].get().getTicks(), encoder.GetRaw());
    }
    instance.second.unlock();
}
