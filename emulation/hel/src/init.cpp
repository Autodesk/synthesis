#include "roborio_manager.hpp"
#include "robot_inputs.hpp"
#include "robot_outputs.hpp"

#include <cstdio>
#include <fstream>

#define LIBHEL_VERSION "1.1.0" // Major, minor, patch
#define VIRTUAL_MACHINE_INFO_PATH "/home/lvuser/.vminfo"

namespace hel{
    std::atomic<bool> hal_is_initialized{false};

    std::shared_ptr<RoboRIO> RoboRIOManager::instance = nullptr;
    std::shared_ptr<RobotOutputs> RobotOutputsManager::instance = nullptr;
    std::shared_ptr<RobotInputs> RobotInputsManager::instance = nullptr;

    std::recursive_mutex RoboRIOManager::roborio_mutex;
    std::recursive_mutex RobotOutputsManager::send_data_mutex;
    std::recursive_mutex RobotInputsManager::receive_data_mutex;

    void __attribute__((constructor)) printVersionInfo() {
        std::ifstream vm_info;
        vm_info.open(VIRTUAL_MACHINE_INFO_PATH);

        std::string vm_version;
        std::getline(vm_info, vm_version);

        std::string wpilib_version;
        std::getline(vm_info, wpilib_version);

        std::string nilib_version;
        std::getline(vm_info, nilib_version);

        printf("Synthesis Emulation Startup Info: \n\tHEL Version: %s\n\tVirtual Machine Version: %s\n\tWPILib Version: %s\n\tNI Libraries Version: %s\n\n", LIBHEL_VERSION, vm_version.c_str(), wpilib_version.c_str(), nilib_version.c_str());
        RoboRIOManager::getInstance().first->robot_mode.setEnabled(HEL_DEFAULT_ENABLED_STATUS);
    }
    
}

namespace nFPGA {
    namespace nRoboRIO_FPGANamespace {
        unsigned int g_currentTargetClass; //Ni FPGA declares this as extern, so define it here
    }
}
