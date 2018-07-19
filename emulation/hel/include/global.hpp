#ifndef _GLOBAL_HPP_
#define _GLOBAL_HPP_

namespace hel{

    struct Global{
    private:
        uint64_t fpga_start_time;

    public:
        uint64_t getFPGAStartTime()const;
        static uint64_t getCurrentTime();
        Global();
    };
}

#endif
