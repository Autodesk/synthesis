#include "roborio.hpp"
#include "send_data.hpp"
#include "receive_data.hpp"

std::atomic<bool> hel::hal_is_initialized{false};

std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::instance = nullptr;
std::shared_ptr<hel::SendData> hel::SendDataManager::instance = nullptr;
std::shared_ptr<hel::ReceiveData> hel::ReceiveDataManager::instance = nullptr;

std::recursive_mutex hel::RoboRIOManager::m;
std::recursive_mutex hel::SendDataManager::m;
std::recursive_mutex hel::ReceiveDataManager::m;
namespace nFPGA {
    namespace nRoboRIO_FPGANamespace {
        unsigned int g_currentTargetClass;
    }
}
