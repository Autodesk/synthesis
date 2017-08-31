/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "HAL/SerialPort.h"

#include <string>
#include <cstdio>

#include "HAL/cpp/SerialHelper.h"
#include "visa/visa.h"

static int32_t resourceManagerHandle;
static HAL_SerialPort portHandles[4];

extern "C" {

void HAL_InitializeSerialPort(HAL_SerialPort port, int32_t* status) {}

void HAL_SetSerialBaudRate(HAL_SerialPort port, int32_t baud, int32_t* status) {}

void HAL_SetSerialDataBits(HAL_SerialPort port, int32_t bits, int32_t* status) {}

void HAL_SetSerialParity(HAL_SerialPort port, int32_t parity, int32_t* status) {}

void HAL_SetSerialStopBits(HAL_SerialPort port, int32_t stopBits,
                           int32_t* status) {}

void HAL_SetSerialWriteMode(HAL_SerialPort port, int32_t mode,
                            int32_t* status) {}

void HAL_SetSerialFlowControl(HAL_SerialPort port, int32_t flow,
                              int32_t* status) {}

void HAL_SetSerialTimeout(HAL_SerialPort port, double timeout,
                          int32_t* status) {}

void HAL_EnableSerialTermination(HAL_SerialPort port, char terminator,
                                 int32_t* status) {}

void HAL_DisableSerialTermination(HAL_SerialPort port, int32_t* status) {}

void HAL_SetSerialReadBufferSize(HAL_SerialPort port, int32_t size,
                                 int32_t* status) {}

void HAL_SetSerialWriteBufferSize(HAL_SerialPort port, int32_t size,
                                  int32_t* status) {}

int32_t HAL_GetSerialBytesReceived(HAL_SerialPort port, int32_t* status) {
  return 0;
}

int32_t HAL_ReadSerial(HAL_SerialPort port, char* buffer, int32_t count,
                       int32_t* status) {
  return 0;
}

int32_t HAL_WriteSerial(HAL_SerialPort port, const char* buffer, int32_t count,
                        int32_t* status) {
  char* string = (char*)malloc(count + 1);
  memcpy(string, buffer, count);
  string[count] = 0;
  printf(string);
  return count;
}

void HAL_FlushSerial(HAL_SerialPort port, int32_t* status) {
  fflush(stdout);
}

void HAL_ClearSerial(HAL_SerialPort port, int32_t* status) {}

void HAL_CloseSerial(HAL_SerialPort port, int32_t* status) {}

}  // extern "C"
