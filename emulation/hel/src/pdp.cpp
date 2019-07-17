#include "roborio_manager.hpp"

namespace hel{
    std::string PDP::toString()const{
        return "()";
    }

    void PDP::parseCANPacket(const int32_t& /*API_ID*/, const std::vector<uint8_t>& /*DATA*/){
        // assert(DATA.size() == MessageData::SIZE);
    }

    std::vector<uint8_t> PDP::generateCANPacket(const int32_t& /*API_ID*/)const{
        return {};
    }

    PDP::PDP()noexcept{}
    PDP::PDP(const PDP&)noexcept:PDP(){
#define COPY(NAME) NAME = source.NAME
#undef COPY
    }
}
