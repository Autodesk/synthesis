#include "mira_assembly.h"

namespace SYN {

    MiraAssembly::MiraAssembly(mirabuf::Assembly *assemblyPtr): assemblyPtr(assemblyPtr) { }
    MiraAssembly::~MiraAssembly() {
        delete this->assemblyPtr;
    }

}