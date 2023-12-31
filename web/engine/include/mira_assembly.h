#pragma once

#include <assembly.pb.h>

namespace SYN {

    class MiraAssembly {
    public:
        MiraAssembly(mirabuf::Assembly *assemblyPtr);
        MiraAssembly(const MiraAssembly&) = delete;
        ~MiraAssembly();
    private:
        mirabuf::Assembly *assemblyPtr;
    };

}