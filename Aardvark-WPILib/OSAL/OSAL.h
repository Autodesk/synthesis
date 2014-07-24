#ifndef __OSAL_H
#define __OSAL_H

#if _WIN32

#define USE_WINAPI 1
#define strdup _strdup
#define fdopen _fdopen

#define __LITTLE_ENDIAN	1

#else

#define __BIG_ENDIAN	1

#define USE_POSIX 1
typedef void* LPVOID ;
typedef int DWORD ;
typedef DWORD (*PTHREAD_START_ROUTINE)(LPVOID) ;
#define WINAPI /**/
#define closesocket close
#define min(a,b) ((a<b) ? (a) : (b))

#endif
#endif
