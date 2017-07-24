/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include <jni.h>
#include "HAL/DIO.h"
#include "HALUtil.h"

#include "edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI.h"

using namespace frc;

/*
 * Class:     edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI
 * Method:    setFilterSelect
 */
JNIEXPORT void JNICALL
Java_edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI_setFilterSelect(
    JNIEnv* env, jclass, jint id, jint filter_index) {
  int32_t status = 0;

  HAL_SetFilterSelect(static_cast<HAL_DigitalHandle>(id), filter_index,
                      &status);
  CheckStatus(env, status);
}

/*
 * Class:     edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI
 * Method:    getFilterSelect
 */
JNIEXPORT jint JNICALL
Java_edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI_getFilterSelect(
    JNIEnv* env, jclass, jint id) {
  int32_t status = 0;

  jint result =
      HAL_GetFilterSelect(static_cast<HAL_DigitalHandle>(id), &status);
  CheckStatus(env, status);
  return result;
}

/*
 * Class:     edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI
 * Method:    setFilterPeriod
 */
JNIEXPORT void JNICALL
Java_edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI_setFilterPeriod(
    JNIEnv* env, jclass, jint filter_index, jint fpga_cycles) {
  int32_t status = 0;

  HAL_SetFilterPeriod(filter_index, fpga_cycles, &status);
  CheckStatus(env, status);
}

/*
 * Class:     edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI
 * Method:    getFilterPeriod
 */
JNIEXPORT jint JNICALL
Java_edu_wpi_first_wpilibj_hal_DigitalGlitchFilterJNI_getFilterPeriod(
    JNIEnv* env, jclass, jint filter_index) {
  int32_t status = 0;

  jint result = HAL_GetFilterPeriod(filter_index, &status);
  CheckStatus(env, status);
  return result;
}
