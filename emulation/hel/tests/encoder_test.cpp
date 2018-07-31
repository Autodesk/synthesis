#include "gtest/gtest.h"
#include "WPILib.h"
#include "roborio_manager.hpp"
#include <iostream>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(EncoderTest, Increment){
    auto instance = hel::RoboRIOManager::getInstance();
    frc::Encoder encoder = {0,1,false,frc::Encoder::EncodingType::k1X};
    tEncoder::tOutput e_count = instance.first->encoders[0].getOutput();
    tCounter::tOutput c_count = instance.first->counters[0].getOutput();
    for(unsigned i = 0; i < 100; i++){
        e_count.Value++;
        c_count.Value++;
        instance.first->encoders[0].setOutput(e_count);
        instance.first->counters[0].setOutput(c_count);
        std::cout<<"Hel encoder count: "<<e_count.Value<<" Hel counter count: "<<c_count.Value<<" WPILib count:"<<encoder.Get()<<"\n";
    }
    instance.second.unlock();
    EXPECT_EQ(c_count.Value, encoder.GetRaw());
}
