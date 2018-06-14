#pragma once
#include "Vector3.h"

class Physics {
public:
	Physics();
	Physics(Vector3*);
	~Physics();			//For COM
private:
	Vector3 * COM;		//center of mass - note: single point mass and not a list
};