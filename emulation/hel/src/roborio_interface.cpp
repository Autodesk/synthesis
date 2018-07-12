#include "roborio_interface.h"

#include "HAL/HAL.h"

void hel::RoboRIOInterface::update(){
    int32_t status = 0;

    for(unsigned i = 0; i < pwm_hdrs.size(); i++){
        pwm_hdrs[i] = HAL_GetPWMSpeed(i, &status);
    }
}
