#ifndef _HEL_TIME_H_
#define _HEL_TIME_H_

// Misc includes to allow HAL to build

#pragma comment(lib, "hel.lib")
#pragma comment(lib, "libwinpthread-1.lib")

#include <algorithm>
#include <cassert>

// sys/time.h

#include <time.h>

#define CLOCK_MONOTONIC 1

using clockid_t = int;

namespace std {
	using time_t = ::time_t;
}

int clock_gettime(clockid_t clk_id, struct timespec* tp);

#endif
