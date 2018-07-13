#include "roborio.h"

std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::sender_instance = nullptr;
std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::execute_instance = nullptr;
std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::reciever_instance = nullptr;

std::mutex hel::RoboRIOManager::m;
