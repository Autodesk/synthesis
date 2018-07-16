#include "roborio.h"
#include "send_data.h"


std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::instance = nullptr;
std::shared_ptr<hel::SendData> hel::SendDataManager::instance = nullptr;

std::recursive_mutex hel::RoboRIOManager::m;
std::recursive_mutex hel::SendDataManager::m;

std::condition_variable hel::SendDataManager::cv;

