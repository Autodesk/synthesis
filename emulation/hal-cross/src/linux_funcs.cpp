#include "sys/time.h"
#include "sys/ioctl.h"
#include "sys/prctl.h"
#include "fcntl.h"

#define NOMINMAX
#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#undef ERROR

int open(const char *path, int oflag, ...){
	return 1;
}

int close(int fildes){
	return 0;
}

int ioctl(int fd, unsigned long request, ...){
	return 0;
}

int prctl(int option, ...){
	return 0; // TODO
}

int kill(pid_t pid, int sig){
	return 0;
}

void setlinebuf(FILE *stream){ // From stdio.h
}

int clock_gettime(clockid_t clk_id, struct timespec *tp){
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
}
