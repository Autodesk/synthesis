#ifndef _NET_COMM_HPP_
#define _NET_COMM_HPP_

#include <functional>

namespace hel{

    struct NetComm{
        uint32_t ref_num;
        std::function<void(uint32_t)> occurFunction;
        NetComm();
    };
}

#endif
