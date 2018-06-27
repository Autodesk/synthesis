#include "types.h"

int32_t minerva::DigitalPort::maxNegativePwm()const{
	if(eliminateDeadband){
		return deadbandMinPwm;
	}
	return centerPwm - 1;
}

int32_t minerva::DigitalPort::minPositivePwm()const{
	if(eliminateDeadband){
		return deadbandMaxPwm;
	} 
	return centerPwm + 1;
}

int32_t minerva::DigitalPort::positiveScaleFactor()const{
	return maxPwm - minPositivePwm();
}

int32_t minerva::DigitalPort::negativeScaleFactor()const{
	return maxNegativePwm() - minPwm;
}

int32_t minerva::DigitalPort::fullRangeScaleFactor()const{
	return maxPwm - minPwm;
}

#ifdef TYPES_TEST

int main(){
}

#endif
