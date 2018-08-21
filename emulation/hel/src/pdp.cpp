#include "roborio_manager.hpp"

namespace hel{
    PDP::PDP()noexcept{}
    PDP::PDP(const PDP&)noexcept:PDP(){
#define COPY(NAME) NAME = source.NAME
#undef COPY
    }
}
