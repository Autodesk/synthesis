#ifndef __CRC32_H
#define __CRC32_H

#include <stdint.h>
#include <OSAL/OSAL.h>
#if USE_WINAPI
#include <Windows.h>
#endif

DWORD crc32buf(char *buf, size_t len);

#endif;