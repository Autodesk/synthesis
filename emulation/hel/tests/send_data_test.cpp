#include "testing.hpp"
#include "send_data.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(SendDataTest, Serialize){
    hel::SendData a = {};
    std::cout<<"Serializaton: "<<a.serializeDeep()<<"\n";

    EXPECT_EQ(true, true); //TODO
}

TEST(SendDataTest, Update){
    tDIO* dio = tDIO::create(nullptr);
    dio->writeOutputEnable_Headers(1u << 3, nullptr);
    dio->writeDO_Headers(1u << 3, nullptr);

    hel::SendData a = {};
    hel::hal_is_initialized.store(true);
    a.updateDeep();
    std::cout<<"Serializaton: "<<a.serializeDeep()<<"\n";
    std::cout<<"SendData::toString: "<<a.toString()<<"\n";

    auto instance = hel::RoboRIOManager::getInstance();
    std::cout<<"RoboRIO digital_hrds[3]="<<hel::checkBitHigh(instance.first->digital_system.getOutputs().Headers,3)<<"\n";
    instance.second.unlock();

    EXPECT_EQ(true, true); //TODO
}

