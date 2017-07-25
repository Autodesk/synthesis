/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include "HAL/Types.h"

#ifdef _WIN32
#include <windows.h>
#define NativeThreadHandle const HANDLE*
#else
#include <pthread.h>
#define NativeThreadHandle const pthread_t*
#endif

extern "C" {
int32_t HAL_GetThreadPriority(NativeThreadHandle handle, HAL_Bool* isRealTime,
                              int32_t* status);
int32_t HAL_GetCurrentThreadPriority(HAL_Bool* isRealTime, int32_t* status);
HAL_Bool HAL_SetThreadPriority(NativeThreadHandle handle, HAL_Bool realTime,
                               int32_t priority, int32_t* status);
HAL_Bool HAL_SetCurrentThreadPriority(HAL_Bool realTime, int32_t priority,
                                      int32_t* status);
}
