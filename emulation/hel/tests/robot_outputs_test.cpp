#include "testing.hpp"
#include "robot_outputs.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(RobotOutputsTest, Serialize){
    EXPECT_TRUE(true); //TODO
}

TEST(RobotOutputsTest, Update){
    // tDIO* dio = tDIO::create(nullptr);
    // dio->writeOutputEnable_Headers(1u << 3, nullptr);
    // dio->writeDO_Headers(1u << 3, nullptr);

    // auto roborio_instance = hel::RoboRIOManager::getInstance();
    // roborio_instance.first->robot_mode.setEnabled(true);
    // auto send_data_instance = hel::RobotOutputsManager::getInstance();
    // hel::hal_is_initialized.store(true);
    // send_data_instance.first->updateDeep();
    // std::cout<<"Serializaton: "<<send_data_instance.first->serializeDeep()<<"\n";
    // std::cout<<"RobotOutputs::toString: "<<send_data_instance.first->toString()<<"\n";

    // std::cout<<"RoboRIO digital_hrds[3]="<<hel::checkBitHigh(roborio_instance.first->digital_system.getOutputs().Headers,3)<<"\n";
    // roborio_instance.second.unlock();
    // send_data_instance.second.unlock();

    EXPECT_TRUE(true); //TODO
}

