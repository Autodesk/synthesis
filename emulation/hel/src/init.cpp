#include "roborio_manager.hpp"
#include "send_data.hpp"
#include "receive_data.hpp"
#include <cstdio>
#include <fstream>

#define LIBHEL_VERSION "1.0"
#define VIRTUAL_MACHINE_INFO_PATH "/home/lvuser/.vminfo"

namespace hel{
    std::atomic<bool> hal_is_initialized{false};

    std::shared_ptr<RoboRIO> RoboRIOManager::instance = nullptr;
    std::shared_ptr<SendData> SendDataManager::instance = nullptr;
    std::shared_ptr<ReceiveData> ReceiveDataManager::instance = nullptr;

    std::recursive_mutex RoboRIOManager::roborio_mutex;
    std::recursive_mutex SendDataManager::send_data_mutex;
    std::recursive_mutex ReceiveDataManager::receive_data_mutex;

    void __attribute__((constructor)) printVersionInfo() {
        std::ifstream vm_info;
        vm_info.open(VIRTUAL_MACHINE_INFO_PATH);

        std::string vm_version;
        std::getline(vm_info, vm_version);

        std::string wpilib_version;
        std::getline(vm_info, wpilib_version);

        printf("Synthesis Emulation Startup Info: \n\tlibhel.so Version: %s\n\tVirtual Machine Version: %s\n\tWPILib Version: %s\n\n", LIBHEL_VERSION, vm_version.c_str(), wpilib_version.c_str());
    }
}
namespace nFPGA {
    namespace nRoboRIO_FPGANamespace {
        unsigned int g_currentTargetClass; //Ni FPGA declares this as extern, so define it here
    }
}
