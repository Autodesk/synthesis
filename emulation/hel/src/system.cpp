#include "system.hpp"

#include "error.hpp"

namespace nFPGA{
    tSystem::tSystem(tRioStatusCode* /*status*/){}

    tSystem::~tSystem(){}

    void tSystem::getFpgaGuid(uint32_t* /*guid_ptr*/, tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tSystem::getFpgaGuid");
    }

    void tSystem::reset(tRioStatusCode* /*status*/){
        hel::warnUnsupportedFeature("Function call tSystem::reset");
    }

    void tSystem::NiFpga_SharedOpen_common(const char* /*bitfile*/){
        hel::warnUnsupportedFeature("Function call tSystem::NiFpga_SharedOpen_common");
    }

    NiFpga_Status tSystem::NiFpga_SharedOpen(const char* /*bitfile*/, const char* /*signature*/, const char* /*resource*/, uint32_t /*attribute*/, NiFpga_Session* /*session*/){
        hel::warnUnsupportedFeature("Function call tSystem::NiFpga_SharedOpen");
        return NiFpga_Status_Success;
    }

    NiFpga_Status tSystem::NiFpgaLv_SharedOpen(const char* const /*bitfile*/, const char* const /*apiSignature*/, const char* const /*resource*/, const uint32_t /*attribute*/, NiFpga_Session* const /*session*/){
        hel::warnUnsupportedFeature("Function call tSystem::NiFpgaLv_SharedOpen");
        return NiFpga_Status_Success;
    }
}
