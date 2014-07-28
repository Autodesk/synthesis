#ifndef __OSAL_H
#define __OSAL_H

#ifndef __LITTLE_ENDIAND
#ifndef __BIG_ENDIAN
#define __LITTLE_ENDIAN	1
#endif
#endif

#if _WIN32

#define NOMINMAX
#define USE_WINAPI 1
#define strdup _strdup
#define fdopen _fdopen

#else

#define USE_POSIX 1
typedef void* LPVOID ;
typedef unsigned int DWORD ;
typedef int SOCKET;
typedef DWORD (*PTHREAD_START_ROUTINE)(LPVOID) ;
#define WINAPI /**/
#define closesocket close
#define SOCKET_ERROR (-1)
#define INFINITE 0xFFFFFFFF

#include <unistd.h>
inline void Sleep(unsigned int ms) { usleep(ms * 1000); }

#endif
#endif
