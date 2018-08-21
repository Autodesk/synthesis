#include "system_interface.hpp"
#include "error.hpp"

using namespace nFPGA;

namespace hel{
    const uint16_t SystemInterface::getExpectedFPGAVersion(){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::getExpectedFPGAVersion\n";
        return 0;
    }

    const uint32_t SystemInterface::getExpectedFPGARevision(){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::getExpectedFPGARevision\n";
        return 0;
    }

    const uint32_t* const SystemInterface::getExpectedFPGASignature(){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::getExpectedFPGASignature\n";
        static const uint32_t i = 0;
        return &i;
    }

    void SystemInterface::getHardwareFpgaSignature(uint32_t* /*guid_ptr*/, tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::getHardwareFpgaSignature\n";
    }

    uint32_t SystemInterface::getLVHandle(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::getLVHandle\n";
        return 0;
    }

    uint32_t SystemInterface::getHandle(){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::getHandle\n";
        return 0;
    }

    void SystemInterface::reset(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::reset\n";
    }

    void SystemInterface::getDmaDescriptor(int /*dmaChannelDescriptorIndex*/, tDMAChannelDescriptor* /*desc*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::getDmaDescriptor\n";
    }
}

