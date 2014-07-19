#ifndef __OSAL_H
#define __OSAL_H

#if _WIN32

#define USE_WINAPI 1
#define strdup _strdup
#define fdopen _fdopen

#else

#define USE_POSIX 1
typedef void* LPVOID ;
typedef int DWORD ;
typedef DWORD (*PTHREAD_START_ROUTINE)(LPVOID) ;
#define WINAPI /**/
#define socketclose close

#endif
#endif
