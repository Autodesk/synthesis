#include "system_interface.hpp"
#include "error.hpp"

using namespace nFPGA;

namespace hel{
    const uint16_t SystemInterface::getExpectedFPGAVersion(){
        hel::warnUnsupportedFeature("Function call tSystem::getExpectedFPGAVersion");
        return 0;
    }

    const uint32_t SystemInterface::getExpectedFPGARevision(){
        hel::warnUnsupportedFeature("Function call tSystem::getExpectedFPGARevision");
        return 0;
    }

    const uint32_t* const SystemInterface::getExpectedFPGASignature(){
        hel::warnUnsupportedFeature("Function call tSystem::getExpectedFPGASignature");
        static const uint32_t i = 0;
        return &i;
    }

    void SystemInterface::getHardwareFpgaSignature(uint32_t* /*guid_ptr*/, tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tSystem::getHardwareFpgaSignature");
    }

    uint32_t SystemInterface::getLVHandle(tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tSystem::getLVHandle");
        return 0;
    }

    uint32_t SystemInterface::getHandle(){
        hel::warnUnsupportedFeature("Function call tSystem::getHandle");
        return 0;
    }

    void SystemInterface::reset(tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tSystem::reset");
    }

    void SystemInterface::getDmaDescriptor(int /*dmaChannelDescriptorIndex*/, tDMAChannelDescriptor* /*desc*/){
        hel::warnUnsupportedFeature("Function call tSystem::getDmaDescriptor");
    }
}

