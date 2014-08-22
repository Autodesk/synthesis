/*
 * System.cpp
 *
 *  Created on: Sep 26, 2012
 *      Author: Mitchell Wills
 */

#include "OSAL/System.h"
#include <stdio.h>
#include <stdint.h>
#include "OSAL/OSAL.h"

#if USE_WINAPI
#include <Windows.h>
#elif USE_POSIX
#include <sys/time.h>
#include <time.h>
#include <unistd.h>
#endif

extern "C" {

void sleep_ms(unsigned long ms) {
#if USE_WINAPI
	Sleep(ms);
#elif USE_POSIX
	usleep(ms * 1000);
#endif
}

unsigned long currentTimeMillis() {
#if USE_WINAPI
	FILETIME ft_now;
	GetSystemTimeAsFileTime(&ft_now);
	uint64_t ll_now = (LONGLONG)ft_now.dwLowDateTime + ((LONGLONG)(ft_now.dwHighDateTime) << 32LL);
	return (unsigned long)(ll_now / 10000);
#elif USE_POSIX
	timeval tt;
	gettimeofday(&tt, NULL);
	return (unsigned long) (tt.tv_sec * 1000L) + (long) (tt.tv_usec / 1000L);
#endif
}

unsigned long threadTimeMicros() {
#if USE_WINAPI
	static uint64_t threadBaseTime = 0;
	FILETIME ft_now;
	GetSystemTimeAsFileTime(&ft_now);
	uint64_t ll_now = (LONGLONG)ft_now.dwLowDateTime + ((LONGLONG)(ft_now.dwHighDateTime) << 32LL);
	if (threadBaseTime == 0) {
		threadBaseTime = ll_now;
	}
	return (unsigned long)((ll_now-threadBaseTime) / 10);
#elif USE_POSIX
	static __time_t threadBaseTime = 0;
	timespec tt;
	clock_gettime(CLOCK_MONOTONIC, &tt);
	if (threadBaseTime == 0) {
		threadBaseTime = tt.tv_sec;
	}
	return (unsigned long) ((tt.tv_sec - threadBaseTime) * 1000000UL)
			+ (long) (tt.tv_nsec / 1000);
#endif
}

void writeWarning(const char* message) {
	fprintf(stderr, "%s\n", message);
	fflush(stderr);
	//TODO implement write warning with wpilib error stuff
}

}
