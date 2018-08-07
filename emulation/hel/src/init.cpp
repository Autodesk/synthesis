#include "roborio_manager.hpp"
#include "send_data.hpp"
#include "receive_data.hpp"
#include <cstdio>
#include <fstream>
#define LIBHEL_VERSION "1.0"
#define VIRTUAL_MACHINE_INFO_PATH "/home/synthesis/.vminfo"

std::atomic<bool> hel::hal_is_initialized{false};

std::shared_ptr<hel::RoboRIO> hel::RoboRIOManager::instance = nullptr;
std::shared_ptr<hel::SendData> hel::SendDataManager::instance = nullptr;
std::shared_ptr<hel::ReceiveData> hel::ReceiveDataManager::instance = nullptr;

std::recursive_mutex hel::RoboRIOManager::m;
std::recursive_mutex hel::SendDataManager::m;
std::recursive_mutex hel::ReceiveDataManager::m;

void __attribute__((constructor)) printVersionInfo() {

    std::ifstream vm_info;
    vm_info.open(VIRTUAL_MACHINE_INFO_PATH);

    std::string vm_version;
    std::getline(vm_info, vm_version);

    std::string wpilib_version;
    std::getline(vm_info, wpilib_version);

    printf("Synthesis Emulation Startup Info: \n\n\tlibhel.so Version: %s\n\tVirtual Machine Version: %s\n\tWPILib Version: %s\n", LIBHEL_VERSION, vm_version.c_str(), wpilib_version.c_str());

}

namespace nFPGA {
    namespace nRoboRIO_FPGANamespace {
        unsigned int g_currentTargetClass;
    }
}
