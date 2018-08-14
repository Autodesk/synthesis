#include "gtest/gtest.h"
#include "frc/WPILib.h"
#include "roborio_manager.hpp"
#include <iostream>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(EncoderTest, Increment){
    auto instance = hel::RoboRIOManager::getInstance();
    {
        std::string in = "{\"a_channel\":2,\"a_type\":\"DI\",\"b_channel\":3,\"b_type\":\"DI\",\"ticks\":-300}";
        instance.first->encoder_managers[0] = hel::EncoderManager::deserialize(in);//hel::EncoderManager{2,hel::EncoderManager::PortType::DI,3,hel::EncoderManager::PortType::DI};
        frc::Encoder encoder = {2,3};
        for(unsigned i = 0; i < 100; i++){
            instance.first->encoder_managers[0].get().setTicks(i);
            instance.first->encoder_managers[0].get().update();
            std::cout<<instance.first->encoder_managers[0].get().toString()<<"\n";

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[0].get().getTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[0].get().getTicks(), encoder.GetRaw());
    }
    {
        frc::Encoder encoder = {12,2,false,frc::Encoder::EncodingType::k1X};
        instance.first->encoder_managers[1] = hel::EncoderManager{12,hel::EncoderManager::PortType::DI,2,hel::EncoderManager::PortType::DI};
        for(int i = 0; i > -100; i--){
            instance.first->encoder_managers[1].get().setTicks(i);
            instance.first->encoder_managers[1].get().update();

            std::cout<<"HEL encoder count: "<<instance.first->encoder_managers[1].get().getTicks()<<" WPILib count raw:"<<encoder.GetRaw()<<" count:"<<encoder.Get()<<"\n";
        }
        EXPECT_EQ(instance.first->encoder_managers[1].get().getTicks(), encoder.GetRaw());
    }
    instance.second.unlock();
}
