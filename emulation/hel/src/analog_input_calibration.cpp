#include "analog_inputs.hpp"

#include "FRC_NetworkCommunication/AICalibration.h"


uint32_t FRC_NetworkCommunication_nAICalibration_getLSBWeight(const uint32_t /*aiSystemIndex*/, const uint32_t /*channel*/, int32_t* /*status*/){
    return hel::AnalogInputs::LSB_WEIGHT;
}
int32_t FRC_NetworkCommunication_nAICalibration_getOffset(const uint32_t /*aiSystemIndex*/, const uint32_t /*channel*/, int32_t* /*status*/){
    return hel::AnalogInputs::OFFSET;
}
