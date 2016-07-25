#ifndef _POTENTIOMETER_H
#define _POTENTIOMETER_H

#include "PIDSource.h"

class Potentiometer : public PIDSource {
public:
	virtual double Get() = 0;
};

#endif
