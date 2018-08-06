#include "roborio_manager.hpp"
#include "send_data.hpp"
#include "receive_data.hpp"

namespace hel{
	std::atomic<bool> hal_is_initialized{false};

	std::shared_ptr<RoboRIO> RoboRIOManager::instance = nullptr;
	std::shared_ptr<SendData> SendDataManager::instance = nullptr;
	std::shared_ptr<ReceiveData> ReceiveDataManager::instance = nullptr;

	std::recursive_mutex RoboRIOManager::roborio_mutex;
	std::recursive_mutex SendDataManager::send_data_mutex;
	std::recursive_mutex ReceiveDataManager::receive_data_mutex;
}
namespace nFPGA {
    namespace nRoboRIO_FPGANamespace {
        unsigned int g_currentTargetClass;
    }
}
