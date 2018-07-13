#include "roborio.h"
#include "send_data.h"


std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::sender_instance = nullptr;
std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::execute_instance = nullptr;
std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::reciever_instance = nullptr;

std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::current_instance = nullptr;

std::shared_ptr<hel::SendData> hel::SendDataManager::instance = nullptr;

std::recursive_mutex hel::RoboRIOManager::m;
