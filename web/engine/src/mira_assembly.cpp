#include "mira_assembly.h"

namespace SYN {

    namespace MIRA {

        Assembly::Assembly(mirabuf::Assembly *assemblyPtr): assemblyPtr(assemblyPtr) { }
        Assembly::~Assembly() {
            delete this->assemblyPtr;
        }

        inline std::string Assembly::GetName() {
            return this->assemblyPtr->info().name();
        }

    }

}