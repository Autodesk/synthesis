#include "system.hpp"

#include "error.hpp"

namespace nFPGA{
    tSystem::tSystem(tRioStatusCode* /*status*/){}

    tSystem::~tSystem(){}

    void tSystem::getFpgaGuid(uint32_t* /*guid_ptr*/, tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::getFpgaGuid\n";
    }

    void tSystem::reset(tRioStatusCode* /*status*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::reset\n";
    }

    void tSystem::NiFpga_SharedOpen_common(const char* /*bitfile*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::NiFpga_SharedOpen_common\n";
    }

    NiFpga_Status tSystem::NiFpga_SharedOpen(const char* /*bitfile*/, const char* /*signature*/, const char* /*resource*/, uint32_t /*attribute*/, NiFpga_Session* /*session*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::NiFpga_SharedOpen\n";
        return NiFpga_Status_Success;
    }

    NiFpga_Status tSystem::NiFpgaLv_SharedOpen(const char* const /*bitfile*/, const char* const /*apiSignature*/, const char* const /*resource*/, const uint32_t /*attribute*/, NiFpga_Session* const /*session*/){
        std::cerr<<"Synthesis warning: Unsupported feature: Function call tSystem::NiFpgaLv_SharedOpen\n";
        return NiFpga_Status_Success;
    }
}
