#ifndef _HEL_TIME_H_
#define _HEL_TIME_H_

// Misc includes to allow HAL to build

#pragma comment(lib, "hel.lib")

#include <algorithm>
#include <cassert>

// sys/time.h

#include <time.h>

/*
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#undef ERROR
*/
#define CLOCK_MONOTONIC 1

using clockid_t=int;

namespace std {
	using time_t = ::time_t;
}

int clock_gettime(clockid_t clk_id, struct timespec *tp);/*{
	LARGE_INTEGER time, frequency;

	bool err = QueryPerformanceCounter(&time);
	assert(err);

	err = QueryPerformanceFrequency(&frequency);
	assert(err);
	double frequencyToMicroseconds = (double)frequency.QuadPart / 1000000.;

    double microseconds = (double)time.QuadPart / frequencyToMicroseconds;
    time.QuadPart = microseconds;
    tp->tv_sec = time.QuadPart / 1000000;
    tp->tv_nsec = time.QuadPart % 1000000;
    return (0);
}*/

#endif
